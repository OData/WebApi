// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Razor.Parser;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.Razor.Test.Framework;
using Xunit;

namespace System.Web.Razor.Test.Parser.VB
{
    // VB Exit Statement: http://msdn.microsoft.com/en-us/library/t2at9t47.aspx
    public class VBExitStatementTest : VBHtmlCodeParserTestBase
    {
        [Fact]
        public void VB_Do_Statement_With_Exit()
        {
            ParseBlockTest(@"@Do While True
    Exit Do
Loop
' Not in the block!",
                new StatementBlock(
                    Factory.CodeTransition(SyntaxConstants.TransitionString)
                           .Accepts(AcceptedCharacters.None),
                    Factory.Code("Do While True\r\n    Exit Do\r\nLoop\r\n")
                           .AsStatement()
                           .Accepts(AcceptedCharacters.AnyExceptNewline)));
        }

        [Fact]
        public void VB_For_Statement_With_Exit()
        {
            ParseBlockTest(@"@For i = 1 To 12
    Exit For
Next i
' Not in the block!",
                new StatementBlock(
                    Factory.CodeTransition(SyntaxConstants.TransitionString)
                           .Accepts(AcceptedCharacters.None),
                    Factory.Code("For i = 1 To 12\r\n    Exit For\r\nNext i\r\n")
                           .AsStatement()
                           .Accepts(AcceptedCharacters.AnyExceptNewline)));
        }

        [Fact]
        public void VB_Select_Statement_With_Exit()
        {
            ParseBlockTest(@"@Select Case Foo
    Case 1
        Exit Select
    Case 2
        Exit Select
End Select
' Not in the block!",
                new StatementBlock(
                    Factory.CodeTransition(SyntaxConstants.TransitionString)
                           .Accepts(AcceptedCharacters.None),
                    Factory.Code("Select Case Foo\r\n    Case 1\r\n        Exit Select\r\n    Case 2\r\n        Exit Select\r\nEnd Select\r\n")
                           .AsStatement()
                           .Accepts(AcceptedCharacters.None)));
        }

        [Fact]
        public void VB_Try_Statement_With_Exit()
        {
            ParseBlockTest(@"@Try
    Foo()
    Exit Try
Catch Bar
    Throw Bar
Finally
    Baz()
End Try
' Not in the block!",
                new StatementBlock(
                    Factory.CodeTransition(SyntaxConstants.TransitionString)
                           .Accepts(AcceptedCharacters.None),
                    Factory.Code("Try\r\n    Foo()\r\n    Exit Try\r\nCatch Bar\r\n    Throw Bar\r\nFinally\r\n    Baz()\r\nEnd Try\r\n")
                           .AsStatement()
                           .Accepts(AcceptedCharacters.None)));
        }

        [Fact]
        public void VB_While_Statement_With_Exit()
        {
            ParseBlockTest(@"@While True
    Exit While
End While
' Not in the block!",
                new StatementBlock(
                    Factory.CodeTransition(SyntaxConstants.TransitionString)
                           .Accepts(AcceptedCharacters.None),
                    Factory.Code("While True\r\n    Exit While\r\nEnd While\r\n")
                           .AsStatement()
                           .Accepts(AcceptedCharacters.None)));
        }
    }
}
