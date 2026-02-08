using AV.FinTS.Exceptions;
using AV.FinTS.Formats;
using AV.FinTS.Helper;
using AV.FinTS.Helpers;
using AV.FinTS.Models;
using AV.FinTS.Parameter;
using AV.FinTS.Parameters;
using AV.FinTS.Raw;
using AV.FinTS.Raw.Codes;
using AV.FinTS.Raw.Security;
using AV.FinTS.Raw.Segments.Auth;
using AV.FinTS.Raw.Segments.Feedback;
using AV.FinTS.Raw.Segments.Internal;
using AV.FinTS.Raw.Segments.ParameterData;
using AV.FinTS.Raw.Segments.Sepa;
using AV.FinTS.Raw.Segments.Transactions;
using AV.FinTS.Raw.Structures;
using AV.FinTS.Security;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using static System.ArgumentNullException;

namespace AV.FinTS
{
    public class PinTanClient
    {
        private char[] _pin = [];
        private readonly BankUserInfo _userInfo;
        private readonly string _endpoint;
        private readonly Func<AuthMethod, TanRequest, Task<TanResponse>> _authHandler;
        private Upd _upd = null!;

        public PinTanClient(string endpoint, string bankId, Func<AuthMethod, TanRequest, Task<TanResponse>> authHandler, CountryCode cc = CountryCode.WEST_GERMANY, FeedbackLogHandler? handler=null)
        {
            _endpoint = endpoint;
            _authHandler = authHandler;
            _userInfo = new()
            {
                Blz = bankId,
                CountryCode = cc,
                SelectedLanguage = Language.GERMAN
            };
            if (handler != null)
            {
                FeedbackLog += handler;
            }

            var anon = BankUserInfo.CreateAnonymous(bankId);
            var anonDialog = new Dialog(endpoint, anon);
            var resp = anonDialog.Init(0, 0, []);

            resp.Wait();
            LogMessage(resp.Result).Wait();
            Bpd = Bpd.FromMessage(resp.Result);

            var closeMsg = anonDialog.End([]);
            closeMsg.Wait();
            LogMessage(closeMsg.Result).Wait();
        }

        public delegate Task FeedbackLogHandler(object sender, FeedbackLogArgs args);

        public event FeedbackLogHandler? FeedbackLog;

        private async Task LogMessage(RawMessage msg)
        {
            if (FeedbackLog == null) return;

            var feedbacks = msg.GetAll<HIRMG2>().Cast<ISegment>().Concat(msg.GetAll<HIRMS2>()).SelectMany(seg => seg.GetPropertyNotNull<List<Feedback>>("Feedbacks"));

            foreach(var f in feedbacks)
            {
                var args = new FeedbackLogArgs
                {
                    Code = f.Code,
                    Text = f.Text,
                    Parameters = [.. f.Parameters.Where(s => !string.IsNullOrEmpty(s)).Select(sn => (string)sn!)]
                };
                await FeedbackLog.Invoke(this, args);
            }
        }

        public async Task Login(string userId, char[] pin, string? customerId=null, string? systemId=null)
        {
            if (_userInfo.UserId != null)
            {
                throw new InvalidOperationException("login data already set");
            }

            _userInfo.UserId = userId;
            if (customerId != null)
            {
                _userInfo.CustomerId = customerId;
            }

            if (systemId != null)
            {
                _userInfo.CustomerSystemId = systemId;
            }

            var diag = new Dialog(_endpoint, _userInfo, new PinTanSecurity(999, pin));
            var addSegs = new List<ISegment>();

            if (systemId == null)
            {
                addSegs.Add(new HKSYN3 { Mode = SynchonizationMode.NEW_CUSTOMER_SYSTEM_ID });
            }
            var initMsg = await diag.Init(Bpd.Version, 0, addSegs);
            await LogMessage(initMsg);
            foreach (var seg in initMsg.GetAll<HIRMG2>())
            {
                foreach(var fb in seg.Feedbacks)
                {
                    if (fb.Code >= 9000)
                    {
                        throw new FinTSAuthException("Encountered errors while logging in. Is PIN correct?");
                    }
                }
            }
            foreach (var seg in initMsg.GetAll<HIRMS2>())
            {
                foreach (var fb in seg.Feedbacks)
                {
                    if (fb.Code >= 9000)
                    {
                        throw new FinTSAuthException("Encountered errors while logging in. Is PIN correct?");
                    }
                }
            }
            var allowedProcedures = initMsg.GetAll<HIRMS2>().SelectMany(x => x.Feedbacks).Where(x => x.Code == 3920).First();
            AvailableAuthenticationMethods = allowedProcedures.Parameters.Where(x => x is not null).Select(x => Bpd.AuthenticationMethods.Where(m => m.SecurityFunction == int.Parse(x!)).First()).ToList();
            await LogMessage(await diag.End([]));

            if (systemId == null)
            {
                var syn = initMsg.Get<HISYN4>();
                _userInfo.CustomerSystemId = syn.CustomerSystemId;
            }

            _pin = pin;
        }

