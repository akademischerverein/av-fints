using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Raw.Structures
{
    public class Account : IElementStructure
    {
        public string AccountNumber { get; set; } = null!;

        public string? SubAccountNumber { get; set; } = null!;

        public BankIdentifier BankInfo { get; set; } = null!;

        public static Account? Read(MessageReader reader)
        {
            reader.EnterGroup();
            var accStr = reader.Read();
            if (accStr == null)
            {
                reader.Read();
                reader.Read();
                reader.Read();
                reader.LeaveGroup();
                return null;
            }

            var a = new Account
            {
                AccountNumber = accStr!,
                SubAccountNumber = reader.Read(),
                BankInfo = BankIdentifier.Read(reader)!
            };
            reader.LeaveGroup();
            return a;
        }

        public void Write(MessageWriter writer)
        {
            writer.Write(AccountNumber, FieldType.IDENTIFICATION);
            if (SubAccountNumber != null)
            {
                writer.Write(SubAccountNumber, FieldType.IDENTIFICATION);
            }
            else
            {
                writer.WriteEmpty();
            }
            writer.Write(BankInfo);
        }
    }

    public class AccountLegacy : IElementStructure
    {
        public string AccountNumber { get; set; } = null!;

        public BankIdentifier BankInfo { get; set; } = null!;

        public static AccountLegacy? Read(MessageReader reader)
        {
            reader.EnterGroup();
            var accStr = reader.Read();
            if (accStr == null)
            {
                reader.Read();
                reader.Read();
                reader.LeaveGroup();
                return null;
            }

            var a = new AccountLegacy
            {
                AccountNumber = accStr!,
                BankInfo = BankIdentifier.Read(reader)!
            };
            reader.LeaveGroup();
            return a;
        }

        public void Write(MessageWriter writer)
        {
            writer.Write(AccountNumber, FieldType.IDENTIFICATION);
            writer.Write(BankInfo);
        }
    }

    public class AccountInternational : IElementStructure
    {
        public string? Iban { get; set; }

        public string? Bic { get; set; }

        public string? AccountNumber { get; set; } = null!;

        public string? SubAccountNumber { get; set; } = null!;

        public BankIdentifier? BankInfo { get; set; } = null!;

        public static AccountInternational? Read(MessageReader reader)
        {
            reader.EnterGroup();
            var a = new AccountInternational
            {
                Iban = reader.Read(),
                Bic = reader.Read(),
                AccountNumber = reader.Read(),
                SubAccountNumber = reader.Read()
            };
            if (a.AccountNumber != null)
            {
                a.BankInfo = BankIdentifier.Read(reader);
            } else
            {
                reader.Read();
            }
            reader.LeaveGroup();
            if (a.Iban == null && a.AccountNumber == null)
            {
                return null;
            }
            return a;
        }

        public void Write(MessageWriter writer)
        {
            if (Iban != null)
            {
                writer.Write(Iban, FieldType.ALPHA_NUMERIC, max_length: 34);
            }
            else
            {
                writer.WriteEmpty();
            }
            if (Bic != null)
            {
                writer.Write(Bic, FieldType.ALPHA_NUMERIC, max_length: 11);
            }
            else
            {
                writer.WriteEmpty();
            }
            if (AccountNumber != null)
            {
                writer.Write(AccountNumber, FieldType.IDENTIFICATION);
            } else
            {
                writer.WriteEmpty();
            }
            if (SubAccountNumber != null)
            {
                writer.Write(SubAccountNumber, FieldType.IDENTIFICATION);
            }
            else
            {
                writer.WriteEmpty();
            }
            if (BankInfo != null)
            {
                writer.Write(BankInfo);
            } else
            {
                writer.WriteEmpty();
            }
        }
    }

    public class AccountInternationalSepa : IElementStructure
    {
        public bool IsSepa { get; set; }

        public string? Iban { get; set; }

        public string? Bic { get; set; }

        public string? AccountNumber { get; set; } = null!;

        public string? SubAccountNumber { get; set; } = null!;

        public BankIdentifier? BankInfo { get; set; } = null!;

        public static AccountInternationalSepa? Read(MessageReader reader)
        {
            reader.EnterGroup();
            var sepa = reader.ReadBool();
            if (sepa == null)
            {
                reader.Read();
                reader.Read();
                reader.Read();
                reader.Read();
                reader.Read();
                reader.Read();
                reader.LeaveGroup();
                return null;
            }
            var a = new AccountInternationalSepa
            {
                IsSepa = (bool)sepa!,
                Iban = reader.Read(),
                Bic = reader.Read(),
                AccountNumber = reader.Read()!,
                SubAccountNumber = reader.Read()
            };
            if (a.AccountNumber != null)
            {
                a.BankInfo = BankIdentifier.Read(reader);
            }
            reader.LeaveGroup();
            return a;
        }

        public void Write(MessageWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
