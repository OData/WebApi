using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Editor
{
    public class EditResult
    {
        public EditResult(PartialParseResult result, SpanBuilder editedSpan)
        {
            Result = result;
            EditedSpan = editedSpan;
        }

        public PartialParseResult Result { get; set; }
        public SpanBuilder EditedSpan { get; set; }
    }
}
