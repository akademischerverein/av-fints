using AV.FinTS.Raw.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Raw.Segments.Internal
{
    public class HKSYN3 : ISegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HKSYN", Version = 3 };

        public SynchonizationMode Mode { get; set; }

        public void Write(MessageWriter writer)
        {
            writer.Write((int)Mode, FieldType.NUMERIC, max_length: 3);
        }
    }

    public enum SynchonizationMode
    {
        NEW_CUSTOMER_SYSTEM_ID = 0,
        LAST_PROCESSED_MESSAGE_NUMBER = 1,
        SIGNATURE_ID = 2
    }

    public class HISYN4 : ISegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HISYN", Version = 4 };

        public string CustomerSystemId { get; set; } = null!;

        public static ISegment Read(MessageReader reader, SegmentId segmentId)
        {
            var segment = new HISYN4
            {
                Head = segmentId,
                CustomerSystemId = reader.Read()!
            };

            reader.Read();
            reader.Read();
            reader.Read();
            return segment;
        }

        public void Write(MessageWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
