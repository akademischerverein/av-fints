namespace AV.FinTS.Raw.Codes
{
    public enum CountryCode
    {
        GERMANY = 276,
        EAST_GERMANY = 278,
        WEST_GERMANY = 280
    }

    public static class CountryCodeExtensions
    {
        public static void Write(this MessageWriter writer, CountryCode cc)
        {
            writer.Write((int)cc, FieldType.DIGITS, length: 3);
        }
    }
}
