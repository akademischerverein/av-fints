using AV.FinTS.Raw.Codes;

namespace AV.FinTS.Raw.Structures
{
    public class BankIdentifier : IElementStructure
    {
        public CountryCode CountryCode { get; set; }

        public string? BankId { get; set; } = null!;

        public static BankIdentifier? Read(MessageReader reader)
        {
            reader.EnterGroup();
            var cc = reader.ReadInt();
            var bankIdentifier = new BankIdentifier
            {
                BankId = reader.Read()
            };
            reader.LeaveGroup();

            if (cc == null && bankIdentifier.BankId == null)
            {
                return null;
            }
            bankIdentifier.CountryCode = (CountryCode)cc!;

            if (!Enum.IsDefined(bankIdentifier.CountryCode))
            {
                throw new InvalidDataException("Invalid country code");
            }

            return bankIdentifier;
        }

        public void Write(MessageWriter writer)
        {
            writer.Write(CountryCode);
            if (BankId != null)
            {
                writer.Write(BankId, FieldType.ALPHA_NUMERIC, max_length: 30);
            } else
            {
                writer.WriteEmpty();
            }
        }
    }
}
