using AV.FinTS.Raw.Codes;
using AV.FinTS.Raw.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Raw.Segments.Auth
{
    public class HKTAB4 : ISegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HKTAB", Version = 4 };

        public TanMediumType Type { get; set; }

        public TanMediumClass Class { get; set; }

        public void Write(MessageWriter writer)
        {
            writer.Write((int)Type, FieldType.NUMERIC, max_length: 1);

            switch (Class)
            {
                case TanMediumClass.ALL:
                    writer.Write("A", FieldType.CODE, max_length: 1);
                    break;

                case TanMediumClass.LIST:
                    writer.Write("L", FieldType.CODE, max_length: 1);
                    break;

                case TanMediumClass.TAN_GENERATOR:
                    writer.Write("G", FieldType.CODE, max_length: 1);
                    break;

                case TanMediumClass.MOBILE_PHONE:
                    writer.Write("M", FieldType.CODE, max_length: 1);
                    break;

                case TanMediumClass.SECODER:
                    writer.Write("S", FieldType.CODE, max_length: 1);
                    break;
                default:
                    throw new InvalidDataException("invalid tan medium class set: " + Class.ToString());
            }
        }
    }

    public class HITAB4 : ISegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HITAB", Version = 4 };

        public TanUsageOption UsageOption { get; set; }

        public List<TanMediumElement> Media { get; private set; } = new();

        public static ISegment Read(MessageReader reader, SegmentId segmentId)
        {
            var tab = new HITAB4
            {
                Head = segmentId,
                UsageOption = (TanUsageOption)reader.ReadInt()!
            };

            if (!Enum.IsDefined(tab.UsageOption))
            {
                throw new InvalidDataException("invalid tan usage option set: " + (int)tab.UsageOption);
            }

            while (!reader.SegmentEnded)
            {
                var ele = TanMediumElement.Read(reader);
                if (ele != null)
                {
                    tab.Media.Add(ele);
                }
            }
            return tab;
        }

        public void Write(MessageWriter writer)
        {
            throw new NotImplementedException();
        }

        public class TanMediumElement : IElementStructure
        {
            public TanMediumClass Class { get; set; }

            public TanMediumStatus State { get; set; }

            public string? CardNumber { get; set; }

            public string? CardFollowUpNumber { get; set; }

            public int? CardType { get; set; }

            public Account? Account { get; set; }

            public DateOnly? ValidFrom { get; set; }

            public DateOnly? ValidUntil { get; set; }

            public string? TanListNumber { get; set; }

            public string? TanMediumName { get; set; }

            public string? MobileNumberConcealed { get; set; }

            public string? MobileNumber { get; set; }

            public AccountInternational? TextMessagePayee { get; set; }

            public int? NumFreeTans { get; set; }

            public DateOnly? LastUsage { get; set; }

            public DateOnly? ActivatedOn { get; set; }

            public static TanMediumElement? Read(MessageReader reader)
            {
                reader.EnterGroup();
                var classStr = reader.Read();
                if (classStr == null && reader.GroupEnded) { return null; }
                TanMediumClass mediumClass;
                switch (classStr)
                {
                    case "A":
                        mediumClass = TanMediumClass.ALL;
                        break;

                    case "G":
                        mediumClass = TanMediumClass.TAN_GENERATOR;
                        break;

                    case "L":
                        mediumClass = TanMediumClass.LIST;
                        break;

                    case "M":
                        mediumClass = TanMediumClass.MOBILE_PHONE;
                        break;

                    case "S":
                        mediumClass = TanMediumClass.SECODER;
                        break;

                    default:
                        throw new InvalidDataException("invalid tan medium class sent: " + classStr);
                }

                var ele = new TanMediumElement
                {
                    Class = mediumClass,
                    State = (TanMediumStatus)reader.ReadInt()!,
                    CardNumber = reader.Read(),
                    CardFollowUpNumber = reader.Read(),
                    CardType = reader.ReadInt(),
                    Account = Account.Read(reader),
                    ValidFrom = reader.ReadDate(),
                    ValidUntil = reader.ReadDate(),
                    TanListNumber = reader.Read(),
                    TanMediumName = reader.Read(),
                    MobileNumberConcealed = reader.Read(),
                    MobileNumber = reader.Read(),
                    TextMessagePayee = AccountInternational.Read(reader),
                    NumFreeTans = reader.ReadInt(),
                    LastUsage = reader.ReadDate(),
                    ActivatedOn = reader.ReadDate()
                };

                if (!Enum.IsDefined(ele.State))
                {
                    throw new InvalidDataException("invalid tan medium status sent: " + (int)ele.State);
                }
                reader.LeaveGroup();
                return ele;
            }

            public void Write(MessageWriter writer)
            {
                throw new NotImplementedException();
            }
        }
    }

    public class HITABS4 : ParameterSegment
    {
        public override SegmentId Head { get; protected set; } = new SegmentId { Name = "HITABS", Version = 4 };

        public static ISegment Read(MessageReader reader, SegmentId segmentId)
        {
            var param = new HITABS4
            {
                Head = segmentId,
            };
            param.Read(reader);

            return param;
        }
    }

    /*public class HKTAB5 : ISegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HKTAB", Version = 5 };

        public TanMediumType Type { get; set; }

        public TanMediumClass Class { get; set; }

        public void Write(MessageWriter writer)
        {
            writer.Write((int)Type, FieldType.NUMERIC, max_length: 1);

            switch (Class)
            {
                case TanMediumClass.ALL:
                    writer.Write("A", FieldType.CODE, max_length: 1);
                    break;

                case TanMediumClass.LIST:
                    writer.Write("L", FieldType.CODE, max_length: 1);
                    break;

                case TanMediumClass.TAN_GENERATOR:
                    writer.Write("G", FieldType.CODE, max_length: 1);
                    break;

                case TanMediumClass.MOBILE_PHONE:
                    writer.Write("M", FieldType.CODE, max_length: 1);
                    break;

                case TanMediumClass.SECODER:
                    writer.Write("S", FieldType.CODE, max_length: 1);
                    break;

                case TanMediumClass.BILATERAL:
                    writer.Write("B", FieldType.CODE, max_length: 1);
                    break;

                default:
                    throw new InvalidDataException("invalid tan medium class set: " + Class.ToString());
            }
        }
    }

    public class HITAB5 : ISegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HITAB", Version = 5 };

        public TanUsageOption UsageOption { get; set; }

        public List<TanMediumElement45> Media { get; private set; } = new();

        public static ISegment Read(MessageReader reader, SegmentId segmentId)
        {
            var tab = new HITAB5
            {
                Head = segmentId,
                UsageOption = (TanUsageOption)reader.ReadInt()!
            };

            if (!Enum.IsDefined(tab.UsageOption))
            {
                throw new InvalidDataException("invalid tan usage option set: " + (int)tab.UsageOption);
            }

            while (!reader.SegmentEnded)
            {
                var ele = TanMediumElement45.Read(reader);
                if (ele != null)
                {
                    tab.Media.Add(ele);
                }
            }
            return tab;
        }

        public void Write(MessageWriter writer)
        {
            throw new NotImplementedException();
        }

        
    }

    public class HITABS5 : ParameterSegment
    {
        public override SegmentId Head { get; protected set; } = new SegmentId { Name = "HITABS", Version = 5 };

        public static ISegment Read(MessageReader reader, SegmentId segmentId)
        {
            var param = new HITABS5
            {
                Head = segmentId,
            };
            param.Read(reader);

            return param;
        }
    }*/
}
