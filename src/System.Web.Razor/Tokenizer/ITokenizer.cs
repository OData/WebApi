using System.Web.Razor.Tokenizer.Symbols;

namespace System.Web.Razor.Tokenizer
{
    public interface ITokenizer
    {
        ISymbol NextSymbol();
    }
}
