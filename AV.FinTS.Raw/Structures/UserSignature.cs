using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Raw.Structures
{
    public class UserSignature : IElementStructure
    {
        public char[] PIN { get; set; } = null!;

        public string? TAN { get; set; } = null!;

        public void Write(MessageWriter writer)
        {
            writer.Write(PIN, FieldType.ALPHA_NUMERIC, max_length: 99);
            if (TAN != null)
            {
                writer.Write(TAN, FieldType.ALPHA_NUMERIC, max_length: 99);
            }
        }
    }
}
