using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Raw.Structures
{
    public class SecurityDateTime : IElementStructure
    {
        public Identifier CodeIdentifier { get; set; }

        public DateOnly? Date { get; set; }

        public TimeOnly? Time { get; set; }

        public static SecurityDateTime Read(MessageReader reader)
        {
            var sdt = new SecurityDateTime();
            reader.EnterGroup();
            var id = reader.ReadInt()!;
            var date = reader.ReadDate();
            var time = reader.ReadTime();
            reader.LeaveGroup();

            if(!Enum.IsDefined(typeof(Identifier), id))
            {
                throw new InvalidDataException("Invalid date/time identifier");
            }
            sdt.Date = date;
            sdt.Time = time;
            sdt.CodeIdentifier = (Identifier)id;
            return sdt;
        }

        public void Write(MessageWriter writer)
        {
            writer.Write((int)CodeIdentifier, FieldType.NUMERIC);
            
            if (Date != null)
            {
                writer.Write((DateOnly)Date, FieldType.DATE);
                if (Time == null)
                {
                    writer.WriteEmpty();
                } else
                {
                    writer.Write((TimeOnly)Time, FieldType.TIME);
                }
            }
        }

        public enum Identifier
        {
            SECURITY_TIMESTAMP = 1,
            CERTIFICATE_REVOCATION_TIME = 6
        }
    }
}
