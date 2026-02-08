using AV.FinTS.Raw.Codes;
using AV.FinTS.Raw.Segments.ParameterData;
using AV.FinTS.Raw.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Models
{
    public class SepaAccount : IMultiValue
    {
        public string Iban { get; private set; } = null!;

        public string? Bic { get; private set; }

        public string? AccountNumber { get; private set; }

        public string? SubAccountNumber { get; private set; }

        public string? Blz { get; private set; }

        internal CountryCode? cc { get; set; }

        public string? AccountHolder { get; set; }

        public string? AccountProductName { get; set; }

        public string? Currency { get; set; }

        internal bool NationalSet { get; set; } = false;

        internal SepaAccount(AccountInternationalSepa acc)
        {
            if (acc.Iban == null || acc.Bic == null) { throw new ArgumentNullException(); }

            Iban = acc.Iban;
            Bic = acc.Bic;
            if (acc.AccountNumber != null)
            {
                AccountNumber = acc.AccountNumber;
                SubAccountNumber = acc.SubAccountNumber;
                Blz = acc.BankInfo!.BankId;
                cc = acc.BankInfo.CountryCode;
                NationalSet = true;
            }
        }

        internal SepaAccount(HIUPD6 upd)
        {
            if (upd.Account == null || upd.Iban == null) { throw new ArgumentNullException(); }
            AccountNumber = upd.Account.AccountNumber;
            SubAccountNumber = upd.Account.SubAccountNumber;
            Blz = upd.Account.BankInfo.BankId;
            cc = upd.Account.BankInfo.CountryCode;
            NationalSet = true;

            Iban = upd.Iban;
            AccountHolder = upd.AccountHolder;
            AccountProductName = upd.AccountProductName;
            Currency = upd.Currency;
        }

        private SepaAccount() { }

        bool IMultiValue.CanBeConvertedTo(Type type)
        {
            if(type == typeof(AccountLegacy) || type == typeof(Account)) return NationalSet;
            if (type == typeof(AccountInternational) || type == typeof(AccountInternationalSepa)) return true;

            return false;
        }

        object IMultiValue.ConvertTo(Type type)
        {
            var thisObj = (IMultiValue)this;
            if (!thisObj.CanBeConvertedTo(type))
            {
                throw new ArgumentException("cant convert SepaAccount to " + type.Name);
            }

            if (type == typeof(AccountLegacy))
            {
                return new AccountLegacy
                {
                    AccountNumber = AccountNumber!,
                    BankInfo = new BankIdentifier { CountryCode = (CountryCode)cc!, BankId = Blz },
                };
            } else if (type == typeof(Account))
            {
                return new Account
                {
                    AccountNumber = AccountNumber!,
                    SubAccountNumber = SubAccountNumber,
                    BankInfo = new BankIdentifier { BankId = Blz, CountryCode = (CountryCode)cc! },
                };
            } else if (type == typeof(AccountInternational))
            {
                var acc = new AccountInternational
                {
                    Iban = Iban,
                    Bic = Bic
                };
                if (NationalSet)
                {
                    acc.AccountNumber = AccountNumber;
                    acc.SubAccountNumber = SubAccountNumber;
                    acc.BankInfo = new BankIdentifier
                    {
                        CountryCode = (CountryCode)cc!,
                        BankId = Blz
                    };
                }
                return acc;
            }
            else if (type == typeof(AccountInternationalSepa))
            {
                var acc = new AccountInternationalSepa
                {
                    Iban = Iban,
                    Bic = Bic,
                    IsSepa = true
                };
                if (NationalSet)
                {
                    acc.AccountNumber = AccountNumber;
                    acc.SubAccountNumber = SubAccountNumber;
                    acc.BankInfo = new BankIdentifier
                    {
                        CountryCode = (CountryCode)cc!,
                        BankId = Blz
                    };
                }
                return acc;
            }

            throw new NotImplementedException();
        }

        internal SepaAccount Clone()
        {
            return new SepaAccount
            {
                Iban = Iban,
                Bic = Bic,
                AccountNumber = AccountNumber,
                SubAccountNumber = SubAccountNumber,
                Blz = Blz,
                cc = cc,
                AccountHolder = AccountHolder,
                AccountProductName = AccountProductName,
                Currency = Currency,
                NationalSet = NationalSet,
            };
        }
    }
}
