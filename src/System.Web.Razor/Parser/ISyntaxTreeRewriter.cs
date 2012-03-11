using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Parser
{
    internal interface ISyntaxTreeRewriter
    {
        Block Rewrite(Block input);
    }
}
