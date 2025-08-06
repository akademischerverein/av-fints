using AV.FinTS.Raw.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Raw.Segments.ParameterData
{
    public class HIUPD6 : ISegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HIUPD", Version = 6 };

        public Account? Account { get; set; }

        public string? Iban { get; set; }

        public string CustomerId { get; set; } = null!;

        public int? AccountType { get; set; }

        public string? Currency { get; set; }

        public string AccountHolder { get; set; } = null!;

        public string? AccountProductName { get; set; }

        public AccountLimit? AccountLimit { get; set; }

        public List<AllowedOperation> AllowedOperations { get; set; } = new();

        public string? Extension { get; set; }

        public static ISegment Read(MessageReader reader, SegmentId segmentId)
        {
            var upd = new HIUPD6
            {
                Head = segmentId,
                Account = Account.Read(reader),
                Iban = reader.Read(),
                CustomerId = reader.Read()!,
                AccountType = reader.ReadInt(),
                Currency = reader.Read(),
                AccountHolder = reader.Read()!
            };
            var name2 = reader.Read();
            if (!string.IsNullOrEmpty(name2))
            {
                upd.AccountHolder += " " + name2;
            }
            upd.AccountProductName = reader.Read();
            upd.AccountLimit = AccountLimit.Read(reader);
            for (int i = 0; i < 999; i++)
            {
                var op = AllowedOperation.Read(reader);
                if (op != null)
                {
                    upd.AllowedOperations.Add(op);
                }
            }
            upd.Extension = reader.Read();
            return upd;
        }

        public void Write(MessageWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
