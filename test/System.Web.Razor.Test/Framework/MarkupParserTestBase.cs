// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Razor.Parser;
using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor.Test.Framework
{
    public abstract class MarkupParserTestBase : CodeParserTestBase
    {
        protected override ParserBase SelectActiveParser(ParserBase codeParser, ParserBase markupParser)
        {
            return markupParser;
        }

        protected virtual void SingleSpanDocumentTest(string document, BlockType blockType, SpanKind spanType)
        {
            Block b = CreateSimpleBlockAndSpan(document, blockType, spanType);
            ParseDocumentTest(document, b);
        }

        protected virtual void ParseDocumentTest(string document)
        {
            ParseDocumentTest(document, null, false);
        }

        protected virtual void ParseDocumentTest(string document, Block expectedRoot)
        {
            ParseDocumentTest(document, expectedRoot, false, null);
        }

        protected virtual void ParseDocumentTest(string document, Block expectedRoot, params RazorError[] expectedErrors)
        {
            ParseDocumentTest(document, expectedRoot, false, expectedErrors);
        }

        protected virtual void ParseDocumentTest(string document, bool designTimeParser)
        {
            ParseDocumentTest(document, null, designTimeParser);
        }

        protected virtual void ParseDocumentTest(string document, Block expectedRoot, bool designTimeParser)
        {
            ParseDocumentTest(document, expectedRoot, designTimeParser, null);
        }

        protected virtual void ParseDocumentTest(string document, Block expectedRoot, bool designTimeParser, params RazorError[] expectedErrors)
        {
            RunParseTest(document, parser => parser.ParseDocument, expectedRoot, expectedErrors, designTimeParser);
        }

        protected virtual ParserResults ParseDocument(string document)
        {
            return ParseDocument(document, designTimeParser: false);
        }

        protected virtual ParserResults ParseDocument(string document, bool designTimeParser)
        {
            return RunParse(document, parser => parser.ParseDocument, designTimeParser);
        }

        protected virtual ParserResults ParseBlock(string document)
        {
            return ParseBlock(document, designTimeParser: false);
        }

        protected virtual ParserResults ParseBlock(string document, bool designTimeParser)
        {
            return RunParse(document, parser => parser.ParseBlock, designTimeParser);
        }
    }
}
