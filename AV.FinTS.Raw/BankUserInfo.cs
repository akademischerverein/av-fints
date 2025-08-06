using AV.FinTS.Raw.Codes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Raw
{
    public class BankUserInfo
    {
        public static string CUSTOMER_ANONYMOUS = "9999999999";

        private string? customerId;

        public string UserId { get; set; } = null!;

        public string CustomerId { get => customerId == null ? UserId : customerId; set { customerId = value; } }

        public string CustomerSystemId { get; set; } = "0";

        public Language SelectedLanguage { get; set; } = Language.GERMAN;

        public CountryCode CountryCode { get; set; } = CountryCode.WEST_GERMANY;

        public string Blz { get; set; } = null!;

        public bool CustomerIdSet => customerId != null;

        public static BankUserInfo CreateAnonymous(string blz, CountryCode cc=CountryCode.WEST_GERMANY)
        {
            return new BankUserInfo
            {
                UserId = "dummy",
                CustomerId = CUSTOMER_ANONYMOUS,
                CountryCode = cc,
                Blz = blz,
            };
        }
    }
}
