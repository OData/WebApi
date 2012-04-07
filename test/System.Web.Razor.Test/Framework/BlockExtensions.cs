// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Test.Framework
{
    public static class BlockExtensions
    {
        public static void LinkNodes(this Block self)
        {
            Span first = null;
            Span previous = null;
            foreach (Span span in self.Flatten())
            {
                if (first == null)
                {
                    first = span;
                }
                span.Previous = previous;

                if (previous != null)
                {
                    previous.Next = span;
                }
                previous = span;
            }
        }
    }
}
