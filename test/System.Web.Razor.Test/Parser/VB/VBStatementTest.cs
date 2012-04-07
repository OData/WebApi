// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Razor.Generator;
using System.Web.Razor.Parser;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.Razor.Test.Framework;
using Xunit;

namespace System.Web.Razor.Test.Parser.VB
{
    public class VBStatementTest : VBHtmlCodeParserTestBase
    {
        [Fact]
        public void VB_Inherits_Statement()
        {
            ParseBlockTest(@"@Inherits System.Foo.Bar(Of Baz)",
                new DirectiveBlock(
                    Factory.CodeTransition(),
                    Factory.MetaCode("Inherits ").Accepts(AcceptedCharacters.None),
                    Factory.Code("System.Foo.Bar(Of Baz)")
                           .AsBaseType("System.Foo.Bar(Of Baz)")));
        }

        [Fact]
        public void InheritsDirectiveSupportsArrays()
        {
            ParseBlockTest("@Inherits System.String(())()",
                new DirectiveBlock(
                    Factory.CodeTransition(),
                    Factory.MetaCode("Inherits ")
                           .Accepts(AcceptedCharacters.None),
                    Factory.Code("System.String(())()")
                           .AsBaseType("System.String(())()")));
        }

        [Fact]
        public void InheritsDirectiveSupportsNestedGenerics()
        {
            ParseBlockTest("@Inherits System.Web.Mvc.WebViewPage(Of IEnumerable(Of MvcApplication2.Models.RegisterModel))",
                new DirectiveBlock(
                    Factory.CodeTransition(),
                    Factory.MetaCode("Inherits ")
                           .Accepts(AcceptedCharacters.None),
                    Factory.Code("System.Web.Mvc.WebViewPage(Of IEnumerable(Of MvcApplication2.Models.RegisterModel))")
                           .AsBaseType("System.Web.Mvc.WebViewPage(Of IEnumerable(Of MvcApplication2.Models.RegisterModel))")));
        }

        [Fact]
        public void InheritsDirectiveSupportsTypeKeywords()
        {
            ParseBlockTest("@Inherits String",
                new DirectiveBlock(
                    Factory.CodeTransition(),
                    Factory.MetaCode("Inherits ").Accepts(AcceptedCharacters.None),
                    Factory.Code("String").AsBaseType("String")));
        }

        [Fact]
        public void VB_Option_Strict_Statement()
        {
            ParseBlockTest(@"@Option Strict Off",
                new DirectiveBlock(
                    Factory.CodeTransition(),
                    Factory.MetaCode("Option Strict Off")
                           .With(SetVBOptionCodeGenerator.Strict(false))));
        }

        [Fact]
        public void VB_Option_Explicit_Statement()
        {
            ParseBlockTest(@"@Option Explicit Off",
                new DirectiveBlock(
                    Factory.CodeTransition(),
                    Factory.MetaCode("Option Explicit Off")
                           .With(SetVBOptionCodeGenerator.Explicit(false))));
        }

        [Fact]
        public void VB_Imports_Statement()
        {
            ParseBlockTest("@Imports Biz = System.Foo.Bar(Of Boz.Baz(Of Qux))",
                new DirectiveBlock(
                    Factory.CodeTransition(),
                    Factory.MetaCode("Imports Biz = System.Foo.Bar(Of Boz.Baz(Of Qux))")
                           .With(new AddImportCodeGenerator(
                               ns: " Biz = System.Foo.Bar(Of Boz.Baz(Of Qux))",
                               namespaceKeywordLength: SyntaxConstants.VB.ImportsKeywordLength))));
        }

        [Fact]
        public void VB_Using_Statement()
        {
            ParseBlockTest(@"@Using foo as Bar
    foo()
End Using",
                new StatementBlock(
                    Factory.CodeTransition(),
                    Factory.Code("Using foo as Bar\r\n    foo()\r\nEnd Using")
                           .AsStatement()
                           .Accepts(AcceptedCharacters.None)));
        }

        [Fact]
        public void VB_Do_Loop_Statement()
        {
            ParseBlockTest(@"@Do
    foo()
Loop While True",
                new StatementBlock(
                    Factory.CodeTransition(),
                    Factory.Code("Do\r\n    foo()\r\nLoop While True")
                           .AsStatement()
                           .Accepts(AcceptedCharacters.AnyExceptNewline)));
        }

        [Fact]
        public void VB_While_Statement()
        {
            ParseBlockTest(@"@While True
    foo()
End While",
                new StatementBlock(
                    Factory.CodeTransition(),
                    Factory.Code("While True\r\n    foo()\r\nEnd While")
                           .AsStatement()
                           .Accepts(AcceptedCharacters.None)));
        }

        [Fact]
        public void VB_If_Statement()
        {
            ParseBlockTest(@"@If True Then
    foo()
ElseIf False Then
    bar()
Else
    baz()
End If",
                new StatementBlock(
                    Factory.CodeTransition(),
                    Factory.Code("If True Then\r\n    foo()\r\nElseIf False Then\r\n    bar()\r\nElse\r\n    baz()\r\nEnd If")
                           .AsStatement()
                           .Accepts(AcceptedCharacters.None)));
        }

        [Fact]
        public void VB_Select_Statement()
        {
            ParseBlockTest(@"@Select Case foo
    Case 1
        foo()
    Case 2
        bar()
    Case Else
        baz()
End Select",
                new StatementBlock(
                    Factory.CodeTransition(),
                    Factory.Code("Select Case foo\r\n    Case 1\r\n        foo()\r\n    Case 2\r\n        bar()\r\n    Case Else\r\n        baz()\r\nEnd Select")
                           .AsStatement()
                           .Accepts(AcceptedCharacters.None)));
        }

        [Fact]
        public void VB_For_Statement()
        {
            ParseBlockTest(@"@For Each foo In bar
    baz()
Next",
                new StatementBlock(
                    Factory.CodeTransition(),
                    Factory.Code("For Each foo In bar\r\n    baz()\r\nNext")
                           .AsStatement()
                           .Accepts(AcceptedCharacters.AnyExceptNewline)));
        }

        [Fact]
        public void VB_Try_Statement()
        {
            ParseBlockTest(@"@Try
    foo()
Catch ex as Exception
    bar()
Finally
    baz()
End Try",
                new StatementBlock(
                    Factory.CodeTransition(),
                    Factory.Code("Try\r\n    foo()\r\nCatch ex as Exception\r\n    bar()\r\nFinally\r\n    baz()\r\nEnd Try")
                           .AsStatement()
                           .Accepts(AcceptedCharacters.None)));
        }

        [Fact]
        public void VB_With_Statement()
        {
            ParseBlockTest(@"@With foo
    .bar()
End With",
                new StatementBlock(
                    Factory.CodeTransition(),
                    Factory.Code("With foo\r\n    .bar()\r\nEnd With")
                           .AsStatement()
                           .Accepts(AcceptedCharacters.None)));
        }

        [Fact]
        public void VB_SyncLock_Statement()
        {
            ParseBlockTest(@"@SyncLock foo
    foo.bar()
End SyncLock",
                new StatementBlock(
                    Factory.CodeTransition(),
                    Factory.Code("SyncLock foo\r\n    foo.bar()\r\nEnd SyncLock")
                           .AsStatement()
                           .Accepts(AcceptedCharacters.None)));
        }
    }
}
