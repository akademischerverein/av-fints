using AV.FinTS.Raw.Structures;

namespace AV.FinTS.Raw
{
    public interface ISegment
    {
        public SegmentId Head { get; }

        public void Write(MessageWriter writer);
    }

    public interface IPickUpSegment : ISegment
    {
        public string? Aufsetzpunkt { get; }
    }
}
