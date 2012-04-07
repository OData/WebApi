// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Parser
{
    internal interface ISyntaxTreeRewriter
    {
        Block Rewrite(Block input);
    }
}
