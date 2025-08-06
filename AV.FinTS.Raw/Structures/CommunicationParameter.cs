using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Raw.Structures
{
    public class CommunicationParameter : IElementStructure
    {
        public CommunicationType Type { get; set; }

        public string Address { get; set; } = null!;

        public static CommunicationParameter Read(MessageReader reader)
        {
            reader.EnterGroup();
            var param = new CommunicationParameter
            {
                Type = (CommunicationType)reader.ReadInt()!,
                Address = reader.Read()!
            };

            if (!Enum.IsDefined(param.Type))
            {
                throw new InvalidDataException("Invalid CommunicationType");
            }

            reader.Read();
            reader.Read();
            reader.Read();
            reader.LeaveGroup();
            return param;
        }

        public void Write(MessageWriter writer)
        {
            throw new NotImplementedException();
        }

        public enum CommunicationType
        {
            T_ONLINE = 1,
            TCP_IP = 2,
            HTTPS = 3
        }
    }
}
