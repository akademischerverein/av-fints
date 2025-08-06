using AV.FinTS.Raw.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Raw.Segments.Auth
{
    public class HKTAN7 : ISegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HKTAN", Version = 7 };

        public string? SegmentName { get; set; } = null;

        public string Process { get; set; } = null!;

        public string? Reference { get; set; } = null;

        public string? TanMedium { get; set; } = null;

        public bool? FutureTanFollowUp { get; set; }

        public void Write(MessageWriter writer)
        {
            writer.Write(Process, FieldType.ALPHA_NUMERIC); // 2

            if (SegmentName != null)
            {
                writer.Write(SegmentName, FieldType.ALPHA_NUMERIC); // 3
            } else
            {
                writer.WriteEmpty(); // 3
            }

            writer.WriteEmpty(); // 4
            writer.WriteEmpty(); // 5

            if (Reference != null)
            {
                writer.Write(Reference, FieldType.ALPHA_NUMERIC); // 6
            } else
            {
                writer.WriteEmpty(); // 6
            }

            if (FutureTanFollowUp != null)
            {
                writer.Write((bool)FutureTanFollowUp, FieldType.YES_NO); // 7
            }
            else
            {
                writer.WriteEmpty(); // 7
            }

            writer.WriteEmpty(); // 8
            writer.WriteEmpty(); // 9
            writer.WriteEmpty(); // 10
            writer.WriteEmpty(); // 11

            if (TanMedium != null)
            {
                writer.Write(TanMedium, FieldType.ALPHA_NUMERIC); // 12
            } else
            {
                writer.WriteEmpty(); // 12
            }

            writer.WriteEmpty(); // 13
        }
    }

    public class HITAN7 : ISegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HITAN", Version = 7 };

        public string Process { get; set; } = null!;

        public string? Reference { get; set; }

        public string? Challenge { get; set; }

        public byte[] ChallengeHddUc { get; set; } = [];

        public string? TanMedium { get; set; }

        public DateTime? ValidUpTo { get; set; }

        public static ISegment Read(MessageReader reader, SegmentId segmentId)
        {
            var seg = new HITAN7
            {
                Head = segmentId,
                Process = reader.Read()!
            };
            reader.Read();
            seg.Reference = reader.Read();
            seg.Challenge = reader.Read();
            seg.ChallengeHddUc = reader.ReadBytes();

            reader.EnterGroup();
            var d = reader.ReadDate();
            if (d != null)
            {
                seg.ValidUpTo = d.Value.ToDateTime((TimeOnly)reader.ReadTime()!);
            }
            reader.LeaveGroup();
            seg.TanMedium = reader.Read();
            return seg;
        }

        public void Write(MessageWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    public class HITANS7 : ISegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HITANS", Version = 7 };

        public int MaxOrders { get; set; }

        public int MinSignatures { get; set; }

        public int SecurityClass { get; set; }

        public bool OneStepProcedureAllowed { get; set; }

        public bool SeveralTanOrdersAllowed { get; set; }

        public int OrderHashProcedure { get; set; }

        public List<TwoStepParameters> Procedures { get; set; } = new();

        public static ISegment Read(MessageReader reader, SegmentId segmentId)
        {
            var tans = new HITANS7
            {
                Head = segmentId,
                MaxOrders = (int)reader.ReadInt()!,
                MinSignatures = (int)reader.ReadInt()!,
                SecurityClass = (int)reader.ReadInt()!
            };
            reader.EnterGroup();
            tans.OneStepProcedureAllowed = reader.Read() == "J";
            tans.SeveralTanOrdersAllowed = reader.Read() == "J";
            tans.OrderHashProcedure = (int)reader.ReadInt()!;

            do
            {
                tans.Procedures.Add(TwoStepParameters.Read(reader));
            } while (!reader.GroupEnded);
            reader.LeaveGroup();

            return tans;
        }

        public void Write(MessageWriter writer)
        {
            throw new NotImplementedException();
        }

        public class TwoStepParameters : IElementStructure
        {
            public int SecurityFunction { get; set; }

            public int TanProcess { get; set; }

            public string TechnicalName { get; set; } = null!;

            public string? DkTanName { get; set; }

            public string? DkTanVersion { get; set; }

            public string TwoStepProcedureName { get; set; } = null!;

            public int? MaxTanLength { get; set; }

            public int? AllowedTanFormat { get; set; }

            public string ReturnValueName { get; set; } = null!;

            public int MaxReturnValueLength { get; set; }

            public bool MultipleUseTanAllowed { get; set; }

            public int TanTimeAndDialogRestriction { get; set; }

            public bool OrderCancellationAllowed { get; set; }

            public int SmsDebitAccountRequired { get; set; }

            public int ClientAccountRequired { get; set; }

            public bool ChallengeClassRequired { get; set; }

            public bool ChallengeStructured { get; set; }

            public string InitialisationMode { get; set; } = null!;

            public int TanMediumRequired { get; set; }

            public bool AnswerHhdUcRequired { get; set; }

            public int? CountActiveTanMedia { get; set; }

            public int? MaximumStatusRequests { get; set; }

            public int? WaitTimeBeforeFirstRequest { get; set; }

            public int? WaitTimeBeforeNextRequest { get; set; }

            public bool? ConfirmationAllowed { get; set; }

            public bool? AutomaticStatusRequestsAllowed { get; set; }

            public static TwoStepParameters Read(MessageReader reader)
            {
                reader.EnterGroup();
                var param = new TwoStepParameters
                {
                    SecurityFunction = (int)reader.ReadInt()!,
                    TanProcess = (int)reader.ReadInt()!,
                    TechnicalName = reader.Read()!,
                    DkTanName = reader.Read(),
                    DkTanVersion = reader.Read(),
                    TwoStepProcedureName = reader.Read()!,
                    MaxTanLength = reader.ReadInt(),
                    AllowedTanFormat = reader.ReadInt(),
                    ReturnValueName = reader.Read()!,
                    MaxReturnValueLength = (int)reader.ReadInt()!,
                    MultipleUseTanAllowed = (bool)reader.ReadBool()!,
                    TanTimeAndDialogRestriction = (int)reader.ReadInt()!,
                    OrderCancellationAllowed = (bool)reader.ReadBool()!,
                    SmsDebitAccountRequired = (int)reader.ReadInt()!,
                    ClientAccountRequired = (int)reader.ReadInt()!,
                    ChallengeClassRequired = (bool)reader.ReadBool()!,
                    ChallengeStructured = (bool)reader.ReadBool()!,
                    InitialisationMode = reader.Read()!,
                    TanMediumRequired = (int)reader.ReadInt()!,
                    AnswerHhdUcRequired = (bool)reader.ReadBool()!,
                    CountActiveTanMedia = reader.ReadInt(),
                    MaximumStatusRequests = reader.ReadInt(),
                    WaitTimeBeforeFirstRequest = reader.ReadInt(),
                    WaitTimeBeforeNextRequest = reader.ReadInt(),
                    ConfirmationAllowed = reader.ReadBool(),
                    AutomaticStatusRequestsAllowed = reader.ReadBool(),
                };
                reader.LeaveGroup();

                return param;
            }

            public void Write(MessageWriter writer)
            {
                throw new NotImplementedException();
            }
        }
    }

    public class HKTAN6 : ISegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HKTAN", Version = 6 };

        public string? SegmentName { get; set; } = null;

        public string Process { get; set; } = null!;

        public string? Reference { get; set; } = null;

        public string? TanMedium { get; set; } = null;

        public bool? FutureTanFollowUp { get; set; }

        public void Write(MessageWriter writer)
        {
            writer.Write(Process, FieldType.ALPHA_NUMERIC); // 2

            if (SegmentName != null)
            {
                writer.Write(SegmentName, FieldType.ALPHA_NUMERIC); // 3
            }
            else
            {
                writer.WriteEmpty(); // 3
            }

            writer.WriteEmpty(); // 4
            writer.WriteEmpty(); // 5

            if (Reference != null)
            {
                writer.Write(Reference, FieldType.ALPHA_NUMERIC); // 6
            }
            else
            {
                writer.WriteEmpty(); // 6
            }

            if (FutureTanFollowUp != null)
            {
                writer.Write((bool)FutureTanFollowUp, FieldType.YES_NO); // 7
            }
            else
            {
                writer.WriteEmpty(); // 7
            }

            writer.WriteEmpty(); // 8
            writer.WriteEmpty(); // 9
            writer.WriteEmpty(); // 10
            writer.WriteEmpty(); // 11

            if (TanMedium != null)
            {
                writer.Write(TanMedium, FieldType.ALPHA_NUMERIC); // 12
            }
            else
            {
                writer.WriteEmpty(); // 12
            }

            writer.WriteEmpty(); // 13
        }
    }

    public class HITAN6 : ISegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HITAN", Version = 6 };

        public string Process { get; set; } = null!;

        public string? Reference { get; set; }

        public string? Challenge { get; set; }

        public byte[] ChallengeHddUc { get; set; } = [];

        public string? TanMedium { get; set; }

        public DateTime? ValidUpTo { get; set; }

        public static ISegment Read(MessageReader reader, SegmentId segmentId)
        {
            var seg = new HITAN6
            {
                Head = segmentId,
                Process = reader.Read()!
            };
            reader.Read();
            seg.Reference = reader.Read();
            seg.Challenge = reader.Read();
            seg.ChallengeHddUc = reader.ReadBytes();

            reader.EnterGroup();
            var d = reader.ReadDate();
            if (d != null)
            {
                seg.ValidUpTo = d.Value.ToDateTime((TimeOnly)reader.ReadTime()!);
            }
            reader.LeaveGroup();
            seg.TanMedium = reader.Read();
            return seg;
        }

        public void Write(MessageWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    public class HITANS6 : ISegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HITANS", Version = 6 };

        public int MaxOrders { get; set; }

        public int MinSignatures { get; set; }

        public int SecurityClass { get; set; }

        public bool OneStepProcedureAllowed { get; set; }

        public bool SeveralTanOrdersAllowed { get; set; }

        public int OrderHashProcedure { get; set; }

        public List<TwoStepParameters> Procedures { get; set; } = new();

        public static ISegment Read(MessageReader reader, SegmentId segmentId)
        {
            var tans = new HITANS6
            {
                Head = segmentId,
                MaxOrders = (int)reader.ReadInt()!,
                MinSignatures = (int)reader.ReadInt()!,
                SecurityClass = (int)reader.ReadInt()!
            };
            reader.EnterGroup();
            tans.OneStepProcedureAllowed = (bool)reader.ReadBool()!;
            tans.SeveralTanOrdersAllowed = (bool)reader.ReadBool()!;
            tans.OrderHashProcedure = (int)reader.ReadInt()!;

            do
            {
                tans.Procedures.Add(TwoStepParameters.Read(reader));
            } while (!reader.GroupEnded);
            reader.LeaveGroup();

            return tans;
        }

        public void Write(MessageWriter writer)
        {
            throw new NotImplementedException();
        }

        public class TwoStepParameters : IElementStructure
        {
            public int SecurityFunction { get; set; }

            public int TanProcess { get; set; }

            public string TechnicalName { get; set; } = null!;

            public string? ZkaTanName { get; set; }

            public string? ZkaTanVersion { get; set; }

            public string TwoStepProcedureName { get; set; } = null!;

            public int MaxTanLength { get; set; }

            public int AllowedTanFormat { get; set; }

            public string ReturnValueName { get; set; } = null!;

            public int MaxReturnValueLength { get; set; }

            public bool MultipleUseTanAllowed { get; set; }

            public int TanTimeAndDialogRestriction { get; set; }

            public bool OrderCancellationAllowed { get; set; }

            public int SmsDebitAccountRequired { get; set; }

            public int ClientAccountRequired { get; set; }

            public bool ChallengeClassRequired { get; set; }

            public bool ChallengeStructured { get; set; }

            public string InitialisationMode { get; set; } = null!;

            public int TanMediumRequired { get; set; }

            public bool AnswerHhdUcRequired { get; set; }

            public int? CountActiveTanMedia { get; set; }

            public static TwoStepParameters Read(MessageReader reader)
            {
                reader.EnterGroup();
                var param = new TwoStepParameters
                {
                    SecurityFunction = (int)reader.ReadInt()!,
                    TanProcess = (int)reader.ReadInt()!,
                    TechnicalName = reader.Read()!,
                    ZkaTanName = reader.Read(),
                    ZkaTanVersion = reader.Read(),
                    TwoStepProcedureName = reader.Read()!,
                    MaxTanLength = (int)reader.ReadInt()!,
                    AllowedTanFormat = (int)reader.ReadInt()!,
                    ReturnValueName = reader.Read()!,
                    MaxReturnValueLength = (int)reader.ReadInt()!,
                    MultipleUseTanAllowed = (bool)reader.ReadBool()!,
                    TanTimeAndDialogRestriction = (int)reader.ReadInt()!,
                    OrderCancellationAllowed = (bool)reader.ReadBool()!,
                    SmsDebitAccountRequired = (int)reader.ReadInt()!,
                    ClientAccountRequired = (int)reader.ReadInt()!,
                    ChallengeClassRequired = (bool)reader.ReadBool()!,
                    ChallengeStructured = (bool)reader.ReadBool()!,
                    InitialisationMode = reader.Read()!,
                    TanMediumRequired = (int)reader.ReadInt()!,
                    AnswerHhdUcRequired = (bool)reader.ReadBool()!,
                    CountActiveTanMedia = reader.ReadInt()
                };
                reader.LeaveGroup();

                return param;
            }

            public void Write(MessageWriter writer)
            {
                throw new NotImplementedException();
            }
        }
    }
}
