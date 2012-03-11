namespace System.Web.Razor.Tokenizer.Symbols
{
    public enum HtmlSymbolType
    {
        Unknown,
        Text, // Text which isn't one of the below
        WhiteSpace, // Non-newline Whitespace
        NewLine, // Newline
        OpenAngle, // <
        Bang, // !
        Solidus, // /
        QuestionMark, // ?
        DoubleHyphen, // --
        LeftBracket, // [
        CloseAngle, // >
        RightBracket, // ]
        Equals, // =
        DoubleQuote, // "
        SingleQuote, // '
        Transition, // @
        Colon,
        RazorComment,
        RazorCommentStar,
        RazorCommentTransition
    }
}
