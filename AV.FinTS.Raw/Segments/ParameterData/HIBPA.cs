using AV.FinTS.Raw.Codes;
using AV.FinTS.Raw.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Raw.Segments.ParameterData
{
    public class HIBPA3 : ISegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HIBPA", Version = 3 };

        public int BpdVersion { get; set; }

        public BankIdentifier Bank { get; set; } = null!;

        public string BankName { get; set; } = null!;

        public int MaxTransactionsPerMessage { get; set; }

        public List<Language> SupportedLanguages { get; set; } = new();

        public List<int> SupportedHbciVersions { get; set; } = new();

        public static ISegment Read(MessageReader reader, SegmentId segmentId)
        {
            var bpa = new HIBPA3
            {
                Head = segmentId,
                BpdVersion = (int)reader.ReadInt()!,
                Bank = BankIdentifier.Read(reader),
                BankName = reader.Read()!,
                MaxTransactionsPerMessage = (int)reader.ReadInt()!
            };

            reader.EnterGroup();
            do
            {
                var langCode = (Language)reader.ReadInt()!;
                if(!Enum.IsDefined(langCode))
                {
                    throw new InvalidDataException("Invalid language");
                }
                bpa.SupportedLanguages.Add(langCode);
            } while (!reader.GroupEnded);
            reader.LeaveGroup();

            reader.EnterGroup();
            do
            {
                var version = (int)reader.ReadInt()!;
                bpa.SupportedHbciVersions.Add(version);
            } while (!reader.GroupEnded);
            reader.LeaveGroup();

            reader.ReadInt();
            reader.ReadInt();
            reader.ReadInt();

            return bpa;
        }

        public void Write(MessageWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
