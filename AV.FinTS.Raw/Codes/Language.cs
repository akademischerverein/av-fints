namespace AV.FinTS.Raw.Codes
{
    public enum Language
    {
        DEFAULT = 0,
        GERMAN = 1,
        ENGLISH = 2,
        FRENCH = 3
    }

    public static class LanguageExtensions
    {
        public static void Write(this MessageWriter writer, Language language)
        {
            writer.Write((int)language, FieldType.NUMERIC, max_length: 3);
        }
    }
}
