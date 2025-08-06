using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Raw.Security
{
    public interface ISecurityProvider
    {
        public RawMessage Encrypt(RawMessage orginalMessage, BankUserInfo userInfo);

        public RawMessage Decrypt(RawMessage orginalMessage);

        public void SignPrepare(RawMessage message, BankUserInfo userInfo);

        public void Sign(RawMessage message);

        public bool Verify(RawMessage message);
    }
}
