using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Raw.Structures
{
    internal class SignatureAlgorithm : IElementStructure
    {
        public void Write(MessageWriter writer)
        {
            writer.Write("6", FieldType.CODE);
            writer.Write("10", FieldType.CODE);
            writer.Write("16", FieldType.CODE);
        }
    }
}
