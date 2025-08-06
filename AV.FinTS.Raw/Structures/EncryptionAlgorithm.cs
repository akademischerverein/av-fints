using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Raw.Structures
{
    internal class EncryptionAlgorithm : IElementStructure
    {
        public static void SkipElement(MessageReader reader)
        {
            reader.EnterGroup();
            reader.Read();
            reader.Read();
            reader.Read();
            reader.ReadBytes();
            reader.Read();
            reader.Read();
            reader.Read();
            reader.LeaveGroup();
        }

        public void Write(MessageWriter writer)
        {
            writer.Write("2", FieldType.CODE, max_length: 3);
            writer.Write("2", FieldType.CODE, max_length: 3);
            writer.Write("13", FieldType.CODE, max_length: 3);
            writer.Write([0, 0, 0, 0, 0, 0, 0, 0]);
            writer.Write("5", FieldType.CODE, max_length: 3);
            writer.Write("1", FieldType.CODE, max_length: 3);
        }
    }
}
