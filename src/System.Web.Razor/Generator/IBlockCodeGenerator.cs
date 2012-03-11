using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Generator
{
    public interface IBlockCodeGenerator
    {
        void GenerateStartBlockCode(Block target, CodeGeneratorContext context);
        void GenerateEndBlockCode(Block target, CodeGeneratorContext context);
    }
}
