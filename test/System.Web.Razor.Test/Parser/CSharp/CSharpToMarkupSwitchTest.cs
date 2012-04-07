// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Razor.Editor;
using System.Web.Razor.Parser;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.Razor.Resources;
using System.Web.Razor.Test.Framework;
using System.Web.Razor.Tokenizer.Symbols;
using Xunit;

namespace System.Web.Razor.Test.Parser.CSharp
{
    public class CSharpToMarkupSwitchTest : CsHtmlCodeParserTestBase
    {
        [Fact]
        public void SingleAngleBracketDoesNotCauseSwitchIfOuterBlockIsTerminated()
        {
            ParseBlockTest("{ List< }",
                new StatementBlock(
                    Factory.MetaCode("{").Accepts(AcceptedCharacters.None),
                    Factory.Code(" List< ").AsStatement(),
                    Factory.MetaCode("}").Accepts(AcceptedCharacters.None)));
        }

        [Fact]
        public void ParseBlockGivesSpacesToCodeOnAtTagTemplateTransitionInDesignTimeMode()
        {
            ParseBlockTest(@"Foo(    @<p>Foo</p>    )",
                           new ExpressionBlock(
                               Factory.Code(@"Foo(    ")
                                   .AsImplicitExpression(CSharpCodeParser.DefaultKeywords)
                                   .Accepts(AcceptedCharacters.Any),
                               new TemplateBlock(
                                   new MarkupBlock(
                                       Factory.MarkupTransition(),
                                       Factory.Markup("<p>Foo</p>").Accepts(AcceptedCharacters.None)
                                       )
                                   ),
                               Factory.Code(@"    )")
                                   .AsImplicitExpression(CSharpCodeParser.DefaultKeywords)
                                   .Accepts(AcceptedCharacters.NonWhiteSpace)
                               ), designTimeParser: true);
        }

        [Fact]
        public void ParseBlockGivesSpacesToCodeOnAtColonTemplateTransitionInDesignTimeMode()
        {
            ParseBlockTest(@"Foo(    
@:<p>Foo</p>    
)",
                           new ExpressionBlock(
                               Factory.Code("Foo(    \r\n").AsImplicitExpression(CSharpCodeParser.DefaultKeywords),
                               new TemplateBlock(
                                   new MarkupBlock(
                                       Factory.MarkupTransition(),
                                       Factory.MetaMarkup(":", HtmlSymbolType.Colon),
                                       Factory.Markup("<p>Foo</p>    \r\n")
                                           .With(new SingleLineMarkupEditHandler(CSharpLanguageCharacteristics.Instance.TokenizeString, AcceptedCharacters.None))
                                       )
                                   ),
                               Factory.Code(@")")
                                   .AsImplicitExpression(CSharpCodeParser.DefaultKeywords)
                                   .Accepts(AcceptedCharacters.NonWhiteSpace)
                               ), designTimeParser: true);
        }