        private async Task<Dialog> OpenDialog(string usage="HKIDN")
        {
            var dialog = new Dialog(_endpoint, _userInfo, new PinTanSecurity(SelectedAuthenticationMethod.SecurityFunction, _pin));

            var tan = GenTanRequest(segment: usage);
            var initMsg = await dialog.Init(Bpd.Version, 0, [tan]);
            await LogMessage(initMsg);

            var errors = initMsg.GetAll<HIRMG2>().SelectMany(g => g.Feedbacks).Where(fb => fb.Code >= 9000).Concat(
                        initMsg.GetAll<HIRMS2>().SelectMany(s => s.Feedbacks).Where(fb => fb.Code >= 9000));

            if (errors.Count() > 0)
            {
                throw new FinTSDialogInitException(string.Join("\n", errors.Select(fb => fb.Code.ToString("D4") + ": " + fb.Text)));
            }

            if (initMsg.Segments.Any(seg => seg.Head.IsBpd))
            {
                Bpd = Bpd.FromMessage(initMsg);
            }

            var completedAuthMsg = await ProcessTanResponse(dialog, initMsg);

            if (completedAuthMsg.Segments.Any(seg => seg.Head.Name == "HIUPA" || seg.Head.Name == "HIUPD"))
            {
                _upd = Upd.FromMessage(completedAuthMsg);
            }
            if (initMsg.Segments.Any(seg => seg.Head.IsBpd))
            {
                Bpd = Bpd.FromMessage(initMsg);
            }

            return dialog;
        }

        private async Task<RawMessage> ProcessTanResponseDecoupled(Dialog dialog, RawMessage input)
        {
            if (SelectedAuthenticationMethod.Version < 7)
            {
                throw new InvalidOperationException("Decoupled is only available starting with version 7");
            }

            var message = input;
            HIRMS2? tanFb;
            var hitan = input.Segments.Where(seg => seg.Head.Name == "HITAN").First();
            var followhi = hitan;

            int statusRequests = 0;
            do
            {
                if (statusRequests >= SelectedAuthenticationMethod.Decoupled!.MaximumStatusRequests && SelectedAuthenticationMethod.Decoupled!.MaximumStatusRequests != 0)
                {
                    throw new FinTSAuthStatusRequestsExceededException();
                }

                var challenge = followhi.GetProperty<string>("Challenge");
                if (challenge == "nochallenge")
                {
                    challenge = null;
                }

                var tanResp = await _authHandler(SelectedAuthenticationMethod, new TanRequest
                {
                    Challenge = challenge,
                    ChallengeHhdUc = followhi.GetPropertyNotNull<byte[]>("ChallengeHhdUc"),
                    Reference = hitan.GetPropertyNotNull<string>("Reference"),
                    ValidUpTo = followhi.GetPropertyValue<DateTime>("ValidUpTo"),
                    TanMedium = followhi.GetProperty<string>("Reference")
                });
                tanResp.Verify(true);

                var tan = GenTanRequest(process: "S", reference: hitan.GetPropertyNotNull<string>("Reference"), tanFollows: false);
                message = await dialog.SendMessage([tan]);
                await LogMessage(message);
                statusRequests++;
                var errors = message.GetAll<HIRMG2>().SelectMany(seg => seg.Feedbacks).Concat(message.GetAll<HIRMS2>().Where(seg => seg.Head.SegmentReference == tan.Head.Number).SelectMany(seg => seg.Feedbacks)).Where(fb => fb.Code >= 9000);
                if (errors.Count() > 0)
                {
                    throw new FinTSAuthException("Encountered errors while sending TAN", errors.ToList());
                }
                followhi = message.Segments.Where(seg => seg.Head.Name == "HITAN").First();
                tanFb = message.GetAll<HIRMS2>().Where(seg => seg.Head.SegmentReference == tan.Head.Number).FirstOrDefault();
            } while (tanFb != null && tanFb.Feedbacks.Any(fb => fb.Code == 3956));

            return message;
        }

