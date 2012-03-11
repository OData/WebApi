using System.IO;

namespace System.Web.Razor.Text
{
    public abstract class LookaheadTextReader : TextReader
    {
        public abstract SourceLocation CurrentLocation { get; }
        public abstract IDisposable BeginLookahead();
        public abstract void CancelBacktrack();
    }
}
