using AV.FinTS.Raw.Codes;
using AV.FinTS.Raw.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Raw.Segments.Internal
{
    public class HKVVB3 : ISegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HKVVB", Version = 3 };

        public int BPD { get; set; }

        public int UPD { get; set; }

        public Language Language { get; set; } = Language.DEFAULT;

        public string Product { get; set; } = null!;

        public string ProductVersion { get; set; } = null!;

        public void Write(MessageWriter writer)
        {
            writer.Write(BPD, FieldType.NUMERIC, max_length: 3);
            writer.Write(UPD, FieldType.NUMERIC, max_length: 3);
            writer.Write(Language);
            writer.Write(Product, FieldType.ALPHA_NUMERIC, max_length: 25);
            writer.Write(ProductVersion, FieldType.ALPHA_NUMERIC);
        }
    }
}