        [Fact]
        public void ParseBlockGivesSpacesToCodeOnTagTransitionInDesignTimeMode()
        {
            ParseBlockTest(@"{
    <p>Foo</p>    
}",
                           new StatementBlock(
                               Factory.MetaCode("{").Accepts(AcceptedCharacters.None),
                               Factory.Code("\r\n    ").AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup("<p>Foo</p>").Accepts(AcceptedCharacters.None)
                                   ),
                               Factory.Code("    \r\n").AsStatement(),
                               Factory.MetaCode("}").Accepts(AcceptedCharacters.None)
                               ), designTimeParser: true);
        }

        [Fact]
        public void ParseBlockGivesSpacesToCodeOnInvalidAtTagTransitionInDesignTimeMode()
        {
            ParseBlockTest(@"{
    @<p>Foo</p>    
}",
                           new StatementBlock(
                               Factory.MetaCode("{").Accepts(AcceptedCharacters.None),
                               Factory.Code("\r\n    ").AsStatement(),
                               new MarkupBlock(
                                   Factory.MarkupTransition(),
                                   Factory.Markup("<p>Foo</p>").Accepts(AcceptedCharacters.None)
                                   ),
                               Factory.Code("    \r\n").AsStatement(),
                               Factory.MetaCode("}").Accepts(AcceptedCharacters.None)
                               ), true,
                           new RazorError(RazorResources.ParseError_AtInCode_Must_Be_Followed_By_Colon_Paren_Or_Identifier_Start, 7, 1, 4));
        }

        [Fact]
        public void ParseBlockGivesSpacesToCodeOnAtColonTransitionInDesignTimeMode()
        {
            ParseBlockTest(@"{
    @:<p>Foo</p>    
}",
                           new StatementBlock(
                               Factory.MetaCode("{").Accepts(AcceptedCharacters.None),
                               Factory.Code("\r\n    ").AsStatement(),
                               new MarkupBlock(
                                   Factory.MarkupTransition(),
                                   Factory.MetaMarkup(":", HtmlSymbolType.Colon),
                                   Factory.Markup("<p>Foo</p>    \r\n")
                                       .With(new SingleLineMarkupEditHandler(CSharpLanguageCharacteristics.Instance.TokenizeString, AcceptedCharacters.None))
                                   ),
                               Factory.EmptyCSharp().AsStatement(),
                               Factory.MetaCode("}").Accepts(AcceptedCharacters.None)
                               ), designTimeParser: true);
        }

        [Fact]
        public void ParseBlockShouldSupportSingleLineMarkupContainingStatementBlock()
        {
            ParseBlockTest(@"Repeat(10,
    @: @{}
)",
                           new ExpressionBlock(
                               Factory.Code("Repeat(10,\r\n    ")
                                   .AsImplicitExpression(CSharpCodeParser.DefaultKeywords),
                               new TemplateBlock(
                                   new MarkupBlock(
                                       Factory.MarkupTransition(),
                                       Factory.MetaMarkup(":", HtmlSymbolType.Colon),
                                       Factory.Markup(" ")
                                           .With(new SingleLineMarkupEditHandler(CSharpLanguageCharacteristics.Instance.TokenizeString)),
                                       new StatementBlock(
                                           Factory.CodeTransition(),
                                           Factory.MetaCode("{").Accepts(AcceptedCharacters.None),
                                           Factory.EmptyCSharp().AsStatement(),
                                           Factory.MetaCode("}").Accepts(AcceptedCharacters.None)
                                           ),
                                       Factory.Markup("\r\n")
                                           .With(new SingleLineMarkupEditHandler(CSharpLanguageCharacteristics.Instance.TokenizeString, AcceptedCharacters.None))
                                       )
                                   ),
                               Factory.Code(")")
                                   .AsImplicitExpression(CSharpCodeParser.DefaultKeywords)
                                   .Accepts(AcceptedCharacters.NonWhiteSpace)
                               ));
        }

        [Fact]
        public void ParseBlockShouldSupportMarkupWithoutPreceedingWhitespace()
        {
            ParseBlockTest(@"foreach(var file in files){


@:Baz
<br/>
<a>Foo</a>
@:Bar
}",
                           new StatementBlock(
                               Factory.Code("foreach(var file in files){\r\n\r\n\r\n").AsStatement(),
                               new MarkupBlock(
                                   Factory.MarkupTransition(),
                                   Factory.MetaMarkup(":", HtmlSymbolType.Colon),
                                   Factory.Markup("Baz\r\n")
                                       .With(new SingleLineMarkupEditHandler(CSharpLanguageCharacteristics.Instance.TokenizeString, AcceptedCharacters.None))
                                   ),
                               new MarkupBlock(
                                   Factory.Markup("<br/>\r\n")
                                       .Accepts(AcceptedCharacters.None)
                                   ),
                               new MarkupBlock(
                                   Factory.Markup("<a>Foo</a>\r\n")
                                       .Accepts(AcceptedCharacters.None)
                                   ),
                               new MarkupBlock(
                                   Factory.MarkupTransition(),
                                   Factory.MetaMarkup(":", HtmlSymbolType.Colon),
                                   Factory.Markup("Bar\r\n")
                                       .With(new SingleLineMarkupEditHandler(CSharpLanguageCharacteristics.Instance.TokenizeString, AcceptedCharacters.None))
                                   ),
                               Factory.Code("}").AsStatement().Accepts(AcceptedCharacters.None)
                               ));
        }

        [Fact]
        public void ParseBlockGivesAllWhitespaceOnSameLineExcludingPreceedingNewlineButIncludingTrailingNewLineToMarkup()
        {
            ParseBlockTest(@"if(foo) {
    var foo = ""After this statement there are 10 spaces"";          
    <p>
        Foo
        @bar
    </p>
    @:Hello!
    var biz = boz;
}",
                           new StatementBlock(
                               Factory.Code("if(foo) {\r\n    var foo = \"After this statement there are 10 spaces\";          \r\n").AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup("    <p>\r\n        Foo\r\n"),
                                   new ExpressionBlock(
                                       Factory.Code("        ").AsStatement(),
                                       Factory.CodeTransition(),
                                       Factory.Code(@"bar").AsImplicitExpression(CSharpCodeParser.DefaultKeywords).Accepts(AcceptedCharacters.NonWhiteSpace)
                                       ),
                                   Factory.Markup("\r\n    </p>\r\n").Accepts(AcceptedCharacters.None)
                                   ),
                               new MarkupBlock(
                                   Factory.Markup(@"    "),
                                   Factory.MarkupTransition(),
                                   Factory.MetaMarkup(":", HtmlSymbolType.Colon),
                                   Factory.Markup("Hello!\r\n").With(new SingleLineMarkupEditHandler(CSharpLanguageCharacteristics.Instance.TokenizeString, AcceptedCharacters.None))
                                   ),
                               Factory.Code("    var biz = boz;\r\n}").AsStatement()));
        }

        [Fact]
        public void ParseBlockAllowsMarkupInIfBodyWithBraces()
        {
            ParseBlockTest("if(foo) { <p>Bar</p> } else if(bar) { <p>Baz</p> } else { <p>Boz</p> }",
                           new StatementBlock(
                               Factory.Code("if(foo) {").AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup(" <p>Bar</p> ").Accepts(AcceptedCharacters.None)
                                   ),
                               Factory.Code("} else if(bar) {").AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup(" <p>Baz</p> ").Accepts(AcceptedCharacters.None)
                                   ),
                               Factory.Code("} else {").AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup(" <p>Boz</p> ").Accepts(AcceptedCharacters.None)
                                   ),
                               Factory.Code("}").AsStatement().Accepts(AcceptedCharacters.None)
                               ));
        }

        [Fact]
        public void ParseBlockAllowsMarkupInIfBodyWithBracesWithinCodeBlock()
        {
            ParseBlockTest("{ if(foo) { <p>Bar</p> } else if(bar) { <p>Baz</p> } else { <p>Boz</p> } }",
                           new StatementBlock(
                               Factory.MetaCode("{").Accepts(AcceptedCharacters.None),
                               Factory.Code(" if(foo) {").AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup(" <p>Bar</p> ").Accepts(AcceptedCharacters.None)
                                   ),
                               Factory.Code("} else if(bar) {").AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup(" <p>Baz</p> ").Accepts(AcceptedCharacters.None)
                                   ),
                               Factory.Code("} else {").AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup(" <p>Boz</p> ").Accepts(AcceptedCharacters.None)
                                   ),
                               Factory.Code("} ").AsStatement(),
                               Factory.MetaCode("}").Accepts(AcceptedCharacters.None)
                               ));
        }

        [Fact]
        public void ParseBlockSupportsMarkupInCaseAndDefaultBranchesOfSwitch()
        {
            // Arrange
            ParseBlockTest(@"switch(foo) {
    case 0:
        <p>Foo</p>
        break;
    case 1:
        <p>Bar</p>
        return;
    case 2:
        {
            <p>Baz</p>
            <p>Boz</p>
        }
    default:
        <p>Biz</p>
}",
                           new StatementBlock(
                               Factory.Code("switch(foo) {\r\n    case 0:\r\n").AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup("        <p>Foo</p>\r\n").Accepts(AcceptedCharacters.None)
                                   ),
                               Factory.Code("        break;\r\n    case 1:\r\n").AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup("        <p>Bar</p>\r\n").Accepts(AcceptedCharacters.None)
                                   ),
                               Factory.Code("        return;\r\n    case 2:\r\n        {\r\n").AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup("            <p>Baz</p>\r\n").Accepts(AcceptedCharacters.None)
                                   ),
                               new MarkupBlock(
                                   Factory.Markup("            <p>Boz</p>\r\n").Accepts(AcceptedCharacters.None)
                                   ),
                               Factory.Code("        }\r\n    default:\r\n").AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup("        <p>Biz</p>\r\n").Accepts(AcceptedCharacters.None)
                                   ),
                               Factory.Code("}").AsStatement().Accepts(AcceptedCharacters.None)));
        }

        [Fact]
        public void ParseBlockSupportsMarkupInCaseAndDefaultBranchesOfSwitchInCodeBlock()
        {
            // Arrange
            ParseBlockTest(@"{ switch(foo) {
    case 0:
        <p>Foo</p>
        break;
    case 1:
        <p>Bar</p>
        return;
    case 2:
        {
            <p>Baz</p>
            <p>Boz</p>
        }
    default:
        <p>Biz</p>
} }",
                           new StatementBlock(
                               Factory.MetaCode("{").Accepts(AcceptedCharacters.None),
                               Factory.Code(" switch(foo) {\r\n    case 0:\r\n").AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup("        <p>Foo</p>\r\n").Accepts(AcceptedCharacters.None)
                                   ),
                               Factory.Code("        break;\r\n    case 1:\r\n").AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup("        <p>Bar</p>\r\n").Accepts(AcceptedCharacters.None)
                                   ),
                               Factory.Code("        return;\r\n    case 2:\r\n        {\r\n").AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup("            <p>Baz</p>\r\n").Accepts(AcceptedCharacters.None)
                                   ),
                               new MarkupBlock(
                                   Factory.Markup("            <p>Boz</p>\r\n").Accepts(AcceptedCharacters.None)
                                   ),
                               Factory.Code("        }\r\n    default:\r\n").AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup("        <p>Biz</p>\r\n").Accepts(AcceptedCharacters.None)
                                   ),
                               Factory.Code("} ").AsStatement(),
                               Factory.MetaCode("}").Accepts(AcceptedCharacters.None)));
        }

        [Fact]
        public void ParseBlockParsesMarkupStatementOnOpenAngleBracket()
        {
            ParseBlockTest("for(int i = 0; i < 10; i++) { <p>Foo</p> }",
                           new StatementBlock(
                               Factory.Code("for(int i = 0; i < 10; i++) {").AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup(" <p>Foo</p> ").Accepts(AcceptedCharacters.None)
                                   ),
                               Factory.Code("}").AsStatement().Accepts(AcceptedCharacters.None)
                               ));
        }

        [Fact]
        public void ParseBlockParsesMarkupStatementOnOpenAngleBracketInCodeBlock()
        {
            ParseBlockTest("{ for(int i = 0; i < 10; i++) { <p>Foo</p> } }",
                           new StatementBlock(
                               Factory.MetaCode("{").Accepts(AcceptedCharacters.None),
                               Factory.Code(" for(int i = 0; i < 10; i++) {").AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup(" <p>Foo</p> ").Accepts(AcceptedCharacters.None)
                                   ),
                               Factory.Code("} ").AsStatement(),
                               Factory.MetaCode("}").Accepts(AcceptedCharacters.None)));
        }

        [Fact]
        public void ParseBlockParsesMarkupStatementOnSwitchCharacterFollowedByColon()
        {
            // Arrange
            ParseBlockTest(@"if(foo) { @:Bar
} zoop",
                           new StatementBlock(
                               Factory.Code("if(foo) {").AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup(" "),
                                   Factory.MarkupTransition(),
                                   Factory.MetaMarkup(":", HtmlSymbolType.Colon),
                                   Factory.Markup("Bar\r\n").With(new SingleLineMarkupEditHandler(CSharpLanguageCharacteristics.Instance.TokenizeString, AcceptedCharacters.None))
                                   ),
                               Factory.Code("}").AsStatement()));
        }

        [Fact]
        public void ParseBlockParsesMarkupStatementOnSwitchCharacterFollowedByColonInCodeBlock()
        {
            // Arrange
            ParseBlockTest(@"{ if(foo) { @:Bar
} } zoop",
                           new StatementBlock(
                               Factory.MetaCode("{").Accepts(AcceptedCharacters.None),
                               Factory.Code(" if(foo) {").AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup(" "),
                                   Factory.MarkupTransition(),
                                   Factory.MetaMarkup(":", HtmlSymbolType.Colon),
                                   Factory.Markup("Bar\r\n").Accepts(AcceptedCharacters.None)
                                   ),
                               Factory.Code("} ").AsStatement(),
                               Factory.MetaCode("}").Accepts(AcceptedCharacters.None)));
        }

        [Fact]
        public void ParseBlockCorrectlyReturnsFromMarkupBlockWithPseudoTag()
        {
            ParseBlockTest(@"if (i > 0) { <text>;</text> }",
                           new StatementBlock(
                               Factory.Code(@"if (i > 0) {").AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup(" "),
                                   Factory.MarkupTransition("<text>").Accepts(AcceptedCharacters.None),
                                   Factory.Markup(";"),
                                   Factory.MarkupTransition("</text>").Accepts(AcceptedCharacters.None),
                                   Factory.Markup(" ").Accepts(AcceptedCharacters.None)
                                   ),
                               Factory.Code(@"}").AsStatement()));
        }

        [Fact]
        public void ParseBlockCorrectlyReturnsFromMarkupBlockWithPseudoTagInCodeBlock()
        {
            ParseBlockTest(@"{ if (i > 0) { <text>;</text> } }",
                           new StatementBlock(
                               Factory.MetaCode("{").Accepts(AcceptedCharacters.None),
                               Factory.Code(@" if (i > 0) {").AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup(" "),
                                   Factory.MarkupTransition("<text>").Accepts(AcceptedCharacters.None),
                                   Factory.Markup(";"),
                                   Factory.MarkupTransition("</text>").Accepts(AcceptedCharacters.None),
                                   Factory.Markup(" ").Accepts(AcceptedCharacters.None)
                                   ),
                               Factory.Code(@"} ").AsStatement(),
                               Factory.MetaCode("}").Accepts(AcceptedCharacters.None)));
        }

        [Fact]
        public void ParseBlockSupportsAllKindsOfImplicitMarkupInCodeBlock()
        {
            ParseBlockTest(@"{
    if(true) {
        @:Single Line Markup
    }
    foreach (var p in Enumerable.Range(1, 10)) {
        <text>The number is @p</text>
    }
    if(!false) {
        <p>A real tag!</p>
    }
}",
                           new StatementBlock(
                               Factory.MetaCode("{").Accepts(AcceptedCharacters.None),
                               Factory.Code("\r\n    if(true) {\r\n").AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup("        "),
                                   Factory.MarkupTransition(),
                                   Factory.MetaMarkup(":", HtmlSymbolType.Colon),
                                   Factory.Markup("Single Line Markup\r\n").With(new SingleLineMarkupEditHandler(CSharpLanguageCharacteristics.Instance.TokenizeString, AcceptedCharacters.None))
                                   ),
                               Factory.Code("    }\r\n    foreach (var p in Enumerable.Range(1, 10)) {\r\n").AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup(@"        "),
                                   Factory.MarkupTransition("<text>").Accepts(AcceptedCharacters.None),
                                   Factory.Markup("The number is "),
                                   new ExpressionBlock(
                                       Factory.CodeTransition(),
                                       Factory.Code("p").AsImplicitExpression(CSharpCodeParser.DefaultKeywords).Accepts(AcceptedCharacters.NonWhiteSpace)
                                       ),
                                   Factory.MarkupTransition("</text>").Accepts(AcceptedCharacters.None),
                                   Factory.Markup("\r\n").Accepts(AcceptedCharacters.None)
                                   ),
                               Factory.Code("    }\r\n    if(!false) {\r\n").AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup("        <p>A real tag!</p>\r\n").Accepts(AcceptedCharacters.None)
                                   ),
                               Factory.Code("    }\r\n").AsStatement(),
                               Factory.MetaCode("}").Accepts(AcceptedCharacters.None)));
        }
    }
}
