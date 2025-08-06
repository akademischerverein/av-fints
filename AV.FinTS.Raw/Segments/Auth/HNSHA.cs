using AV.FinTS.Raw.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Raw.Segments.Auth
{
    public class HNSHA2 : ISegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HNSHA", Version = 2 };

        public string SecurityReference { get; set; } = null!;

        public UserSignature Signature { get; set; } = null!;

        public void Write(MessageWriter writer)
        {
            writer.Write(SecurityReference, FieldType.ALPHA_NUMERIC, max_length: 14);
            writer.WriteEmpty();
            writer.Write(Signature);
        }
    }
}
