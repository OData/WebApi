namespace System.Web.Razor.Tokenizer
{
    public static class VBHelpers
    {
        public static bool IsSingleQuote(char character)
        {
            return character == '\'' || character == '‘' || character == '’';
        }

        public static bool IsDoubleQuote(char character)
        {
            return character == '"' || character == '“' || character == '”';
        }

        public static bool IsOctalDigit(char character)
        {
            return character >= '0' && character <= '7';
        }
    }
}
