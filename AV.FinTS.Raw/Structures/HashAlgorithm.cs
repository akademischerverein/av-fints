using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Raw.Structures
{
    public class HashAlgorithm : IElementStructure
    {
        public Code CodedAlgorithm { get; set; }

        public void Write(MessageWriter writer)
        {
            writer.Write("1", FieldType.CODE, max_length: 3);
            writer.Write((int)CodedAlgorithm, FieldType.NUMERIC, max_length: 3);
            writer.Write("1", FieldType.CODE, max_length: 3);
        }

        public enum Code
        {
            SHA1 = 1,
            RESERVED = 2,
            SHA256 = 3,
            SHA384 = 4,
            SHA512 = 5,
            SHA256_SHA256 = 6,
            MUTUALLY_AGREED = 999
        }
    }
}
