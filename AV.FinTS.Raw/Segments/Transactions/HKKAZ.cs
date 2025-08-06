using AV.FinTS.Raw.Codes;
using AV.FinTS.Raw.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Raw.Segments.Transactions
{
    public class HKKAZ4 : ISegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HKKAZ", Version = 4 };

        public AccountLegacy Account { get; set; } = null!;

        public string? Currency { get; set; }

        public string? Aufsetzpunkt { get; set; }

        public DateOnly? From { get; set; }

        public DateOnly? To { get; set; }

        public int? MaximumEntries { get; set; }

        public void Write(MessageWriter writer)
        {
            writer.Write(Account);
            if (Currency != null)
            {
                writer.Write(Currency, FieldType.ALPHA_NUMERIC, max_length: 3);
            } else
            {
                writer.WriteEmpty();
            }
            if (From != null)
            {
                writer.Write((DateOnly)From, FieldType.DATE);
            } else
            {
                writer.WriteEmpty();
            }
            if (To != null)
            {
                writer.Write((DateOnly)To, FieldType.DATE);
            }
            else
            {
                writer.WriteEmpty();
            }
            if (MaximumEntries != null)
            {
                writer.Write((int)MaximumEntries, FieldType.NUMERIC, max_length: 4);
            }
            else
            {
                writer.WriteEmpty();
            }

            if (Aufsetzpunkt != null)
            {
                writer.Write(Aufsetzpunkt, FieldType.ALPHA_NUMERIC, max_length: 35);
            } else
            {
                writer.WriteEmpty();
            }
        }
    }

    public class HIKAZ4 : ISegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HIKAZ", Version = 4 };

        public byte[] BookedTransactions { get; set; } = null!;

        public byte[] UnbookedTransactions { get; set; } = null!;

        public static ISegment Read(MessageReader reader, SegmentId segmentId)
        {
            return new HIKAZ4
            {
                Head = segmentId,
                BookedTransactions = reader.ReadBytes(),
                UnbookedTransactions = reader.ReadBytes()
            };
        }

        public void Write(MessageWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    public class HIKAZS4 : ParameterSegment
    {
        public override SegmentId Head { get; protected set; } = new SegmentId { Name = "HIKAZS", Version = 4 };

        public int StorageDuration { get; set; }

        public bool NumberOfEntriesAllowed { get; set; }

        public static ISegment Read(MessageReader reader, SegmentId segmentId)
        {
            var param = new HIKAZS4
            {
                Head = segmentId,
            };
            param.Read(reader, true);
            reader.EnterGroup();
            param.StorageDuration = (int)reader.ReadInt()!;
            param.NumberOfEntriesAllowed = (bool)reader.ReadBool()!;
            reader.LeaveGroup();

            return param;
        }
    }

    public class HKKAZ5 : ISegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HKKAZ", Version = 5 };

        public Account Account { get; set; } = null!;

        public bool AllAccounts { get; set; }

        public int? MaximumEntries { get; set; }

        public string? Aufsetzpunkt { get; set; }

        public DateOnly? From { get; set; }

        public DateOnly? To { get; set; }

        public void Write(MessageWriter writer)
        {
            writer.Write(Account);
            writer.Write(AllAccounts, FieldType.YES_NO);
            if (From != null)
            {
                writer.Write((DateOnly)From, FieldType.DATE);
            }
            else
            {
                writer.WriteEmpty();
            }
            if (To != null)
            {
                writer.Write((DateOnly)To, FieldType.DATE);
            }
            else
            {
                writer.WriteEmpty();
            }
            if (MaximumEntries != null)
            {
                writer.Write((int)MaximumEntries, FieldType.NUMERIC, max_length: 4);
            }
            else
            {
                writer.WriteEmpty();
            }

            if (Aufsetzpunkt != null)
            {
                writer.Write(Aufsetzpunkt, FieldType.ALPHA_NUMERIC, max_length: 35);
            }
            else
            {
                writer.WriteEmpty();
            }
        }
    }

    public class HIKAZ5 : ISegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HIKAZ", Version = 5 };

        public byte[] BookedTransactions { get; set; } = null!;

        public byte[] UnbookedTransactions { get; set; } = null!;

        public static ISegment Read(MessageReader reader, SegmentId segmentId)
        {
            return new HIKAZ5
            {
                Head = segmentId,
                BookedTransactions = reader.ReadBytes(),
                UnbookedTransactions = reader.ReadBytes()
            };
        }

        public void Write(MessageWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    public class HIKAZS5 : ParameterSegment
    {
        public override SegmentId Head { get; protected set; } = new SegmentId { Name = "HIKAZS", Version = 5 };

        public int StorageDuration { get; set; }

        public bool NumberOfEntriesAllowed { get; set; }

        public bool AllAccountsAllowed { get; set; }

        public static ISegment Read(MessageReader reader, SegmentId segmentId)
        {
            var param = new HIKAZS5
            {
                Head = segmentId,
            };
            param.Read(reader, true);
            reader.EnterGroup();
            param.StorageDuration = (int)reader.ReadInt()!;
            param.NumberOfEntriesAllowed = (bool)reader.ReadBool()!;
            param.AllAccountsAllowed = (bool)reader.ReadBool()!;
            reader.LeaveGroup();

            return param;
        }
    }

    public class HKKAZ6 : ISegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HKKAZ", Version = 6 };

        public Account Account { get; set; } = null!;

        public bool AllAccounts { get; set; }

        public int? MaximumEntries { get; set; }

        public string? Aufsetzpunkt { get; set; }

        public DateOnly? From { get; set; }

        public DateOnly? To { get; set; }

        public void Write(MessageWriter writer)
        {
            writer.Write(Account);
            writer.Write(AllAccounts, FieldType.YES_NO);
            if (From != null)
            {
                writer.Write((DateOnly)From, FieldType.DATE);
            }
            else
            {
                writer.WriteEmpty();
            }
            if (To != null)
            {
                writer.Write((DateOnly)To, FieldType.DATE);
            }
            else
            {
                writer.WriteEmpty();
            }
            if (MaximumEntries != null)
            {
                writer.Write((int)MaximumEntries, FieldType.NUMERIC, max_length: 4);
            }
            else
            {
                writer.WriteEmpty();
            }

            if (Aufsetzpunkt != null)
            {
                writer.Write(Aufsetzpunkt, FieldType.ALPHA_NUMERIC, max_length: 35);
            }
            else
            {
                writer.WriteEmpty();
            }
        }
    }

    public class HIKAZ6 : ISegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HIKAZ", Version = 6 };

        public byte[] BookedTransactions { get; set; } = null!;

        public byte[] UnbookedTransactions { get; set; } = null!;

        public static ISegment Read(MessageReader reader, SegmentId segmentId)
        {
            return new HIKAZ6
            {
                Head = segmentId,
                BookedTransactions = reader.ReadBytes(),
                UnbookedTransactions = reader.ReadBytes()
            };
        }

        public void Write(MessageWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    public class HIKAZS6 : ParameterSegment
    {
        public override SegmentId Head { get; protected set; } = new SegmentId { Name = "HIKAZS", Version = 6 };

        public int StorageDuration { get; set; }

        public bool NumberOfEntriesAllowed { get; set; }

        public bool AllAccountsAllowed { get; set; }

        public static ISegment Read(MessageReader reader, SegmentId segmentId)
        {
            var param = new HIKAZS6
            {
                Head = segmentId,
            };
            param.Read(reader);
            reader.EnterGroup();
            param.StorageDuration = (int)reader.ReadInt()!;
            param.NumberOfEntriesAllowed = (bool)reader.ReadBool()!;
            param.AllAccountsAllowed = (bool)reader.ReadBool()!;
            reader.LeaveGroup();

            return param;
        }
    }

    public class HKKAZ7 : ISegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HKKAZ", Version = 7 };

        public AccountInternational Account { get; set; } = null!;

        public bool AllAccounts { get; set; }

        public int? MaximumEntries { get; set; }

        public string? Aufsetzpunkt { get; set; }

        public DateOnly? From { get; set; }

        public DateOnly? To { get; set; }

        public void Write(MessageWriter writer)
        {
            writer.Write(Account);
            writer.Write(AllAccounts, FieldType.YES_NO);
            if (From != null)
            {
                writer.Write((DateOnly)From, FieldType.DATE);
            }
            else
            {
                writer.WriteEmpty();
            }
            if (To != null)
            {
                writer.Write((DateOnly)To, FieldType.DATE);
            }
            else
            {
                writer.WriteEmpty();
            }
            if (MaximumEntries != null)
            {
                writer.Write((int)MaximumEntries, FieldType.NUMERIC, max_length: 4);
            }
            else
            {
                writer.WriteEmpty();
            }

            if (Aufsetzpunkt != null)
            {
                writer.Write(Aufsetzpunkt, FieldType.ALPHA_NUMERIC, max_length: 35);
            }
            else
            {
                writer.WriteEmpty();
            }
        }
    }

    public class HIKAZ7 : ISegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HIKAZ", Version = 7 };

        public byte[] BookedTransactions { get; set; } = null!;

        public byte[] UnbookedTransactions { get; set; } = null!;

        public static ISegment Read(MessageReader reader, SegmentId segmentId)
        {
            return new HIKAZ7
            {
                Head = segmentId,
                BookedTransactions = reader.ReadBytes(),
                UnbookedTransactions = reader.ReadBytes()
            };
        }

        public void Write(MessageWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    public class HIKAZS7 : ParameterSegment
    {
        public override SegmentId Head { get; protected set; } = new SegmentId { Name = "HIKAZS", Version = 7 };

        public int StorageDuration { get; set; }

        public bool NumberOfEntriesAllowed { get; set; }

        public bool AllAccountsAllowed { get; set; }

        public static ISegment Read(MessageReader reader, SegmentId segmentId)
        {
            var param = new HIKAZS7
            {
                Head = segmentId,
            };
            param.Read(reader);
            reader.EnterGroup();
            param.StorageDuration = (int)reader.ReadInt()!;
            param.NumberOfEntriesAllowed = (bool)reader.ReadBool()!;
            param.AllAccountsAllowed = (bool)reader.ReadBool()!;
            reader.LeaveGroup();

            return param;
        }
    }
}
