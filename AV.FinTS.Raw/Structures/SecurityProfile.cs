using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Raw.Structures
{
    public class SecurityProfile : IElementStructure
    {
        public SecurityProcedure Procedure { get; set; }

        public SecurityProcedureVersion ProcedureVersion { get; set; }

        public static SecurityProfile Read(MessageReader reader)
        {
            var profile = new SecurityProfile();
            reader.EnterGroup();
            var code = reader.Read()!;
            var version = (int)reader.ReadInt()!;
            reader.LeaveGroup();
            if(!Enum.IsDefined((SecurityProcedureVersion)version))
            {
                throw new InvalidDataException("Invalid security procedure version");
            }

            if (!Enum.IsDefined(typeof(SecurityProcedure), code))
            {
                throw new InvalidDataException("Invalid security procedure");
            }

            profile.Procedure = Enum.Parse<SecurityProcedure>(code);
            profile.ProcedureVersion = (SecurityProcedureVersion)version;
            return profile;
        }

        public void Write(MessageWriter writer)
        {
            switch (Procedure)
            {
                case SecurityProcedure.RAH:
                    writer.Write("RAH", FieldType.CODE);
                    break;

                case SecurityProcedure.PIN:
                    writer.Write("PIN", FieldType.CODE);
                    break;

                default:
                    throw new NotSupportedException(Procedure.ToString());
            }

            writer.Write((int)ProcedureVersion, FieldType.NUMERIC, max_length: 3);
        }
    }

    public enum SecurityProcedureVersion
    {
        PINTAN_ONE_STEP = 1,
        PINTAN_TWO_STEP = 2
    }

    public enum SecurityProcedure
    {
        RAH,
        PIN
    }
}
