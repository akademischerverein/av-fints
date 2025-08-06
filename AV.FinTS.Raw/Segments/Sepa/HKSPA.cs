using AV.FinTS.Raw.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Raw.Segments.Sepa
{
    public class HKSPA1 : ISegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HKSPA", Version = 1 };

        public void Write(MessageWriter writer)
        {
        }
    }

    public class HISPA1 : ISegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HISPA", Version = 1 };

        public List<AccountInternationalSepa> Accounts { get; set; } = new();

        public static ISegment Read(MessageReader reader, SegmentId head)
        {
            var spa = new HISPA1 { Head = head };

            while(!reader.SegmentEnded)
            {
                spa.Accounts.Add(AccountInternationalSepa.Read(reader)!);
            }

            return spa;
        }

        public void Write(MessageWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    public class HISPAS1 : ParameterSegment
    {
        public override SegmentId Head { get; protected set; } = new SegmentId { Name = "HISPAS", Version = 1 };

        public bool SingleAccountRetrievalAllowed { get; set; }

        public bool NationalAccountNumberAllowed { get; set; }

        public bool StructedPurposeAllowed { get; set; }

        public List<string> SupportedSepaDataFormats { get; set; } = new();

        public static ISegment Read(MessageReader reader, SegmentId head)
        {
            var param = new HISPAS1 { Head = head };
            param.Read(reader);
            reader.EnterGroup();
            param.SingleAccountRetrievalAllowed = (bool)reader.ReadBool()!;
            param.NationalAccountNumberAllowed = (bool)reader.ReadBool()!;
            param.StructedPurposeAllowed = (bool)reader.ReadBool()!;

            while(!reader.GroupEnded)
            {
                param.SupportedSepaDataFormats.Add(reader.Read()!);
            }

            reader.LeaveGroup();

            return param;
        }
    }

    public class HKSPA2 : IPickUpSegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HKSPA", Version = 2 };

        public string? Aufsetzpunkt { get; set; }

        public void Write(MessageWriter writer)
        {
            if (Aufsetzpunkt != null)
            {
                for(int i = 0; i < 999; i++)
                {
                    writer.WriteEmpty();
                }

                writer.WriteEmpty();
                writer.Write(Aufsetzpunkt, FieldType.ALPHA_NUMERIC, max_length: 35);
            }
        }
    }

    public class HISPA2 : ISegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HISPA", Version = 2 };

        public List<AccountInternationalSepa> Accounts { get; set; } = new();

        public static ISegment Read(MessageReader reader, SegmentId head)
        {
            var spa = new HISPA2 { Head = head };

            while (!reader.SegmentEnded)
            {
                spa.Accounts.Add(AccountInternationalSepa.Read(reader)!);
            }

            return spa;
        }

        public void Write(MessageWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    public class HISPAS2 : ParameterSegment
    {
        public override SegmentId Head { get; protected set; } = new SegmentId { Name = "HISPAS", Version = 2 };

        public bool SingleAccountRetrievalAllowed { get; set; }

        public bool NationalAccountNumberAllowed { get; set; }

        public bool StructedPurposeAllowed { get; set; }

        public bool NumberEntriesAllowed { get; set; }

        public List<string> SupportedSepaDataFormats { get; set; } = new();

        public static ISegment Read(MessageReader reader, SegmentId head)
        {
            var param = new HISPAS2 { Head = head };
            param.Read(reader);
            reader.EnterGroup();
            param.SingleAccountRetrievalAllowed = (bool)reader.ReadBool()!;
            param.NationalAccountNumberAllowed = (bool)reader.ReadBool()!;
            param.StructedPurposeAllowed = (bool)reader.ReadBool()!;
            param.NumberEntriesAllowed = (bool)reader.ReadBool()!;

            while (!reader.GroupEnded)
            {
                param.SupportedSepaDataFormats.Add(reader.Read()!);
            }

            reader.LeaveGroup();

            return param;
        }
    }
}