        private async Task<RawMessage> ProcessTanResponse(Dialog dialog, RawMessage input)
        {
            var tanSeg = dialog.CustomerMessages.OrderByDescending(kv => kv.Key).First().Value.Segments.Where(seg => seg.Head.Name == "HKTAN").First();

            var tanFb = input.GetAll<HIRMS2>().Where(seg => seg.Head.SegmentReference == tanSeg.Head.Number).First();

            if (tanFb.Feedbacks.Any(fb => fb.Code == 3076))
            {
                // SCA not required
                return input;
            }

            if (SelectedAuthenticationMethod.TechnicalSpecName == AuthMethod.Spec.Decoupled)
            {
                return await ProcessTanResponseDecoupled(dialog, input);
            }
            else if (SelectedAuthenticationMethod.TechnicalSpecName == AuthMethod.Spec.DecoupledPush)
            {
                throw new NotImplementedException();
            }
            else if (SelectedAuthenticationMethod.Version < 6)
            {
                throw new NotSupportedException();
            }
            else
            {
                var hitan = input.Segments.Where(seg => seg.Head.Name == "HITAN").First();

                var tanResp = await _authHandler(SelectedAuthenticationMethod, new TanRequest
                {
                    Challenge = hitan.GetProperty<string>("Challenge"),
                    ChallengeHhdUc = hitan.GetPropertyNotNull<byte[]>("ChallengeHhdUc"),
                    Reference = hitan.GetPropertyNotNull<string>("Reference"),
                    ValidUpTo = hitan.GetPropertyValue<DateTime>("ValidUpTo"),
                    TanMedium = hitan.GetProperty<string>("Reference")
                });
                tanResp.Verify(true);

                var tan = GenTanRequest(process: "2", reference: hitan.GetPropertyNotNull<string>("Reference"), tanFollows: false);
                ((PinTanSecurity)dialog.SecurityProvider).Tan = tanResp.Tan;
                var response = await dialog.SendMessage([tan]);
                await LogMessage(response);
                var errors = response.GetAll<HIRMG2>().SelectMany(seg => seg.Feedbacks).Concat(response.GetAll<HIRMS2>().Where(seg => seg.Head.SegmentReference == tan.Head.Number).SelectMany(seg => seg.Feedbacks)).Where(fb => fb.Code >= 9000);
                if (errors.Count() > 0)
                {
                    throw new FinTSAuthException("Encountered errors while sending TAN", errors.ToList());
                }

                return response;
            }
        }

        private ISegment GenTanRequest(AuthMethod? auth = null, string? segment = null, string process = "4", string? reference = null, bool? tanFollows = null)
        {
            if (auth == null)
            {
                auth = SelectedAuthenticationMethod;
            }

            if (auth.Process != 2)
            {
                throw new NotImplementedException("TAN process " + auth.Process.ToString() + " not implemented");
            }

            if (auth.Version == 7)
            {
                var tan7 = new HKTAN7
                {
                    Process = process,
                    SegmentName = segment,
                    TanMedium = SelectedTanMedium?.Name,
                    Reference = reference,
                    FutureTanFollowUp = tanFollows
                };

                return tan7;
            }
            else if (auth.Version == 6)
            {
                var tan6 = new HKTAN6
                {
                    Process = process,
                    SegmentName = segment,
                    TanMedium = SelectedTanMedium?.Name,
                    Reference = reference,
                    FutureTanFollowUp = tanFollows
                };

                return tan6;
            } else
            {
                throw new NotImplementedException(auth.TechnicalName);
            }
        }

        public string CustomerSystemId => _userInfo.CustomerSystemId;

        public IReadOnlyCollection<AuthMethod> AvailableAuthenticationMethods { get; private set; } = null!;

        public AuthMethod SelectedAuthenticationMethod { get; set; } = null!;

        public TanMedium? SelectedTanMedium { get; set; }

        public Bpd Bpd { get; private set; }

