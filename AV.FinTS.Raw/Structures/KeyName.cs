using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Raw.Structures
{
    public class KeyName : IElementStructure
    {
        public BankIdentifier Bank { get; set; } = null!;

        public string UserId { get; set; } = null!;

        public Type KeyType { get; set; }

        public static KeyName Read(MessageReader reader)
        {
            reader.EnterGroup();
            var keyName = new KeyName
            {
                Bank = BankIdentifier.Read(reader),
                UserId = reader.Read()!
            };
            var key_type = reader.Read();
            switch(key_type)
            {
                case "D":
                    keyName.KeyType = Type.D_DS_KEY;
                    break;

                case "S":
                    keyName.KeyType = Type.S_SIGNATURE_KEY;
                    break;

                case "V":
                    keyName.KeyType = Type.V_CIPHER_KEY;
                    break;

                default:
                    throw new InvalidDataException("Invalid key type");
            }
            reader.Read();
            reader.Read();
            reader.LeaveGroup();
            return keyName;
        }

        public void Write(MessageWriter writer)
        {
            writer.Write(Bank);
            writer.Write(UserId, FieldType.IDENTIFICATION);
            
            switch(KeyType)
            {
                case Type.D_DS_KEY:
                    writer.Write("D", FieldType.CODE, max_length: 1);
                    break;

                case Type.S_SIGNATURE_KEY:
                    writer.Write("S", FieldType.CODE, max_length: 1);
                    break;

                case Type.V_CIPHER_KEY:
                    writer.Write("V", FieldType.CODE, max_length: 1);
                    break;

                default:
                    throw new NotImplementedException();
            }

            writer.Write(0, FieldType.NUMERIC, max_length: 3);
            writer.Write(0, FieldType.NUMERIC, max_length: 3);
        }

        public enum Type
        {
            D_DS_KEY,
            S_SIGNATURE_KEY,
            V_CIPHER_KEY
        }
    }
}
