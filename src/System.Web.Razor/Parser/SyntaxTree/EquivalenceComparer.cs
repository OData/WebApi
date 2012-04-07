// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.Razor.Parser.SyntaxTree
{
    internal class EquivalenceComparer : IEqualityComparer<SyntaxTreeNode>
    {
        public bool Equals(SyntaxTreeNode x, SyntaxTreeNode y)
        {
            return x.EquivalentTo(y);
        }

        public int GetHashCode(SyntaxTreeNode obj)
        {
            return obj.GetHashCode();
        }
    }
}
