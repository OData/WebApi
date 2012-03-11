namespace System.Web.Razor.Parser.SyntaxTree
{
    public enum BlockType
    {
        // Code
        Statement,
        Directive,
        Functions,
        Expression,
        Helper,

        // Markup
        Markup,
        Section,
        Template,

        // Special
        Comment
    }
}
