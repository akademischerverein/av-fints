using AV.FinTS.Raw.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Raw.Segments.Internal
{
    public class HNHBS1 : ISegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HNHBS", Version = 1 };

        public int MessageNumber { get; set; }

        public static ISegment Read(MessageReader reader, SegmentId segmentId)
        {
            var tail = new HNHBS1();
            tail.MessageNumber = (int)reader.ReadInt()!;
            tail.Head = segmentId;
            return tail;
        }

        public void Write(MessageWriter writer)
        {
            writer.Write(MessageNumber, FieldType.NUMERIC, max_length: 4);
        }
    }
}
