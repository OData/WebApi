// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Razor.Parser;
using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Generator
{
    public class StatementCodeGenerator : SpanCodeGenerator
    {
        public override void GenerateCode(Span target, CodeGeneratorContext context)
        {
            context.FlushBufferedStatement();

            string generatedCode = context.BuildCodeString(cw =>
            {
                cw.WriteSnippet(target.Content);
            });

            int startGeneratedCode = target.Start.CharacterIndex;
            generatedCode = Pad(generatedCode, target);

            // Is this the span immediately following "@"?
            if (context.Host.DesignTimeMode &&
                !String.IsNullOrEmpty(generatedCode) &&
                Char.IsWhiteSpace(generatedCode[0]) &&
                target.Previous != null &&
                target.Previous.Kind == SpanKind.Transition &&
                String.Equals(target.Previous.Content, SyntaxConstants.TransitionString))
            {
                generatedCode = generatedCode.Substring(1);
                startGeneratedCode--;
            }

            context.AddStatement(
                generatedCode,
                context.GenerateLinePragma(target, startGeneratedCode));
        }

        public override string ToString()
        {
            return "Stmt";
        }

        public override bool Equals(object obj)
        {
            return obj is StatementCodeGenerator;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
