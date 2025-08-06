using AV.FinTS.Raw.Security;
using AV.FinTS.Raw.Segments.Internal;
using AV.FinTS.Raw.Structures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Raw
{
    public class Dialog
    {
        private class EmptySP : ISecurityProvider
        {
            public RawMessage Encrypt(RawMessage orginalMessage, BankUserInfo userInfo) => orginalMessage;
            public void SignPrepare(RawMessage message, BankUserInfo userInfo) {}

            public void Sign(RawMessage message) {}

            public RawMessage Decrypt(RawMessage orginalMessage) => orginalMessage;

            public bool Verify(RawMessage message) => true;
        }

        private static HttpClient http = new HttpClient();
        private static string ProductId = "Dummy";
        private static string ProductVersion = "1.0";

        public static void SetProduct(string productId, string productVersion)
        {
            ProductId = productId;
            ProductVersion = productVersion;
        }

        private int messageId = 1;

        private readonly string endpoint;
        private readonly BankUserInfo userInfo;
        private readonly ISecurityProvider securityProvider;
        private string dialogId = "0";
        private bool isClosed = false;
        
        public bool IsOpen { get; private set; }

        public ISecurityProvider SecurityProvider => securityProvider;

        public IReadOnlyDictionary<int, RawMessage> CustomerMessages { get; private set; } = new Dictionary<int, RawMessage>();

        public IReadOnlyDictionary<int, RawMessage> BankMessages { get; private set; } = new Dictionary<int, RawMessage>();

        public Dialog(string endpoint, BankUserInfo userInfo, ISecurityProvider? securityProvider=null)
        {
            this.endpoint = endpoint;
            this.userInfo = userInfo;
            IsOpen = false;
            if (securityProvider != null)
            {
                this.securityProvider = securityProvider;
            } else
            {
                this.securityProvider = new EmptySP();
            }
        }

        public async Task<RawMessage> Init(int bpd, int upd, IReadOnlyCollection<ISegment> additionals)
        {
            if(isClosed)
            {
                throw new InvalidOperationException("Dialog has already ended");
            }

            if (IsOpen)
            {
                throw new InvalidOperationException("Dialog is already open");
            }

            IsOpen = true;
            var resp = await SendMessage([
                new HKIDN2 { Bank = new BankIdentifier { CountryCode = userInfo.CountryCode, BankId = userInfo.Blz }, CustomerId = userInfo.CustomerId, CustomerSystemId = userInfo.CustomerSystemId, CustomerSystemIdRequired = userInfo.CustomerId != BankUserInfo.CUSTOMER_ANONYMOUS },
                new HKVVB3 { BPD = bpd, UPD = upd, Language = userInfo.SelectedLanguage, Product = ProductId, ProductVersion = ProductVersion },
                ..additionals
            ]);
            return resp;
        }

        public async Task<RawMessage> End(IReadOnlyCollection<ISegment> additionals)
        {
            if (isClosed)
            {
                throw new InvalidOperationException("Dialog has already ended");
            }

            if (!IsOpen)
            {
                throw new InvalidOperationException("Dialog is not open");
            }

            var resp =  await SendMessage([
                new HKEND1 { DialogId = dialogId },
                ..additionals
            ]);
            IsOpen = false;
            isClosed = true;
            return resp;
        }

        /*public static async Task<string> GenerateSystemId(string endpoint, BankUserInfo userInfo, ISecurityProvider securityProvider)
        {
            var dialog = new Dialog(endpoint, userInfo, securityProvider);
            var resp = await dialog.Init(0, 0, [new HKSYN3 { Mode = SynchonizationMode.NEW_CUSTOMER_SYSTEM_ID }]);
            await dialog.End([]);

            var hisyn = resp.Segments.First(seg => seg.Head.Name == "HISYN" && seg.Head.Version == 4) as HISYN4;

            return hisyn!.CustomerSystemId;
        }*/

        private void NumerateSegments(List<ISegment> segments)
        {
            for(int i = 0; i < segments.Count; i++)
            {
                segments[i].Head.Number = i + 1;
            }
        }

        public async Task<RawMessage> SendMessage(IReadOnlyCollection<ISegment> origSegments)
        {
            if (isClosed)
            {
                throw new InvalidOperationException("Dialog has already ended");
            }

            if (!IsOpen)
            {
                throw new InvalidOperationException("Dialog is not open");
            }

            var thisMsgNum = messageId++;

            var segments = new List<ISegment>(origSegments);
            segments.Insert(0, new HNHBK3 { Length = 0, DialogId = dialogId, MessageNumber = thisMsgNum });
            segments.Add(new HNHBS1 { MessageNumber = thisMsgNum });
            var customerMessage = new RawMessage { Segments = segments };
            NumerateSegments((List<ISegment>)customerMessage.Segments);
            securityProvider.SignPrepare(customerMessage, userInfo);
            NumerateSegments((List<ISegment>)customerMessage.Segments);
            securityProvider.Sign(customerMessage);
            ((Dictionary<int, RawMessage>)CustomerMessages).Add(thisMsgNum, customerMessage);
            customerMessage = securityProvider.Encrypt(customerMessage, userInfo);
            var response = await Send(customerMessage);
            var msg = new RawMessage
            {
                RawResponse = response,
                RawDecrypted = response
            };
            msg.Segments = MessageParser.ParseMessage(response);
            var responseMessage = securityProvider.Decrypt(msg);
            if (!securityProvider.Verify(responseMessage))
            {
                throw new InvalidDataException("Couldn't verify response message");
            }
            ((Dictionary<int, RawMessage>)BankMessages).Add((responseMessage.Segments[0] as HNHBK3)!.MessageNumber, responseMessage);
            return responseMessage;
        }

        private async Task<byte[]> Send(RawMessage mesg)
        {
            Debug.Assert(IsOpen);

            var headWriter = new MessageWriter();
            var writer = new MessageWriter();
            var head = (HNHBK3)mesg.Segments[0];
            headWriter.Write(head);

            foreach (var segment in mesg.Segments.Skip(1))
            {
                writer.Write(segment);
            }

            head.Length = writer.ToBytes().Length + headWriter.ToBytes().Length;
            headWriter.Clear();
            headWriter.Write(head);

            var headMsg = headWriter.ToBytes();
            var bodyMsg = writer.ToBytes();

            var msg = new byte[headMsg.Length + bodyMsg.Length];
            Array.Copy(headMsg, msg, headMsg.Length);
            Array.Copy(bodyMsg, 0, msg, headMsg.Length, bodyMsg.Length);

            var response = await http.PostAsync(endpoint, new StringContent(Convert.ToBase64String(msg)));
            response.EnsureSuccessStatusCode();
            var b64 = await response.Content.ReadAsStringAsync();
            var bytes = Convert.FromBase64String(b64);

            var reader = new MessageReader(bytes);
            var segment_id = reader.ReadSegmentHeader();

            if (segment_id.Name != "HNHBK" || segment_id.Version != 3 || segment_id.Number != 1)
            {
                throw new InvalidOperationException("Invalid Message header");
            }
            var header = (HNHBK3)HNHBK3.Read(reader, segment_id);
            if (dialogId == "0")
            {
                dialogId = header.DialogId;
            }

            if (header.Length != bytes.Length)
            {
                throw new InvalidDataException("length missmatch");
            }

            return bytes;
        }
    }

    public class RawMessage
    {
        public IReadOnlyList<ISegment> Segments { get; set; } = new List<ISegment>();

        public byte[] RawResponse { get; set; } = Array.Empty<byte>();

        public byte[] RawDecrypted { get; set; } = Array.Empty<byte>();
    }
}
