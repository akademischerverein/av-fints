using AV.FinTS.Helper;
using AV.FinTS.Models;
using AV.FinTS.Raw;
using AV.FinTS.Raw.Segments.ParameterData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Parameters
{
    public class Upd
    {
        public IReadOnlyCollection<ISegment> Segments { get; private set; } = null!;

        private Upd() { }

        internal HIUPD6? GetForAccount(SepaAccount account)
        {
            return Segments.Where(seg => seg.Head.Name == "HIUPD" && seg.Head.Version == 6).Select(seg => (HIUPD6)seg).Where(
                        seg => seg.Account != null && seg.Account.AccountNumber == account.AccountNumber && seg.Account.SubAccountNumber == account.SubAccountNumber &&
                        seg.Account.BankInfo.BankId == account.Blz && seg.Account.BankInfo.CountryCode == account.cc).FirstOrDefault();
        }

        public static Upd FromMessage(RawMessage msg)
        {
            return new Upd
            {
                Segments = msg.GetAll<HIUPA4>().Select(seg => (ISegment)seg).Concat(msg.GetAll<HIUPD6>()).ToList(),
            };
        }
    }
}
