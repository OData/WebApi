// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Generator
{
    public abstract class CodeGeneratorBase
    {
        // Helpers
        protected internal static int CalculatePadding(Span target)
        {
            return CalculatePadding(target, generatedStart: 0);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "This method should only be used on Spans")]
        protected internal static int CalculatePadding(Span target, int generatedStart)
        {
            int padding = target.Start.CharacterIndex - generatedStart;
            if (padding < 0)
            {
                padding = 0;
            }
            return padding;
        }

        protected internal static string Pad(string code, Span target)
        {
            return Pad(code, target, 0);
        }

        protected internal static string Pad(string code, Span target, int generatedStart)
        {
            return code.PadLeft(CalculatePadding(target, generatedStart) + code.Length, ' ');
        }
    }
}
