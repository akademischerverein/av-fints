using AV.FinTS.Raw.Segments.Internal;
using AV.FinTS.Raw.Structures;

namespace AV.FinTS.Raw.Segments.Auth
{
    public class HNVSK3 : ISegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HNVSK", Version = 3 };

        public SecurityProfile SecurityProfile { get; set; } = null!;

        public int SecurityFunction { get; set; }

        public VendorRole VendorRole { get; set; }

        public SecurityId SecurityId { get; set; } = null!;

        public SecurityDateTime SecurityDateTime { get; set; } = null!;

        internal EncryptionAlgorithm EncryptionAlgorithm { get; set; } = new EncryptionAlgorithm();

        public KeyName KeyName { get; set; } = null!;

        public static ISegment Read(MessageReader reader, SegmentId segmentId)
        {
            var encryption_header = new HNVSK3
            {
                SecurityProfile = SecurityProfile.Read(reader),
                SecurityFunction = (int)reader.ReadInt()!,
                VendorRole = (VendorRole)reader.ReadInt()!,
                SecurityId = SecurityId.Read(reader),
                SecurityDateTime = SecurityDateTime.Read(reader)
            };
            EncryptionAlgorithm.SkipElement(reader);
            encryption_header.KeyName = KeyName.Read(reader);

            if (reader.Read() != "0")
            {
                throw new NotSupportedException("Unsupported compression function");
            }
            reader.Read();
            encryption_header.Head = segmentId;

            return encryption_header;
        }

        public void Write(MessageWriter writer)
        {
            writer.Write(SecurityProfile);
            writer.Write(SecurityFunction, FieldType.NUMERIC, max_length: 3);
            writer.Write((int)VendorRole, FieldType.NUMERIC, max_length: 3);
            writer.Write(SecurityId);
            writer.Write(SecurityDateTime);
            writer.Write(EncryptionAlgorithm);
            writer.Write(KeyName);
            writer.Write("0", FieldType.CODE, max_length: 3);
        }
    }
}
