// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Razor.Editor;
using System.Web.Razor.Generator;
using System.Web.Razor.Parser;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.Razor.Test.Framework;
using Xunit;
using Xunit.Extensions;

namespace System.Web.Razor.Test.Parser.CSharp
{
    public class CSharpLayoutDirectiveTest : CsHtmlCodeParserTestBase
    {
        [Theory]
        [InlineData("Layout")]
        [InlineData("LAYOUT")]
        [InlineData("layOut")]
        [InlineData("LayOut")]
        private void LayoutKeywordIsCaseSensitive(string word)
        {
            ParseBlockTest(word,
                new ExpressionBlock(
                    Factory.Code(word)
                        .AsImplicitExpression(CSharpCodeParser.DefaultKeywords)
                        .Accepts(AcceptedCharacters.NonWhiteSpace)
                    ));
        }

        [Fact]
        public void LayoutDirectiveAcceptsAllTextToEndOfLine()
        {
            ParseBlockTest(@"@layout Foo Bar Baz",
                new DirectiveBlock(
                    Factory.CodeTransition(),
                    Factory.MetaCode("layout ").Accepts(AcceptedCharacters.None),
                    Factory.MetaCode("Foo Bar Baz")
                           .With(new SetLayoutCodeGenerator("Foo Bar Baz"))
                           .WithEditorHints(EditorHints.VirtualPath | EditorHints.LayoutPage)
                )
            );
        }

        [Fact]
        public void LayoutDirectiveAcceptsAnyIfNoWhitespaceFollowingLayoutKeyword()
        {
            ParseBlockTest(@"@layout",
                new DirectiveBlock(
                    Factory.CodeTransition(),
                    Factory.MetaCode("layout")
                )
            );
        }

        [Fact]
        public void LayoutDirectiveOutputsMarkerSpanIfAnyWhitespaceAfterLayoutKeyword()
        {
            ParseBlockTest(@"@layout ",
                new DirectiveBlock(
                    Factory.CodeTransition(),
                    Factory.MetaCode("layout ").Accepts(AcceptedCharacters.None),
                    Factory.EmptyCSharp()
                           .AsMetaCode()
                           .With(new SetLayoutCodeGenerator(String.Empty))
                           .WithEditorHints(EditorHints.VirtualPath | EditorHints.LayoutPage)
                )
            );
        }

        [Fact]
        public void LayoutDirectiveAcceptsTrailingNewlineButDoesNotIncludeItInLayoutPath()
        {
            ParseBlockTest(@"@layout Foo
",
                new DirectiveBlock(
                    Factory.CodeTransition(),
                    Factory.MetaCode("layout ").Accepts(AcceptedCharacters.None),
                    Factory.MetaCode("Foo\r\n")
                           .With(new SetLayoutCodeGenerator("Foo"))
                           .Accepts(AcceptedCharacters.None)
                           .WithEditorHints(EditorHints.VirtualPath | EditorHints.LayoutPage)
                )
            );
        }
    }
}
