using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Generator
{
    public interface ISpanCodeGenerator
    {
        void GenerateCode(Span target, CodeGeneratorContext context);
    }
}
