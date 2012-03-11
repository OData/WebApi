using System.Web.Razor.Parser.SyntaxTree;
using System.Web.Razor.Resources;
using System.Web.Razor.Tokenizer.Symbols;

namespace System.Web.Razor.Parser
{
    public partial class HtmlMarkupParser
    {
        public override void ParseDocument()
        {
            if (Context == null)
            {
                throw new InvalidOperationException(RazorResources.Parser_Context_Not_Set);
            }

            using (PushSpanConfig(DefaultMarkupSpan))
            {
                using (Context.StartBlock(BlockType.Markup))
                {
                    NextToken();
                    while (!EndOfFile)
                    {
                        SkipToAndParseCode(HtmlSymbolType.OpenAngle);
                        if (Optional(HtmlSymbolType.OpenAngle) && !At(HtmlSymbolType.Solidus))
                        {
                            Optional(HtmlSymbolType.Text); // Tag Name, but we don't care what it is
                            TagContent(); // Parse the tag, don't care about the content
                        }
                    }
                    AddMarkerSymbolIfNecessary();
                    Output(SpanKind.Markup);
                }
            }
        }
    }
}
