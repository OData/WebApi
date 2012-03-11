namespace System.Web.Razor.Utils
{
    internal static class CharUtils
    {
        internal static bool IsNonNewLineWhitespace(char c)
        {
            return Char.IsWhiteSpace(c) && !IsNewLine(c);
        }

        internal static bool IsNewLine(char c)
        {
            return c == 0x000d // Carriage return
                   || c == 0x000a // Linefeed
                   || c == 0x2028 // Line separator
                   || c == 0x2029; // Paragraph separator
        }
    }
}
