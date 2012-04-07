// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Razor.Parser.SyntaxTree;
using System.Web.Razor.Test.Framework;
using System.Web.Razor.Tokenizer.Symbols;
using Xunit;

namespace System.Web.Razor.Test.Parser.VB
{
    public class VBHtmlDocumentTest : VBHtmlMarkupParserTestBase
    {
        [Fact]
        public void BlockCommentInMarkupDocumentIsHandledCorrectly()
        {
            ParseDocumentTest(@"<ul>
                @* This is a block comment </ul> *@ foo",
                new MarkupBlock(
                    Factory.Markup("<ul>\r\n                "),
                    new CommentBlock(
                        Factory.MarkupTransition(HtmlSymbolType.RazorCommentTransition),
                        Factory.MetaMarkup("*", HtmlSymbolType.RazorCommentStar).Accepts(AcceptedCharacters.None),
                        Factory.Comment(" This is a block comment </ul> ", HtmlSymbolType.RazorComment),
                        Factory.MetaMarkup("*", HtmlSymbolType.RazorCommentStar).Accepts(AcceptedCharacters.None),
                        Factory.MarkupTransition(HtmlSymbolType.RazorCommentTransition)),
                    Factory.Markup(" foo")));
        }

        [Fact]
        public void BlockCommentInMarkupBlockIsHandledCorrectly()
        {
            ParseBlockTest(@"<ul>
                @* This is a block comment </ul> *@ foo </ul>",
                new MarkupBlock(
                    Factory.Markup("<ul>\r\n                "),
                    new CommentBlock(
                        Factory.MarkupTransition(HtmlSymbolType.RazorCommentTransition),
                        Factory.MetaMarkup("*", HtmlSymbolType.RazorCommentStar).Accepts(AcceptedCharacters.None),
                        Factory.Comment(" This is a block comment </ul> ", HtmlSymbolType.RazorComment),
                        Factory.MetaMarkup("*", HtmlSymbolType.RazorCommentStar).Accepts(AcceptedCharacters.None),
                        Factory.MarkupTransition(HtmlSymbolType.RazorCommentTransition)),
                    Factory.Markup(" foo </ul>").Accepts(AcceptedCharacters.None)));
        }

