// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting.DataSets;
using System.Text;
using Microsoft.TestCommon;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Net.Http.Formatting.Parsers
{
    public class HttpResponseHeaderParserTests
    {
        [Fact]
        [Trait("Description", "HttpResponseHeaderParser is internal class")]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties<HttpResponseHeaderParser>(TypeAssert.TypeProperties.IsClass);
        }

        private static byte[] CreateBuffer(string version, string statusCode, string reasonPhrase, Dictionary<string, string> headers)
        {
            const string SP = " ";
            const string CRLF = "\r\n";
            string lws = SP;

            StringBuilder response = new StringBuilder();
            response.AppendFormat("{0}{1}{2}{3}{4}{5}", version, lws, statusCode, lws, reasonPhrase, CRLF);
            if (headers != null)
            {
                foreach (var h in headers)
                {
                    response.AppendFormat("{0}: {1}{2}", h.Key, h.Value, CRLF);
                }
            }

            response.Append(CRLF);
            return Encoding.UTF8.GetBytes(response.ToString());
        }

        private static ParserState ParseBufferInSteps(HttpResponseHeaderParser parser, byte[] buffer, int readsize, out int totalBytesConsumed)
        {
            ParserState state = ParserState.Invalid;
            totalBytesConsumed = 0;
            while (totalBytesConsumed <= buffer.Length)
            {
                int size = Math.Min(buffer.Length - totalBytesConsumed, readsize);
                byte[] parseBuffer = new byte[size];
                Buffer.BlockCopy(buffer, totalBytesConsumed, parseBuffer, 0, size);

                int bytesConsumed = 0;
                state = parser.ParseBuffer(parseBuffer, parseBuffer.Length, ref bytesConsumed);
                totalBytesConsumed += bytesConsumed;

                if (state != ParserState.NeedMoreData)
                {
                    return state;
                }
            }

            return state;
        }

        private static void ValidateResult(
            HttpUnsortedResponse statusLine,
            Version version,
            HttpStatusCode statusCode,
            string reasonPhrase,
            Dictionary<string, string> headers)
        {
            Assert.Equal(version, statusLine.Version);
            Assert.Equal(statusCode, statusLine.StatusCode);
            Assert.Equal(reasonPhrase, statusLine.ReasonPhrase);

            if (headers != null)
            {
                Assert.Equal(headers.Count, statusLine.HttpHeaders.Count());
                foreach (var header in headers)
                {
                    Assert.True(statusLine.HttpHeaders.Contains(header.Key), "Parsed header did not contain expected key " + header.Key);
                    Assert.Equal(header.Value, statusLine.HttpHeaders.GetValues(header.Key).ElementAt(0));
                }
            }
        }

        [Fact]
        [Trait("Description", "HttpResponseHeaderParser constructor throws on invalid arguments")]
        public void HttpResponseHeaderParserConstructorTest()
        {
            HttpUnsortedResponse result = new HttpUnsortedResponse();
            Assert.NotNull(result);

            Assert.ThrowsArgumentGreaterThanOrEqualTo(() => new HttpResponseHeaderParser(result, ParserData.MinStatusLineSize - 1, ParserData.MinHeaderSize),
                "maxStatusLineSize", ParserData.MinStatusLineSize.ToString(), ParserData.MinStatusLineSize - 1);

            Assert.ThrowsArgumentGreaterThanOrEqualTo(() => new HttpResponseHeaderParser(result, ParserData.MinStatusLineSize, ParserData.MinHeaderSize - 1),
                "maxHeaderSize", ParserData.MinHeaderSize.ToString(), ParserData.MinHeaderSize - 1);

            HttpResponseHeaderParser parser = new HttpResponseHeaderParser(result, ParserData.MinStatusLineSize, ParserData.MinHeaderSize);
            Assert.NotNull(parser);

            Assert.ThrowsArgumentNull(() => { new HttpResponseHeaderParser(null); }, "httpResponse");
        }


        [Fact]
        [Trait("Description", "HttpResponseHeaderParser.ParseBuffer throws on null buffer.")]
        public void ResponseHeaderParserNullBuffer()
        {
            HttpUnsortedResponse result = new HttpUnsortedResponse();
            HttpResponseHeaderParser parser = new HttpResponseHeaderParser(result, ParserData.MinStatusLineSize, ParserData.MinHeaderSize);
            Assert.NotNull(parser);
            int bytesConsumed = 0;
            Assert.ThrowsArgumentNull(() => { parser.ParseBuffer(null, 0, ref bytesConsumed); }, "buffer");
        }

        [Fact]
        [Trait("Description", "HttpResponseHeaderParser.ParseBuffer parses minimum statusLine.")]
        public void ResponseHeaderParserMinimumBuffer()
        {
            byte[] data = CreateBuffer("HTTP/1.1", "200", "", null);
            HttpUnsortedResponse result = new HttpUnsortedResponse();
            HttpResponseHeaderParser parser = new HttpResponseHeaderParser(result, ParserData.MinStatusLineSize, ParserData.MinHeaderSize);
            Assert.NotNull(parser);

            int bytesConsumed = 0;
            ParserState state = parser.ParseBuffer(data, data.Length, ref bytesConsumed);
            Assert.Equal(ParserState.Done, state);
            Assert.Equal(data.Length, bytesConsumed);

            ValidateResult(result, new Version("1.1"), HttpStatusCode.OK, "", null);
        }

        [Fact]
        [Trait("Description", "HttpResponseHeaderParser.ParseBuffer parses standard status codes.")]
        public void ResponseHeaderParserAcceptsStandardStatusCodes()
        {
            foreach (HttpStatusCode status in HttpUnitTestDataSets.AllHttpStatusCodes)
            {
                byte[] data = CreateBuffer("HTTP/1.1", ((int)status).ToString(), "Reason", ParserData.ValidHeaders);

                for (var cnt = 1; cnt <= data.Length; cnt++)
                {
                    HttpUnsortedResponse result = new HttpUnsortedResponse();
                    HttpResponseHeaderParser parser = new HttpResponseHeaderParser(result);
                    Assert.NotNull(parser);

                    int totalBytesConsumed = 0;
                    ParserState state = ParseBufferInSteps(parser, data, cnt, out totalBytesConsumed);
                    Assert.Equal(ParserState.Done, state);
                    Assert.Equal(data.Length, totalBytesConsumed);

                    ValidateResult(result, new Version("1.1"), status, "Reason", ParserData.ValidHeaders);
                }
            }
        }

        [Fact]
        [Trait("Description", "HttpResponseHeaderParser.ParseBuffer parses custom status codes.")]
        public void ResponseHeaderParserAcceptsCustomStatusCodes()
        {
            foreach (HttpStatusCode status in HttpUnitTestDataSets.CustomHttpStatusCodes)
            {
                byte[] data = CreateBuffer("HTTP/1.1", ((int)status).ToString(), "Reason", ParserData.ValidHeaders);

                for (var cnt = 1; cnt <= data.Length; cnt++)
                {
                    HttpUnsortedResponse result = new HttpUnsortedResponse();
                    HttpResponseHeaderParser parser = new HttpResponseHeaderParser(result);
                    Assert.NotNull(parser);

                    int totalBytesConsumed = 0;
                    ParserState state = ParseBufferInSteps(parser, data, cnt, out totalBytesConsumed);
                    Assert.Equal(ParserState.Done, state);
                    Assert.Equal(data.Length, totalBytesConsumed);

                    ValidateResult(result, new Version("1.1"), status, "Reason", ParserData.ValidHeaders);
                }
            }
        }

        [Fact]
        [Trait("Description", "HttpResponseHeaderParser.ParseBuffer rejects invalid status codes")]
        public void ResponseHeaderParserRejectsInvalidStatusCodes()
        {
            foreach (string invalidStatus in ParserData.InvalidStatusCodes)
            {
                byte[] data = CreateBuffer("HTTP/1.1", invalidStatus, "Reason", ParserData.ValidHeaders);

                for (var cnt = 1; cnt <= data.Length; cnt++)
                {
                    HttpUnsortedResponse result = new HttpUnsortedResponse();
                    HttpResponseHeaderParser parser = new HttpResponseHeaderParser(result);
                    Assert.NotNull(parser);

                    int totalBytesConsumed = 0;
                    ParserState state = ParseBufferInSteps(parser, data, cnt, out totalBytesConsumed);
                    Assert.Equal(ParserState.Invalid, state);
                }
            }
        }

        [Fact]
        [Trait("Description", "HttpResponseHeaderParser.ParseBuffer rejects invalid reason phrase.")]
        public void ResponseHeaderParserRejectsInvalidReasonPhrase()
        {
            foreach (string invalidReason in ParserData.InvalidReasonPhrases)
            {
                byte[] data = CreateBuffer("HTTP/1.1", "200", invalidReason, ParserData.ValidHeaders);

                for (var cnt = 1; cnt <= data.Length; cnt++)
                {
                    HttpUnsortedResponse result = new HttpUnsortedResponse();
                    HttpResponseHeaderParser parser = new HttpResponseHeaderParser(result);
                    Assert.NotNull(parser);

                    int totalBytesConsumed = 0;
                    ParserState state = ParseBufferInSteps(parser, data, cnt, out totalBytesConsumed);
                    Assert.Equal(ParserState.Invalid, state);
                }
            }
        }

        public static IEnumerable<object[]> Versions
        {
            get { return ParserData.Versions; }
        }

        [Theory]
        [PropertyData("Versions")]
        [Trait("Description", "HttpResponseHeaderParser.ParseBuffer accepts valid versions.")]
        public void ResponseHeaderParserAcceptsValidVersion(Version version)
        {
            byte[] data = CreateBuffer(String.Format("HTTP/{0}", version.ToString(2)), "200", "Reason", ParserData.ValidHeaders);

            for (var cnt = 1; cnt <= data.Length; cnt++)
            {
                HttpUnsortedResponse result = new HttpUnsortedResponse();
                HttpResponseHeaderParser parser = new HttpResponseHeaderParser(result);
                Assert.NotNull(parser);

                int totalBytesConsumed = 0;
                ParserState state = ParseBufferInSteps(parser, data, cnt, out totalBytesConsumed);
                Assert.Equal(data.Length, totalBytesConsumed);

                ValidateResult(result, version, HttpStatusCode.OK, "Reason", ParserData.ValidHeaders);
            }
        }

        public static IEnumerable<object[]> InvalidVersions
        {
            get { return ParserData.InvalidVersions; }
        }

        [Theory]
        [PropertyData("InvalidVersions")]
        [Trait("Description", "HttpResponseHeaderParser.ParseBuffer rejects invalid protocol version.")]
        public void ResponseHeaderParserRejectsInvalidVersion(string invalid)
        {
            byte[] data = CreateBuffer(invalid, "200", "Reason", ParserData.ValidHeaders);

            for (var cnt = 1; cnt <= data.Length; cnt++)
            {
                HttpUnsortedResponse result = new HttpUnsortedResponse();
                HttpResponseHeaderParser parser = new HttpResponseHeaderParser(result);
                Assert.NotNull(parser);

                int totalBytesConsumed = 0;
                ParserState state = ParseBufferInSteps(parser, data, cnt, out totalBytesConsumed);
                Assert.Equal(ParserState.Invalid, state);
            }
        }
    }
}