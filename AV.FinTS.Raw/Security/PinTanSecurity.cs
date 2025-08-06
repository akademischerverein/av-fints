using AV.FinTS.Raw.Segments.Auth;
using AV.FinTS.Raw.Segments.Internal;
using AV.FinTS.Raw.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HashAlgorithm = AV.FinTS.Raw.Structures.HashAlgorithm;

namespace AV.FinTS.Raw.Security
{
    public class PinTanSecurity : ISecurityProvider
    {
        private readonly int securityFunction;
        private readonly char[] pinStr;
        private string? _tan;

        public PinTanSecurity(int securityFunction, char[] pinStr)
        {
            this.securityFunction = securityFunction;
            this.pinStr = pinStr;
        }

        public void ClearTan()
        {
            _tan = null;
        }

        public string Tan { set => _tan = value; }

        public int SecurityFunction => securityFunction;

        public RawMessage Encrypt(RawMessage orginalMessage, BankUserInfo userInfo)
        {
            var writer = new MessageWriter();
            foreach (var segment in orginalMessage.Segments.Skip(1).SkipLast(1))
            {
                writer.Write(segment);
            }
            var encHead = new HNVSK3
            {
                SecurityProfile = new SecurityProfile { Procedure = SecurityProcedure.PIN, ProcedureVersion = securityFunction == 999 ? SecurityProcedureVersion.PINTAN_ONE_STEP : SecurityProcedureVersion.PINTAN_TWO_STEP },
                SecurityFunction = 998,
                VendorRole = VendorRole.ISS,
                SecurityId = new SecurityId { SecurityParty = SecurityParty.MESSAGE_SENDER, PartyId = userInfo.CustomerSystemId },
                SecurityDateTime = new SecurityDateTime { CodeIdentifier = SecurityDateTime.Identifier.SECURITY_TIMESTAMP, Date = DateOnly.FromDateTime(DateTime.Now), Time = TimeOnly.FromDateTime(DateTime.Now) },
                KeyName = new KeyName { Bank = new BankIdentifier { CountryCode = userInfo.CountryCode, BankId = userInfo.Blz }, UserId = userInfo.UserId, KeyType = KeyName.Type.V_CIPHER_KEY }
            };
            encHead.Head.Number = 998;
            var encData = new HNVSD1 { Data = writer.ToBytes() };
            encData.Head.Number = 999;

            return new RawMessage
            {
                Segments = [
                    orginalMessage.Segments[0],
                    encHead,
                    encData,
                    orginalMessage.Segments[orginalMessage.Segments.Count - 1]
                ]
            };
        }

        public void SignPrepare(RawMessage message, BankUserInfo userInfo)
        {
            var strReference = RandomNumberGenerator.GetString("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 12);
            var intReference = RandomNumberGenerator.GetInt32(int.MaxValue);

            var signHead = new HNSHK4
            {
                SecurityProfile = new SecurityProfile { Procedure = SecurityProcedure.PIN, ProcedureVersion = securityFunction == 999 ? SecurityProcedureVersion.PINTAN_ONE_STEP : SecurityProcedureVersion.PINTAN_TWO_STEP },
                SecurityFunction = securityFunction,
                SecurityReference = strReference,
                SignatureApplication = SignatureApplication.SHM,
                VendorRole = VendorRole.ISS,
                SecurityId = new SecurityId { SecurityParty = SecurityParty.MESSAGE_SENDER, PartyId = userInfo.CustomerSystemId },
                SecurityReferenceNumber = intReference,
                SecurityDateTime = new SecurityDateTime { CodeIdentifier = SecurityDateTime.Identifier.SECURITY_TIMESTAMP, Date = DateOnly.FromDateTime(DateTime.Now), Time = TimeOnly.FromDateTime(DateTime.Now) },
                HashAlgorithm = new HashAlgorithm { CodedAlgorithm = HashAlgorithm.Code.MUTUALLY_AGREED },
                KeyName = new KeyName { Bank = new BankIdentifier { CountryCode = userInfo.CountryCode, BankId = userInfo.Blz }, UserId = userInfo.UserId, KeyType = KeyName.Type.S_SIGNATURE_KEY }
            };
            var signTail = new HNSHA2 { SecurityReference = strReference, Signature = new UserSignature { PIN = pinStr, TAN = _tan } };
            ((List<ISegment>)message.Segments).Insert(1, signHead);
            ((List<ISegment>)message.Segments).Insert(message.Segments.Count - 1, signTail);
        }

        public void Sign(RawMessage message)
        {
            _tan = null;
        }

        public RawMessage Decrypt(RawMessage orginalMessage)
        {
            if (orginalMessage.Segments.Count != 4 || orginalMessage.Segments[1].Head.Number != 998 || orginalMessage.Segments[2].Head.Number != 999)
            {
                return orginalMessage;
            }

            if (orginalMessage.Segments[2] is not HNVSD1)
            {
                return orginalMessage;
            }
            var encData = orginalMessage.Segments[2] as HNVSD1;
            var decrypted = MessageParser.ParseMessage(encData!.Data);

            var encryptedMessage = new List<ISegment>();
            encryptedMessage.Add(orginalMessage.Segments[0]);
            encryptedMessage.AddRange(decrypted);
            encryptedMessage.Add(orginalMessage.Segments[3]);

            return new RawMessage
            {
                Segments = encryptedMessage,
                RawResponse = orginalMessage.RawResponse,
                RawDecrypted = encData!.Data
            };
        }

        public bool Verify(RawMessage message) => true;
    }
}
