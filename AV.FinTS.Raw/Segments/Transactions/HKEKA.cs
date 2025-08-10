using AV.FinTS.Raw.Codes;
using AV.FinTS.Raw.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Raw.Segments.Transactions
{
    public class HKEKA3 : IPickUpSegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HKEKA", Version = 3 };

        public Account Account { get; set; } = null!;

        public AccountStatementFormat? Format { get; set; }

        public int? Number { get; set; }

        public int? Year { get; set; }

        public int? MaximumEntries { get; set; }

        public string? Aufsetzpunkt { get; set; }

        public void Write(MessageWriter writer)
        {
            writer.Write(Account);
            writer.Write((int?)Format, FieldType.NUMERIC, max_length: 1);
            writer.Write(Number, FieldType.NUMERIC, max_length: 5);
            writer.Write(Year, FieldType.DIGITS, length: 4);
            writer.Write(MaximumEntries, FieldType.NUMERIC, max_length: 4);
            writer.Write(Aufsetzpunkt, FieldType.ALPHA_NUMERIC, max_length: 35);
        }
    }

    public class HIEKA3 : ISegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HIEKA", Version = 3 };

        public AccountStatementFormat Format { get; set; }

        public DateOnly From { get; set; }

        public DateOnly? To { get; set; }

        public byte[] Data { get; set; } = null!;

        public string? InformationForAccountStatement { get; set; }

        public string? InformationOnCustomerConditions { get; set; }

        public string? AdvertisementText { get; set; }

        public string? Iban { get; set; }

        public string? Bic { get; set; }

        public string? Name { get; set; }

        public string? NameAffix { get; set; }

        public byte[] ReceiptCode { get; set; } = null!;

        public static ISegment Read(MessageReader reader, SegmentId head)
        {
            var segment = new HIEKA3
            {
                Head = head,
                Format = (AccountStatementFormat)reader.ReadInt()!,
            };
            reader.EnterGroup();
            segment.From = (DateOnly)reader.ReadDate()!;
            segment.To = reader.ReadDate();
            reader.LeaveGroup();
            segment.Data = reader.ReadBytes();
            segment.InformationForAccountStatement = reader.Read();
            segment.InformationOnCustomerConditions = reader.Read();
            segment.AdvertisementText = reader.Read();
            segment.Iban = reader.Read();
            segment.Bic = reader.Read();
            segment.Name = reader.Read();
            var name2 = reader.Read();
            if (name2 != null)
            {
                segment.Name += name2;
            }
            segment.NameAffix = reader.Read();
            segment.ReceiptCode = reader.ReadBytes();
            return segment;
        }

        public void Write(MessageWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    public class HIEKAS3 : ParameterSegment
    {
        public override SegmentId Head { get; protected set; } = new SegmentId { Name = "HIEKAS", Version = 3 };

        public bool NumberAllowed { get; set; }

        public bool ReceiptConfirmationRequired { get; set; }

        public bool NumEntriesAllowed { get; set; }

        public List<AccountStatementFormat> SupportedFormats { get; set; } = new();

        public static ISegment Read(MessageReader reader, SegmentId head)
        {
            var param = new HIEKAS3 { Head = head };
            param.Read(reader);

            reader.EnterGroup();
            param.NumberAllowed = (bool)reader.ReadBool()!;
            param.ReceiptConfirmationRequired = (bool)reader.ReadBool()!;
            param.NumEntriesAllowed = (bool)reader.ReadBool()!;

            while (!reader.GroupEnded)
            {
                param.SupportedFormats.Add((AccountStatementFormat)reader.ReadInt()!);
            }
            reader.LeaveGroup();

            return param;
        }
    }

    public class HKEKA4 : IPickUpSegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HKEKA", Version = 4 };

        public AccountInternational Account { get; set; } = null!;

        public AccountStatementFormat? Format { get; set; }

        public int? Number { get; set; }

        public int? Year { get; set; }

        public int? MaximumEntries { get; set; }

        public string? Aufsetzpunkt { get; set; }

        public void Write(MessageWriter writer)
        {
            writer.Write(Account);
            writer.Write((int?)Format, FieldType.NUMERIC, max_length: 1);
            writer.Write(Number, FieldType.NUMERIC, max_length: 5);
            writer.Write(Year, FieldType.DIGITS, length: 4);
            writer.Write(MaximumEntries, FieldType.NUMERIC, max_length: 4);
            writer.Write(Aufsetzpunkt, FieldType.ALPHA_NUMERIC, max_length: 35);
        }
    }

    public class HIEKA4 : ISegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HIEKA", Version = 4 };

        public AccountStatementFormat Format { get; set; }

        public DateOnly From { get; set; }

        public DateOnly? To { get; set; }

        public byte[] Data { get; set; } = null!;

        public string? InformationForAccountStatement { get; set; }

        public string? InformationOnCustomerConditions { get; set; }

        public string? AdvertisementText { get; set; }

        public string? Iban { get; set; }

        public string? Bic { get; set; }

        public string? Name { get; set; }

        public string? NameAffix { get; set; }

        public byte[] ReceiptCode { get; set; } = null!;

        public static ISegment Read(MessageReader reader, SegmentId head)
        {
            var segment = new HIEKA4
            {
                Head = head,
                Format = (AccountStatementFormat)reader.ReadInt()!,
            };
            reader.EnterGroup();
            segment.From = (DateOnly)reader.ReadDate()!;
            segment.To = reader.ReadDate();
            reader.LeaveGroup();
            segment.Data = reader.ReadBytes();
            segment.InformationForAccountStatement = reader.Read();
            segment.InformationOnCustomerConditions = reader.Read();
            segment.AdvertisementText = reader.Read();
            segment.Iban = reader.Read();
            segment.Bic = reader.Read();
            segment.Name = reader.Read();
            var name2 = reader.Read();
            if (name2 != null)
            {
                segment.Name += name2;
            }
            segment.NameAffix = reader.Read();
            segment.ReceiptCode = reader.ReadBytes();
            return segment;
        }

        public void Write(MessageWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    public class HIEKAS4 : ParameterSegment
    {
        public override SegmentId Head { get; protected set; } = new SegmentId { Name = "HIEKAS", Version = 4 };

        public bool NumberAllowed { get; set; }

        public bool ReceiptConfirmationRequired { get; set; }

        public bool NumEntriesAllowed { get; set; }

        public List<AccountStatementFormat> SupportedFormats { get; set; } = new();

        public static ISegment Read(MessageReader reader, SegmentId head)
        {
            var param = new HIEKAS4 { Head = head };
            param.Read(reader);

            reader.EnterGroup();
            param.NumberAllowed = (bool)reader.ReadBool()!;
            param.ReceiptConfirmationRequired = (bool)reader.ReadBool()!;
            param.NumEntriesAllowed = (bool)reader.ReadBool()!;

            while (!reader.GroupEnded)
            {
                param.SupportedFormats.Add((AccountStatementFormat)reader.ReadInt()!);
            }
            reader.LeaveGroup();

            return param;
        }
    }

    public class HKEKA5 : IPickUpSegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HKEKA", Version = 5 };

        public AccountInternational Account { get; set; } = null!;

        public AccountStatementFormat? Format { get; set; }

        public int? Number { get; set; }

        public int? Year { get; set; }

        public int? MaximumEntries { get; set; }

        public string? Aufsetzpunkt { get; set; }

        public void Write(MessageWriter writer)
        {
            writer.Write(Account);
            writer.Write((int?)Format, FieldType.NUMERIC, max_length: 1);
            writer.Write(Number, FieldType.NUMERIC, max_length: 5);
            writer.Write(Year, FieldType.DIGITS, length: 4);
            writer.Write(MaximumEntries, FieldType.NUMERIC, max_length: 4);
            writer.Write(Aufsetzpunkt, FieldType.ALPHA_NUMERIC, max_length: 35);
        }
    }

    public class HIEKA5 : ISegment
    {
        public SegmentId Head { get; private set; } = new SegmentId { Name = "HIEKA", Version = 5 };

        public AccountStatementFormat Format { get; set; }

        public DateOnly From { get; set; }

        public DateOnly? To { get; set; }

        public DateOnly? CreationDate { get; set; }

        public int? Year{ get; set; }

        public int? Number { get; set; }

        public byte[] Data { get; set; } = null!;

        public string? InformationForAccountStatement { get; set; }

        public string? InformationOnCustomerConditions { get; set; }

        public string? AdvertisementText { get; set; }

        public string? Iban { get; set; }

        public string? Bic { get; set; }

        public string? Name { get; set; }

        public string? NameAffix { get; set; }

        public byte[] ReceiptCode { get; set; } = null!;

        public static ISegment Read(MessageReader reader, SegmentId head)
        {
            var segment = new HIEKA5
            {
                Head = head,
                Format = (AccountStatementFormat)reader.ReadInt()!,
            };
            reader.EnterGroup();
            segment.From = (DateOnly)reader.ReadDate()!;
            segment.To = reader.ReadDate();
            reader.LeaveGroup();
            segment.CreationDate = reader.ReadDate();
            segment.Year = reader.ReadInt();
            segment.Number = reader.ReadInt();
            segment.Data = reader.ReadBytes();
            segment.InformationForAccountStatement = reader.Read();
            segment.InformationOnCustomerConditions = reader.Read();
            segment.AdvertisementText = reader.Read();
            segment.Iban = reader.Read();
            segment.Bic = reader.Read();
            segment.Name = reader.Read();
            var name2 = reader.Read();
            if (name2 != null)
            {
                segment.Name += name2;
            }
            segment.NameAffix = reader.Read();
            segment.ReceiptCode = reader.ReadBytes();
            return segment;
        }

        public void Write(MessageWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    public class HIEKAS5 : ParameterSegment
    {
        public override SegmentId Head { get; protected set; } = new SegmentId { Name = "HIEKAS", Version = 5 };

        public bool NumberAllowed { get; set; }

        public bool ReceiptConfirmationRequired { get; set; }

        public bool NumEntriesAllowed { get; set; }

        public List<AccountStatementFormat> SupportedFormats { get; set; } = new();

        public static ISegment Read(MessageReader reader, SegmentId head)
        {
            var param = new HIEKAS5 { Head = head };
            param.Read(reader);

            reader.EnterGroup();
            param.NumberAllowed = (bool)reader.ReadBool()!;
            param.ReceiptConfirmationRequired = (bool)reader.ReadBool()!;
            param.NumEntriesAllowed = (bool)reader.ReadBool()!;

            while (!reader.GroupEnded)
            {
                param.SupportedFormats.Add((AccountStatementFormat)reader.ReadInt()!);
            }
            reader.LeaveGroup();

            return param;
        }
    }
}
