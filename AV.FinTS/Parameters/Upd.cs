using AV.FinTS.Helper;
using AV.FinTS.Models;
using AV.FinTS.Raw;
using AV.FinTS.Raw.Segments.ParameterData;
using AV.FinTS.Raw.Structures;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AV.FinTS.Parameters
{
    public class Upd
    {
        public IReadOnlyCollection<ISegment> Segments { get; private set; } = null!;

        public string User { get; private set; } = null!;

        public string? Extension { get; private set; }

        public bool NonListedOperationsDisallowed { get; private set; }

        public IReadOnlyDictionary<SepaAccount, string> AccountExtensions { get; private set; } = null!;

        public IReadOnlyDictionary<SepaAccount, AccountLimit> AccountLimits { get; private set; } = null!;

        public IReadOnlyDictionary<SepaAccount, IReadOnlyCollection<AllowedOperation>> AccountOperations { get; private set; } = null!;

        public IReadOnlyDictionary<SepaAccount, IReadOnlyCollection<AccountFeature>> AccountFeatures { get; private set; } = null!;

        private Upd() { }

        internal HIUPD6? GetForAccount(SepaAccount account)
        {
            return Segments.Where(seg => seg.Head.Name == "HIUPD" && seg.Head.Version == 6).Select(seg => (HIUPD6)seg).Where(
                        seg => seg.Account != null && seg.Account.AccountNumber == account.AccountNumber && seg.Account.SubAccountNumber == account.SubAccountNumber &&
                        seg.Account.BankInfo.BankId == account.Blz && seg.Account.BankInfo.CountryCode == account.cc).FirstOrDefault();
        }

        public static Upd FromMessage(RawMessage msg)
        {
            var hiupa = msg.Get<HIUPA4>();
            var hiupds = msg.GetAll<HIUPD6>().Select(upd => new KeyValuePair<SepaAccount, HIUPD6>(new SepaAccount(upd), upd));

            var accFeatures = new Dictionary<SepaAccount, IReadOnlyCollection<AccountFeature>>();
            foreach(var kv in hiupds)
            {
                var support = new HashSet<AccountFeature>();
                foreach(var op in kv.Value.AllowedOperations)
                {
                    // cam-Formats currently not supported
                    switch(op.Operation)
                    {
                        case "HKSPA":
                            support.Add(AccountFeature.BankDetails);
                            break;
                        //case "HKCAZ":
                        case "HKKAZ":
                            support.Add(AccountFeature.Transactions);
                            break;
                        case "HKCUB":
                            support.Add(AccountFeature.RequestReceiverAccounts);
                            break;
                        case "HKSAL":
                            support.Add(AccountFeature.Balance);
                            break;
                        case "HKEKA":
                        //case "HKECA":
                        case "HKEKP":
                            support.Add(AccountFeature.Statement);
                            break;
                    }
                }
                accFeatures.Add(kv.Key, support);
            }

            var upd = new Upd
            {
                Segments = msg.GetAll<HIUPA4>().Select(seg => (ISegment)seg).Concat(msg.GetAll<HIUPD6>()).ToList(),
                User = hiupa.Username ?? hiupa.UserId,
                NonListedOperationsDisallowed = hiupa.NonListedOperationsDisallowed,
                Extension = hiupa.Extension,
                AccountExtensions = hiupds.Where(kv => kv.Value.Extension != null).Select(kv => new KeyValuePair<SepaAccount, string>(kv.Key, kv.Value.Extension!)).ToDictionary(),
                AccountLimits = hiupds.Where(kv => kv.Value.AccountLimit != null).Select(kv => new KeyValuePair<SepaAccount, AccountLimit>(kv.Key, kv.Value.AccountLimit!)).ToDictionary(),
                AccountOperations = hiupds.Select(kv => new KeyValuePair<SepaAccount, IReadOnlyCollection<AllowedOperation>>(kv.Key, kv.Value.AllowedOperations)).ToDictionary(),
                AccountFeatures = accFeatures,
            };

            return upd;
        }

        public enum AccountFeature
        {
            BankDetails,
            Transactions,
            Statement,
            RequestReceiverAccounts,
            Balance
        }
    }
}
