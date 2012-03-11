using System.Globalization;
using System.Web.Razor.Text;

namespace System.Web.Razor.Generator
{
    internal static class CodeWriterExtensions
    {
        public static void WriteLocationTaggedString(this CodeWriter writer, LocationTagged<string> value)
        {
            writer.WriteStartMethodInvoke("Tuple.Create");
            writer.WriteStringLiteral(value.Value);
            writer.WriteParameterSeparator();
            writer.WriteSnippet(value.Location.AbsoluteIndex.ToString(CultureInfo.CurrentCulture));
            writer.WriteEndMethodInvoke();
        }
    }
}