        private static NullabilityInfoContext nullCtx = new NullabilityInfoContext();
        private ISegment CreateSegment(SegmentId paramHead, object obj, params Type[] segTypes)
        {
            ISegment retSeg = null!;

            foreach(var segType in segTypes)
            {
                var seg = (ISegment)segType.GetConstructor(Type.EmptyTypes)!.Invoke([]);
                if ($"{seg.Head.Name[0]}I{seg.Head.Name.Substring(2, 3)}S" == paramHead.Name && seg.Head.Version == paramHead.Version)
                {
                    retSeg = seg;
                    break;
                }
            }

            var objType = obj.GetType();

            foreach (var prop in retSeg.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                if (prop.GetSetMethod() == null || prop.Name == "Head")
                {
                    continue;
                }

                var objProp = objType.GetProperty(prop.Name);
                if (objProp == null)
                {
                    if (Nullable.GetUnderlyingType(prop.PropertyType) != null || nullCtx.Create(prop).WriteState is NullabilityState.Nullable)
                    {
                        continue;
                    }

                    throw new ArgumentException($"Missing {prop.Name} property");
                }
                else if (objProp.PropertyType.IsAssignableTo(prop.PropertyType) ||
                    (Nullable.GetUnderlyingType(prop.PropertyType) != null && objProp.PropertyType.IsAssignableTo(Nullable.GetUnderlyingType(prop.PropertyType))))
                {
                    prop.SetValue(retSeg, objProp.GetValue(obj));
                }
                else if (objProp.PropertyType.IsAssignableTo(typeof(IMultiValue)))
                {
                    var multiVal = (IMultiValue?)objProp.GetValue(obj);
                    if (multiVal == null)
                    {
                        if (Nullable.GetUnderlyingType(prop.PropertyType) != null || nullCtx.Create(prop).WriteState is NullabilityState.Nullable)
                        {
                            continue;
                        } else
                        {
                            throw new ArgumentException($"{prop.Name} may not be null");
                        }
                    }

                    if (!multiVal.CanBeConvertedTo(prop.PropertyType))
                    {
                        throw new ArgumentException($"Invalid {prop.Name} property");
                    }
                    prop.SetValue(retSeg, multiVal.ConvertTo(prop.PropertyType));
                }
                else
                {
                    throw new ArgumentException($"Invalid {prop.Name} property");
                }
            }

            return retSeg;
        }

        private bool IsAccountSpecific(string segName)
        {
            switch(segName)
            {
                case "HKKAZ":
                case "HKSAL":
                case "HKEKA":
                case "HKKAU":
                case "HKKIF":
                case "NULL":
                    return true;
            }

            return false;
        }

        private async Task<RawMessage> Send(Dialog dialog, List<ISegment> segments, SepaAccount? account=null)
        {
            if(!segments.All(seg => Bpd.PinTanInfo.ContainsKey(seg.Head.Name)))
            {
                await LogMessage(await dialog.End([]));
                throw new OperationNotSupported();
            }

            var accountlessSegments = segments.Where(seg => !IsAccountSpecific(seg.Head.Name)).Select(seg => seg.Head.Name).ToList();
            var accountSegments = segments.Where(seg => IsAccountSpecific(seg.Head.Name)).Select(seg  => seg.Head.Name).ToList();
            var accountTanSegments = accountSegments.Where(seg => Bpd.PinTanInfo[seg]).ToList();

            var tanRequired = Bpd.PinTanInfo.Where(kv => accountlessSegments.Contains(kv.Key)).Any(kv => kv.Value);

            var upd = (HIUPD6?)null;
            if (account != null)
            {
                upd = _upd.GetForAccount(account);
                if (upd == null)
                {
                    await LogMessage(await dialog.End([]));
                    throw new OperationNotSupported();
                }

                var ops = upd.AllowedOperations.Where(op => accountTanSegments.Contains(op.Operation)).ToList();
                if (ops.Any(op => op.NumRequiredSignatures > 1) || accountTanSegments.Distinct().Count() != ops.Count)
                {
                    await LogMessage(await dialog.End([]));
                    throw new OperationNotSupported();
                }

                tanRequired = tanRequired || ops.Any(op => op.NumRequiredSignatures == 1);
            } else if(accountSegments.Count() > 0)
            {
                await LogMessage(await dialog.End([]));
                throw new ArgumentNullException(nameof(accountSegments));
            }

            RawMessage respMessage;
            if (tanRequired)
            {
                respMessage = await dialog.SendMessage([..segments, GenTanRequest(segment: segments[0].Head.Name)]);
                await LogMessage(respMessage);
                var interErrors = respMessage.GetAll<HIRMG2>().SelectMany(seg => seg.Feedbacks).Concat(respMessage.GetAll<HIRMS2>().SelectMany(seg => seg.Feedbacks)).Where(fb => fb.Code >= 9000);
                if (interErrors.Count() > 0)
                {
                    throw new FinTSAuthException("Bank responded with errors", interErrors.ToList());
                }
                respMessage = await ProcessTanResponse(dialog, respMessage);
            }
            else
            {
                respMessage = await dialog.SendMessage(segments);
                await LogMessage(respMessage);
            }

            var errors = respMessage.GetAll<HIRMG2>().SelectMany(seg => seg.Feedbacks).Concat(respMessage.GetAll<HIRMS2>().SelectMany(seg => seg.Feedbacks)).Where(fb => fb.Code >= 9000);
            if (errors.Count() > 0)
            {
                throw new FinTSAuthException("Bank responded with errors", errors.ToList());
            }

            return respMessage;
        }

