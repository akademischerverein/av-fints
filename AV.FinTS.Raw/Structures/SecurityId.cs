using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Raw.Structures
{
    public class SecurityId : IElementStructure
    {
        public SecurityParty SecurityParty { get; set; }

        public string PartyId { get; set; } = null!;

        public static SecurityId Read(MessageReader reader)
        {
            reader.EnterGroup();
            var party = reader.ReadInt()!;
            reader.Read();
            var partyId = reader.Read()!;
            reader.LeaveGroup();

            if (!Enum.IsDefined(typeof(SecurityParty), party))
            {
                throw new InvalidDataException("Invalid security party");
            }

            return new SecurityId
            {
                SecurityParty = (SecurityParty)party,
                PartyId = partyId
            };
        }

        public void Write(MessageWriter writer)
        {
            writer.Write((int)SecurityParty, FieldType.NUMERIC);
            writer.WriteEmpty();
            writer.Write(PartyId, FieldType.IDENTIFICATION);
        }
    }

    public enum SecurityParty
    {
        MESSAGE_SENDER = 1,
        MESSAGE_RECEIVER = 2
    }
}
