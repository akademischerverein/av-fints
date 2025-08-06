using AV.FinTS.Raw.Segments.Internal;
using AV.FinTS.Raw.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Raw.Segments.Auth
{
    public class HNSHK4 : ISegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HNSHK", Version = 4 };

        public SecurityProfile SecurityProfile { get; set; } = null!;

        public int SecurityFunction { get; set; }

        public string SecurityReference { get; set; } = null!;

        public SignatureApplication SignatureApplication { get; set; }

        public VendorRole VendorRole { get; set; }

        public SecurityId SecurityId { get; set; } = null!;

        public int SecurityReferenceNumber { get; set; }

        public SecurityDateTime SecurityDateTime { get; set; } = null!;

        public HashAlgorithm HashAlgorithm { get; set; } = null!;

        internal SignatureAlgorithm SignatureAlgorithm { get; set; } = new SignatureAlgorithm();

        public KeyName KeyName { get; set; } = null!;

        public void Write(MessageWriter writer)
        {
            writer.Write(SecurityProfile);
            writer.Write(SecurityFunction, FieldType.NUMERIC, max_length: 3);
            writer.Write(SecurityReference, FieldType.ALPHA_NUMERIC, max_length: 14);
            writer.Write((int)SignatureApplication, FieldType.NUMERIC, max_length: 3);
            writer.Write((int)VendorRole, FieldType.NUMERIC, max_length: 3);
            writer.Write(SecurityId);
            writer.Write(SecurityReferenceNumber, FieldType.NUMERIC, max_length: 16);
            writer.Write(SecurityDateTime);
            writer.Write(HashAlgorithm);
            writer.Write(SignatureAlgorithm);
            writer.Write(KeyName);
        }
    }

    public enum SignatureApplication
    {
        SHM = 1,
        SHT = 2,
    }
}
