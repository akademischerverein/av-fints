using AV.FinTS.Raw.Structures;

namespace AV.FinTS.Raw.Segments.Internal
{
    public class HKIDN2 : ISegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HKIDN", Version = 2 };

        public BankIdentifier Bank { get; set; } = null!;

        public string CustomerId { get; set; } = null!;

        public string CustomerSystemId { get; set; } = null!;

        public bool CustomerSystemIdRequired { get; set; }

        public void Write(MessageWriter writer)
        {
            writer.Write(Bank);
            writer.Write(CustomerId, FieldType.IDENTIFICATION);
            writer.Write(CustomerSystemId, FieldType.IDENTIFICATION);

            if (CustomerSystemIdRequired)
            {
                writer.Write("1", FieldType.CODE, max_length: 1);
            } else
            {
                writer.Write("0", FieldType.CODE, max_length: 1);
            }
        }
    }
}