        public async Task<Upd> RequestUpd()
        {
            var diag = await OpenDialog();
            await LogMessage(await diag.End([]));

            return _upd;
        }

        public async Task<TanMediumResponse> GetTanMedia(TanMediumType type = TanMediumType.All, TanMediumClass mediumClass = TanMediumClass.ALL)
        {
            var best = Bpd.GetBestParameterRequired(typeof(HITABS4));
            var seg = CreateSegment(best.Head, new
            {
                Type = type,
                Class = mediumClass
            }, typeof(HKTAB4));

            Dialog diag;
            if (SelectedTanMedium == null && SelectedAuthenticationMethod.MediumRequired)
            {
                diag = await OpenDialog("HKTAB");
            } else
            {
                diag = await OpenDialog();
            }

            var respMessage = await Send(diag, [seg]);
            await LogMessage(await diag.End([]));

            var hiseg = respMessage.Get($"{seg.Head.Name[0]}I{seg.Head.Name.Substring(2)}", seg.Head.Version);
            var media = hiseg.GetPropertyNotNull<List<HITAB4.TanMediumElement>>("Media");
            var usage = hiseg.GetPropertyNotNull<TanUsageOption>("UsageOption");

            var resp = new TanMediumResponse
            {
                UsageOption = usage
            };

            foreach(var m in media)
            {
                var medium = new TanMedium
                {
                    Name = m.TanMediumName,
                    Class = m.Class,
                    State = m.State,
                    LastUsage = m.LastUsage,
                    ActivatedOn = m.ActivatedOn
                };
                resp.TanMediums.Add(medium);
            }

            return resp;
        }

        public async Task<IReadOnlyCollection<SepaAccount>> GetAccounts()
        {
            var best = Bpd.GetBestParameterRequired(typeof(HISPAS2), typeof(HISPAS1));

            var seg = CreateSegment(best.Head, new
            {
            }, typeof(HKSPA2), typeof(HKSPA1));

            var diag = await OpenDialog();

            var respMessage = await Send(diag, [seg]);

            var hiseg = respMessage.Get($"{seg.Head.Name[0]}I{seg.Head.Name.Substring(2)}", seg.Head.Version);
            var accounts = hiseg.GetPropertyNotNull<List<AccountInternationalSepa>>("Accounts");

            var sepa_accounts = new List<SepaAccount>();
            foreach(var acc in accounts)
            {
                if (!acc.IsSepa) continue;

                var sepa = new SepaAccount(acc);
                if (sepa.NationalSet)
                {
                    var upd = _upd.Segments.Where(seg => seg.Head.Name == "HIUPD" && seg.Head.Version == 6).Select(seg => (HIUPD6)seg).Where(
                        seg => seg.Account != null && seg.Account.AccountNumber == sepa.AccountNumber && seg.Account.SubAccountNumber == sepa.SubAccountNumber && seg.Account.BankInfo.BankId == sepa.Blz && seg.Account.BankInfo.CountryCode == sepa.cc).FirstOrDefault();
                    if (upd != null)
                    {
                        sepa.AccountProductName = upd.AccountProductName;
                        sepa.AccountHolder = upd.AccountHolder;
                        sepa.Currency = upd.Currency;
                    }
                }

                sepa_accounts.Add(sepa);
            }
            await LogMessage(await diag.End([]));
            return sepa_accounts;
        }

