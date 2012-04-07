// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Razor.Parser;
using System.Web.Razor.Resources;
using System.Web.Razor.Test.Framework;
using System.Web.Razor.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Test.Parser.CSharp {
    [TestClass]
    public class CsHtmlDocumentTest : CsHtmlMarkupParserTestBase {
        [TestMethod]
        public void UnterminatedBlockCommentCausesRazorError() {
            ParseDocumentTest(@"@* Foo Bar",
                new MarkupBlock(
                    new MarkupSpan(String.Empty),
                    new CommentBlock(
                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new CommentSpan(" Foo Bar")
                    )
                ),
                new RazorError(RazorResources.ParseError_RazorComment_Not_Terminated, SourceLocation.Zero));
        }

        [TestMethod]
        public void BlockCommentInMarkupDocumentIsHandledCorrectly() {
            ParseDocumentTest(@"<ul>
                @* This is a block comment </ul> *@ foo",
                new MarkupBlock(
                    new MarkupSpan(@"<ul>
                "),
                    new CommentBlock(
                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new CommentSpan(" This is a block comment </ul> "),
                        new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None)
                    ),
                    new MarkupSpan(" foo")
                ));
        }

        [TestMethod]
        public void BlockCommentInMarkupBlockIsHandledCorrectly() {
            ParseBlockTest(@"<ul>
                @* This is a block comment </ul> *@ foo </ul>",
                new MarkupBlock(
                    new MarkupSpan(@"<ul>
                "),
                    new CommentBlock(
                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new CommentSpan(" This is a block comment </ul> "),
                        new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None)
                    ),
                    new MarkupSpan(" foo </ul>", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                ));
        }

        [TestMethod]
        public void BlockCommentAtStatementStartInCodeBlockIsHandledCorrectly() {
            ParseDocumentTest(@"@if(Request.IsAuthenticated) {
    @* User is logged in! } *@
    Write(""Hello friend!"");
}",
                new MarkupBlock(
                    new StatementBlock(
                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new CodeSpan(@"if(Request.IsAuthenticated) {
    "),
                        new CommentBlock(
                            new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                            new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                            new CommentSpan(" User is logged in! } "),
                            new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                            new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None)
                        ),
                        new CodeSpan(@"
    Write(""Hello friend!"");
}"))));
        }

        [TestMethod]
        public void BlockCommentInStatementInCodeBlockIsHandledCorrectly() {
            ParseDocumentTest(@"@if(Request.IsAuthenticated) {
    var foo = @* User is logged in! ; *@;
    Write(""Hello friend!"");
}",
                new MarkupBlock(
                    new StatementBlock(
                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new CodeSpan(@"if(Request.IsAuthenticated) {
    var foo = "),
                        new CommentBlock(
                            new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                            new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                            new CommentSpan(" User is logged in! ; "),
                            new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                            new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None)
                        ),
                        new CodeSpan(@";
    Write(""Hello friend!"");
}"))));
        }

        [TestMethod]
        public void BlockCommentInStringIsIgnored() {
            ParseDocumentTest(@"@if(Request.IsAuthenticated) {
    var foo = ""@* User is logged in! ; *@"";
    Write(""Hello friend!"");
}",
                new MarkupBlock(
                    new StatementBlock(
                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new CodeSpan(@"if(Request.IsAuthenticated) {
    var foo = ""@* User is logged in! ; *@"";
    Write(""Hello friend!"");
}"))));
        }

        [TestMethod]
        public void BlockCommentInCSharpBlockCommentIsIgnored() {
            ParseDocumentTest(@"@if(Request.IsAuthenticated) {
    var foo = /*@* User is logged in! */ *@ */;
    Write(""Hello friend!"");
}",
                new MarkupBlock(
                    new StatementBlock(
                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new CodeSpan(@"if(Request.IsAuthenticated) {
    var foo = /*@* User is logged in! */ *@ */;
    Write(""Hello friend!"");
}"))));
        }

        [TestMethod]
        public void BlockCommentInCSharpLineCommentIsIgnored() {
            ParseDocumentTest(@"@if(Request.IsAuthenticated) {
    var foo = //@* User is logged in! */ *@;
    Write(""Hello friend!"");
}",
                new MarkupBlock(
                    new StatementBlock(
                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new CodeSpan(@"if(Request.IsAuthenticated) {
    var foo = //@* User is logged in! */ *@;
    Write(""Hello friend!"");
}"))));
        }

        [TestMethod]
        public void BlockCommentInImplicitExpressionIsHandledCorrectly() {
            ParseDocumentTest(@"@Html.Foo@*bar*@",
                new MarkupBlock(
                    new ExpressionBlock(
                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new ImplicitExpressionSpan(@"Html.Foo", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                    ),
                    new MarkupSpan(String.Empty),
                    new CommentBlock(
                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new CommentSpan("bar"),
                        new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None)
                    ),
                    new MarkupSpan(String.Empty)));
        }

        [TestMethod]
        public void BlockCommentAfterDotOfImplicitExpressionIsHandledCorrectly() {
            ParseDocumentTest(@"@Html.@*bar*@",
                new MarkupBlock(
                    new ExpressionBlock(
                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new ImplicitExpressionSpan(@"Html", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                    ),
                    new MarkupSpan("."),
                    new CommentBlock(
                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new CommentSpan("bar"),
                        new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None)
                    ),
                    new MarkupSpan(String.Empty)));
        }

        [TestMethod]
        public void BlockCommentInParensOfImplicitExpressionIsHandledCorrectly() {
            ParseDocumentTest(@"@Html.Foo(@*bar*@ 4)",
                new MarkupBlock(
                    new ExpressionBlock(
                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new ImplicitExpressionSpan(@"Html.Foo(", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.Any),
                        new CommentBlock(
                            new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                            new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                            new CommentSpan("bar"),
                            new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                            new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None)
                        ),
                        new ImplicitExpressionSpan(" 4)", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                    ),
                    new MarkupSpan(String.Empty)));
        }

        [TestMethod]
        public void BlockCommentInBracketsOfImplicitExpressionIsHandledCorrectly() {
            ParseDocumentTest(@"@Html.Foo[@*bar*@ 4]",
                new MarkupBlock(
                    new ExpressionBlock(
                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new ImplicitExpressionSpan(@"Html.Foo[", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.Any),
                        new CommentBlock(
                            new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                            new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                            new CommentSpan("bar"),
                            new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                            new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None)
                        ),
                        new ImplicitExpressionSpan(" 4]", CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false, acceptedCharacters: AcceptedCharacters.NonWhiteSpace)
                    ),
                    new MarkupSpan(String.Empty)));
        }

        [TestMethod]
        public void BlockCommentInParensOfConditionIsHandledCorrectly() {
            ParseDocumentTest(@"@if(@*bar*@) {}",
                new MarkupBlock(
                    new StatementBlock(
                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new CodeSpan(@"if("),
                        new CommentBlock(
                            new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                            new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                            new CommentSpan("bar"),
                            new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                            new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None)
                        ),
                        new CodeSpan(") {}")
                    )));
        }

        [TestMethod]
        public void BlockCommentInExplicitExpressionIsHandledCorrectly() {
            ParseDocumentTest(@"@(1 + @*bar*@ 1)",
                new MarkupBlock(
                    new ExpressionBlock(
                        new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new MetaCodeSpan("(", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                        new CodeSpan(@"1 + "),
                        new CommentBlock(
                            new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None),
                            new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                            new CommentSpan("bar"),
                            new MetaCodeSpan("*", hidden: false, acceptedCharacters: AcceptedCharacters.None),
                            new TransitionSpan(RazorParser.TransitionString, hidden: false, acceptedCharacters: AcceptedCharacters.None)
                        ),
                        new CodeSpan(" 1"),
                        new MetaCodeSpan(")", hidden: false, acceptedCharacters: AcceptedCharacters.None)
                    ),
                    new MarkupSpan(String.Empty)));
        }
    }
}
