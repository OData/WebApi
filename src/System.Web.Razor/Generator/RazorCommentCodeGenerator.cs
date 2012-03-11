using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Generator
{
    public class RazorCommentCodeGenerator : BlockCodeGenerator
    {
        public override void GenerateStartBlockCode(Block target, CodeGeneratorContext context)
        {
            // Flush the buffered statement since we're interrupting it with a comment.
            if (!String.IsNullOrEmpty(context.CurrentBufferedStatement))
            {
                context.MarkEndOfGeneratedCode();
                context.BufferStatementFragment(context.BuildCodeString(cw => cw.WriteLineContinuation()));
            }
            context.FlushBufferedStatement();
        }
    }
}
