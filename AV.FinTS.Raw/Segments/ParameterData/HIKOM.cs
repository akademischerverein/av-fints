using AV.FinTS.Raw.Codes;
using AV.FinTS.Raw.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Raw.Segments.ParameterData
{
    public class HIKOM4 : ISegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HIKOM", Version = 4 };

        public BankIdentifier Bank { get; set; } = null!;

        public Language DefaultLanguage { get; set; }

        public List<CommunicationParameter> CommunicationParameters { get; set; } = new();

        public static ISegment Read(MessageReader reader, SegmentId segmentId)
        {
            var kom = new HIKOM4
            {
                Bank = BankIdentifier.Read(reader),
                DefaultLanguage = (Language)reader.ReadInt()!
            };

            if (!Enum.IsDefined(kom.DefaultLanguage))
            {
                throw new InvalidDataException("Invalid language");
            }

            do
            {
                kom.CommunicationParameters.Add(CommunicationParameter.Read(reader));
            } while (!reader.SegmentEnded);

            return kom;
        }

        public void Write(MessageWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
