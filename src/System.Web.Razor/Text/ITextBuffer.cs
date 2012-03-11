namespace System.Web.Razor.Text
{
    public interface ITextBuffer
    {
        int Length { get; }
        int Position { get; set; }
        int Read();
        int Peek();
    }

    // TextBuffer with Location tracking
    public interface ITextDocument : ITextBuffer
    {
        SourceLocation Location { get; }
    }
}
