using AV.FinTS.Raw.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Raw.Segments.Internal
{
    public class HNHBK3 : ISegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HNHBK", Version = 3 };

        public int Length { get; set; }

        public int HBCI_Version { get; private set; } = 300;

        public string DialogId { get; set; } = null!;

        public int MessageNumber { get; set; }

        public static ISegment Read(MessageReader reader, SegmentId segmentId)
        {
            var head = new HNHBK3();
            head.Length = (int)reader.ReadInt()!;
            head.HBCI_Version = (int)reader.ReadInt()!;
            head.DialogId = reader.Read()!;
            head.MessageNumber = (int)reader.ReadInt()!;
            reader.EnterGroup();
            reader.Read();
            reader.ReadInt();
            reader.LeaveGroup();
            head.Head = segmentId;
            return head;
        }

        public void Write(MessageWriter writer)
        {
            writer.Write(Length, FieldType.DIGITS, length: 12);
            writer.Write(HBCI_Version, FieldType.NUMERIC, max_length: 3);
            writer.Write(DialogId, FieldType.IDENTIFICATION);
            writer.Write(MessageNumber, FieldType.NUMERIC, max_length: 4);
        }
    }
}