        [Fact]
        public void BlockCommentAtStatementStartInCodeBlockIsHandledCorrectly()
        {
            ParseDocumentTest(@"@If Request.IsAuthenticated Then
    @* User is logged in! End If *@
    Write(""Hello friend!"")
End If",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new StatementBlock(
                        Factory.CodeTransition(),
                        Factory.Code("If Request.IsAuthenticated Then\r\n    ").AsStatement(),
                        new CommentBlock(
                            Factory.CodeTransition(VBSymbolType.RazorCommentTransition),
                            Factory.MetaCode("*", VBSymbolType.RazorCommentStar).Accepts(AcceptedCharacters.None),
                            Factory.Comment(" User is logged in! End If ", VBSymbolType.RazorComment),
                            Factory.MetaCode("*", VBSymbolType.RazorCommentStar).Accepts(AcceptedCharacters.None),
                            Factory.CodeTransition(VBSymbolType.RazorCommentTransition)),
                        Factory.Code("\r\n    Write(\"Hello friend!\")\r\nEnd If")
                               .AsStatement()
                               .Accepts(AcceptedCharacters.None)),
                    Factory.EmptyHtml()));
        }

        [Fact]
        public void BlockCommentInStatementInCodeBlockIsHandledCorrectly()
        {
            ParseDocumentTest(@"@If Request.IsAuthenticated Then
    Dim foo = @* User is logged in! End If *@ bar
    Write(""Hello friend!"")
End If",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new StatementBlock(
                        Factory.CodeTransition(),
                        Factory.Code("If Request.IsAuthenticated Then\r\n    Dim foo = ").AsStatement(),
                        new CommentBlock(
                            Factory.CodeTransition(VBSymbolType.RazorCommentTransition),
                            Factory.MetaCode("*", VBSymbolType.RazorCommentStar).Accepts(AcceptedCharacters.None),
                            Factory.Comment(" User is logged in! End If ", VBSymbolType.RazorComment),
                            Factory.MetaCode("*", VBSymbolType.RazorCommentStar).Accepts(AcceptedCharacters.None),
                            Factory.CodeTransition(VBSymbolType.RazorCommentTransition)),
                        Factory.Code(" bar\r\n    Write(\"Hello friend!\")\r\nEnd If")
                               .AsStatement()
                               .Accepts(AcceptedCharacters.None)),
                    Factory.EmptyHtml()));
        }

        [Fact]
        public void BlockCommentInStringInCodeBlockIsIgnored()
        {
            ParseDocumentTest(@"@If Request.IsAuthenticated Then
    Dim foo = ""@* User is logged in! End If *@ bar""
    Write(""Hello friend!"")
End If",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new StatementBlock(
                        Factory.CodeTransition(),
                        Factory.Code("If Request.IsAuthenticated Then\r\n    Dim foo = \"@* User is logged in! End If *@ bar\"\r\n    Write(\"Hello friend!\")\r\nEnd If")
                               .AsStatement()
                               .Accepts(AcceptedCharacters.None)),
                    Factory.EmptyHtml()));
        }

        [Fact]
        public void BlockCommentInTickCommentInCodeBlockIsIgnored()
        {
            ParseDocumentTest(@"@If Request.IsAuthenticated Then
    Dim foo = '@* User is logged in! End If *@ bar
    Write(""Hello friend!"")
End If",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new StatementBlock(
                        Factory.CodeTransition(),
                        Factory.Code("If Request.IsAuthenticated Then\r\n    Dim foo = '@* User is logged in! End If *@ bar\r\n    Write(\"Hello friend!\")\r\nEnd If")
                               .AsStatement()
                               .Accepts(AcceptedCharacters.None)),
                    Factory.EmptyHtml()));
        }

        [Fact]
        public void BlockCommentInRemCommentInCodeBlockIsIgnored()
        {
            ParseDocumentTest(@"@If Request.IsAuthenticated Then
    Dim foo = REM @* User is logged in! End If *@ bar
    Write(""Hello friend!"")
End If",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new StatementBlock(
                        Factory.CodeTransition(),
                        Factory.Code("If Request.IsAuthenticated Then\r\n    Dim foo = REM @* User is logged in! End If *@ bar\r\n    Write(\"Hello friend!\")\r\nEnd If")
                               .AsStatement()
                               .Accepts(AcceptedCharacters.None)),
                    Factory.EmptyHtml()));
        }

        [Fact]
        public void BlockCommentInImplicitExpressionIsHandledCorrectly()
        {
            ParseDocumentTest("@Html.Foo@*bar*@",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new ExpressionBlock(
                        Factory.CodeTransition(),
                        Factory.Code("Html.Foo")
                               .AsImplicitExpression(KeywordSet)
                               .Accepts(AcceptedCharacters.NonWhiteSpace)),
                    Factory.EmptyHtml(),
                    new CommentBlock(
                        Factory.MarkupTransition(HtmlSymbolType.RazorCommentTransition),
                        Factory.MetaMarkup("*", HtmlSymbolType.RazorCommentStar).Accepts(AcceptedCharacters.None),
                        Factory.Comment("bar", HtmlSymbolType.RazorComment),
                        Factory.MetaMarkup("*", HtmlSymbolType.RazorCommentStar).Accepts(AcceptedCharacters.None),
                        Factory.MarkupTransition(HtmlSymbolType.RazorCommentTransition)),
                    Factory.EmptyHtml()));
        }

        [Fact]
        public void BlockCommentAfterDotOfImplicitExpressionIsHandledCorrectly()
        {
            ParseDocumentTest("@Html.@*bar*@",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new ExpressionBlock(
                        Factory.CodeTransition(),
                        Factory.Code("Html")
                               .AsImplicitExpression(KeywordSet)
                               .Accepts(AcceptedCharacters.NonWhiteSpace)),
                    Factory.Markup("."),
                    new CommentBlock(
                        Factory.MarkupTransition(HtmlSymbolType.RazorCommentTransition),
                        Factory.MetaMarkup("*", HtmlSymbolType.RazorCommentStar).Accepts(AcceptedCharacters.None),
                        Factory.Comment("bar", HtmlSymbolType.RazorComment),
                        Factory.MetaMarkup("*", HtmlSymbolType.RazorCommentStar).Accepts(AcceptedCharacters.None),
                        Factory.MarkupTransition(HtmlSymbolType.RazorCommentTransition)),
                    Factory.EmptyHtml()));
        }

        [Fact]
        public void BlockCommentInParensOfImplicitExpressionIsHandledCorrectly()
        {
            ParseDocumentTest("@Html.Foo(@*bar*@ 4)",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new ExpressionBlock(
                        Factory.CodeTransition(),
                        Factory.Code("Html.Foo(")
                               .AsImplicitExpression(KeywordSet)
                               .Accepts(AcceptedCharacters.Any),
                        new CommentBlock(
                            Factory.CodeTransition(VBSymbolType.RazorCommentTransition),
                            Factory.MetaCode("*", VBSymbolType.RazorCommentStar).Accepts(AcceptedCharacters.None),
                            Factory.Comment("bar", VBSymbolType.RazorComment),
                            Factory.MetaCode("*", VBSymbolType.RazorCommentStar).Accepts(AcceptedCharacters.None),
                            Factory.CodeTransition(VBSymbolType.RazorCommentTransition)),
                        Factory.Code(" 4)")
                               .AsImplicitExpression(KeywordSet)
                               .Accepts(AcceptedCharacters.NonWhiteSpace)),
                    Factory.EmptyHtml()));
        }

        [Fact]
        public void BlockCommentInConditionIsHandledCorrectly()
        {
            ParseDocumentTest("@If @*bar*@ Then End If",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new StatementBlock(
                        Factory.CodeTransition(),
                        Factory.Code("If ").AsStatement(),
                        new CommentBlock(
                            Factory.CodeTransition(VBSymbolType.RazorCommentTransition),
                            Factory.MetaCode("*", VBSymbolType.RazorCommentStar).Accepts(AcceptedCharacters.None),
                            Factory.Comment("bar", VBSymbolType.RazorComment),
                            Factory.MetaCode("*", VBSymbolType.RazorCommentStar).Accepts(AcceptedCharacters.None),
                            Factory.CodeTransition(VBSymbolType.RazorCommentTransition)),
                        Factory.Code(" Then End If").AsStatement().Accepts(AcceptedCharacters.None)),
                    Factory.EmptyHtml()));
        }

        [Fact]
        public void BlockCommentInExplicitExpressionIsHandledCorrectly()
        {
            ParseDocumentTest(@"@(1 + @*bar*@ 1)",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new ExpressionBlock(
                        Factory.CodeTransition(),
                        Factory.MetaCode("(").Accepts(AcceptedCharacters.None),
                        Factory.Code(@"1 + ").AsExpression(),
                        new CommentBlock(
                            Factory.CodeTransition(VBSymbolType.RazorCommentTransition),
                            Factory.MetaCode("*", VBSymbolType.RazorCommentStar).Accepts(AcceptedCharacters.None),
                            Factory.Comment("bar", VBSymbolType.RazorComment),
                            Factory.MetaCode("*", VBSymbolType.RazorCommentStar).Accepts(AcceptedCharacters.None),
                            Factory.CodeTransition(VBSymbolType.RazorCommentTransition)
                            ),
                        Factory.Code(" 1").AsExpression(),
                        Factory.MetaCode(")").Accepts(AcceptedCharacters.None)
                        ),
                    Factory.EmptyHtml()));
        }
    }
}
