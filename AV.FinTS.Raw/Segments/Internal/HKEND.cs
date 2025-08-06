using AV.FinTS.Raw.Structures;

namespace AV.FinTS.Raw.Segments.Internal
{
    public class HKEND1 : ISegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HKEND", Version = 1 };

        public string DialogId { get; set; } = null!;

        public void Write(MessageWriter writer)
        {
            writer.Write(DialogId, FieldType.IDENTIFICATION);
        }
    }
}
