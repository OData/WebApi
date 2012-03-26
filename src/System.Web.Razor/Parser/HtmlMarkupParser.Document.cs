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
                            bool scriptTag = At(HtmlSymbolType.Text) &&
                                             String.Equals(CurrentSymbol.Content, "script", StringComparison.OrdinalIgnoreCase);
                            Optional(HtmlSymbolType.Text);
                            TagContent(); // Parse the tag, don't care about the content
                            Optional(HtmlSymbolType.Solidus);
                            Optional(HtmlSymbolType.CloseAngle);
                            if (scriptTag)
                            {
                                SkipToEndScriptAndParseCode();
                            }
                        }
                    }
                    AddMarkerSymbolIfNecessary();
                    Output(SpanKind.Markup);
                }
            }
        }
    }
}
