// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Editor
{
    public class EditResult
    {
        public EditResult(PartialParseResult result, SpanBuilder editedSpan)
        {
            Result = result;
            EditedSpan = editedSpan;
        }

        public PartialParseResult Result { get; set; }
        public SpanBuilder EditedSpan { get; set; }
    }
}