        public async Task<TransactionsResponse> GetTransactions(SepaAccount account, DateOnly from, DateOnly to)
        {
            ThrowIfNull(account, nameof(account));
            ThrowIfNull(from, nameof(from));
            ThrowIfNull(to, nameof(to));
            if (from > to)
            {
                throw new ArgumentException("from must before or equal to to");
            }
            if(from > DateOnly.FromDateTime(DateTime.Now) || to > DateOnly.FromDateTime(DateTime.Now))
            {
                throw new ArgumentException("can't request bookings in the future");
            }
            var best = Bpd.GetBestParameterRequired(typeof(HIKAZS7), typeof(HIKAZS6), typeof(HIKAZS5), typeof(HIKAZS4));
            if (best.GetPropertyNotNull<int>("StorageDuration") < (DateTime.Now - from.ToDateTime(TimeOnly.MinValue)).TotalDays)
            {
                throw new ArgumentException("transactions are only stored for " + best.GetPropertyNotNull<int>("StorageDuration") + " days");
            }

            var seg = CreateSegment(best.Head, new
            {
                AllAccounts = false,
                From = from,
                To = to,
                Account = account
            }, typeof(HKKAZ7), typeof(HKKAZ6), typeof(HKKAZ5), typeof(HKKAZ4));

            var diag = await OpenDialog();

            var respMessage = await Send(diag, [seg], account);

            await LogMessage(await diag.End([]));

            var datasets = respMessage.Segments.Where(seg => seg.Head.Name == "HIKAZ").Select(seg => seg.GetPropertyNotNull<byte[]>("BookedTransactions")).Where(b => b.Length > 0).SelectMany(MT940.Parse).ToList();

            var response = new TransactionsResponse
            {
                Account = account,
                From = from,
                To = to,
            };

            if (datasets.Count > 0)
            {
                var ourStmt = new SwiftStatement
                {
                    AccountCode = datasets[0].AccountCode,
                    BankCode = datasets[0].BankCode,
                    Currency = datasets[0].Currency,
                    Number = datasets[0].Number,
                    PageNumber = datasets[0].PageNumber,
                    OpeningBalance = datasets[0].OpeningBalance,
                    OpeningDate = datasets[0].OpeningDate,
                    OrderReference = datasets[0].OrderReference,
                    Referencing = datasets[0].Referencing,
                };
                var lastStmt = datasets.Last();
                ourStmt.ClosingBalance = lastStmt.ClosingBalance;
                ourStmt.ClosingDate = lastStmt.ClosingDate;
                ourStmt.Transactions = datasets.SelectMany(t => t.Transactions).ToList();
                response.BookedTransactions = ourStmt;
            }
            return response;
        }

        public async Task GetStatement(SepaAccount account, AccountStatementFormat? format=null, int? number=null, int? year=null)
        {
            ThrowIfNull(account, nameof(account));
            var best = Bpd.GetBestParameterRequired(typeof(HIEKAS5), typeof(HIEKAS4), typeof(HIEKAS3));
            var spas = Bpd.GetBestParameterRequired(typeof(HISPAS2), typeof(HISPAS1));

            if ((number != null || year != 0) && !best.GetPropertyNotNull<bool>("NumberAllowed"))
            {
                throw new ArgumentException("number and year not allowed");
            }
            var acc_copy = account.Clone();

            if (best.Head.Version > 3 && !spas.GetPropertyNotNull<bool>("NationalAccountNumberAllowed"))
            {
                acc_copy.NationalSet = false;
            } else if (best.Head.Version < 4 && !acc_copy.NationalSet)
            {
                throw new ArgumentException("need to provide national account number", nameof(account));
            }

            if (format != null && !best.GetPropertyNotNull<List<AccountStatementFormat>>("SupportedFormats").Contains((AccountStatementFormat)format))
            {
                throw new ArgumentException("Format not supported by bank", nameof(format));
            }

            var seg = CreateSegment(best.Head, new
            {
                Account = account,
                Format = format,
                Number = number,
                Year = year
            }, typeof(HKEKA5), typeof(HKEKA4), typeof(HKEKA3));

            var diag = await OpenDialog();
            var respMessage = await Send(diag, [seg], account);

            var hiseg = respMessage.Get("HIEKA");
            await LogMessage(await diag.End([]));
        }
    }

    public class FeedbackLogArgs
    {
        public int Code { get; internal set; }

        public string Text { get; internal set; } = null!;

        public List<string> Parameters { get; internal set; } = null!;

        public bool IsError => Code >= 9000 && Code <= 9999;

        public bool IsWarning => Code >= 3000 && Code <= 3999;

        public bool IsSuccess => Code >= 0 && Code <= 999;

        internal FeedbackLogArgs() { }
    }
}
