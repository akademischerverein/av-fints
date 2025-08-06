using AV.FinTS.Raw.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fb = AV.FinTS.Raw.Structures.Feedback;

namespace AV.FinTS.Raw.Segments.Feedback
{
    public class HIRMG2 : ISegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HIRMG", Version = 2 };

        public List<Fb> Feedbacks { get; private set; } = new();

        public static ISegment Read(MessageReader reader, SegmentId segmentId)
        {
            var seg = new HIRMG2();
            seg.Head = segmentId;
            while(!reader.SegmentEnded)
            {
                seg.Feedbacks.Add(Fb.Read(reader));
            }
            return seg;
        }

        public void Write(MessageWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
