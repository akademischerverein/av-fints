using AV.FinTS.Raw.Structures;

namespace AV.FinTS.Raw.Segments.ParameterData
{
    public class HIUPA4 : ISegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HIUPA", Version = 4 };

        public string UserId { get; set; } = null!;

        public int UpdVersion { get; set; }

        public bool NonListedOperationsDisallowed { get; set; }

        public string? Username { get; set; }

        public string? Extension { get; set; }

        public static ISegment Read(MessageReader reader, SegmentId segmentId)
        {
            return new HIUPA4
            {
                Head = segmentId,
                UserId = reader.Read()!,
                UpdVersion = (int)reader.ReadInt()!,
                NonListedOperationsDisallowed = !(bool)reader.ReadBool()!,
                Username = reader.Read(),
                Extension = reader.Read()
            };
        }

        public void Write(MessageWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
