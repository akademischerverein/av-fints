using AV.FinTS.Raw.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Raw.Segments.Auth
{
    public class HNVSD1 : ISegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HNVSD", Version = 1 };

        public byte[] Data { get; set; } = null!;

        public static ISegment Read(MessageReader reader, SegmentId segmentId)
        {
            var seg = new HNVSD1
            {
                Head = segmentId,
                Data = reader.ReadBytes()
            };
            return seg;
        }

        public void Write(MessageWriter writer)
        {
            writer.Write(Data);
        }
    }
}
