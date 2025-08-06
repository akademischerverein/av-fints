using AV.FinTS.Raw.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Raw.Segments.ParameterData
{
    public class HIPINS1 : ISegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HIPINS", Version = 1 };

        public int MaxOrders { get; set; }

        public int MinSignatures { get; set; }

        public int SecurityClass { get; set; }

        public int? MinPinLength { get; set; }

        public int? MaxPinLength { get; set; }

        public int? MaxTanLength { get; set; }

        public string? NameUserId { get; set; }

        public string? NameCustomerId { get; set; }

        public Dictionary<string, bool> PinTanInfo { get; set; } = new();

        public static ISegment Read(MessageReader reader, SegmentId segmentId)
        {
            var param = new HIPINS1
            {
                Head = segmentId,
                MaxOrders = (int)reader.ReadInt()!,
                MinSignatures = (int)reader.ReadInt()!,
                SecurityClass = (int)reader.ReadInt()!,
            };
            reader.EnterGroup();
            param.MinPinLength = reader.ReadInt();
            param.MaxPinLength = reader.ReadInt();
            param.MaxTanLength = reader.ReadInt();
            param.NameUserId = reader.Read();
            param.NameCustomerId = reader.Read();
            reader.EnterGroup();

            while(!reader.SegmentEnded)
            {
                var segment = (string)reader.Read()!;
                var tanRequired = (bool)reader.ReadBool()!;

                param.PinTanInfo.Add(segment, tanRequired);
            }

            reader.LeaveGroup();
            reader.LeaveGroup();

            return param;
        }

        public void Write(MessageWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
