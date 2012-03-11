using System.Web.Razor.Text;

namespace System.Web.Razor.Tokenizer.Symbols
{
    public interface ISymbol
    {
        SourceLocation Start { get; }
        string Content { get; }

        void OffsetStart(SourceLocation documentStart);
        void ChangeStart(SourceLocation newStart);
    }
}
