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
    public class HttpRequestHeaderParserTests
    {
        [Fact]
        [Trait("Description", "HttpRequestHeaderParser is internal class")]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties<HttpRequestHeaderParser>(TypeAssert.TypeProperties.IsClass);
        }


        private static byte[] CreateBuffer(string method, string address, string version, Dictionary<string, string> headers)
        {
            const string SP = " ";
            const string CRLF = "\r\n";
            string lws = SP;

            StringBuilder request = new StringBuilder();
            request.AppendFormat("{0}{1}{2}{3}{4}{5}", method, lws, address, lws, version, CRLF);
            if (headers != null)
            {
                foreach (var h in headers)
                {
                    request.AppendFormat("{0}: {1}{2}", h.Key, h.Value, CRLF);
                }
            }

            request.Append(CRLF);
            return Encoding.UTF8.GetBytes(request.ToString());
        }

        private static ParserState ParseBufferInSteps(HttpRequestHeaderParser parser, byte[] buffer, int readsize, out int totalBytesConsumed)
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
            HttpUnsortedRequest requestLine,
            string method,
            string requestUri,
            Version version,
            Dictionary<string, string> headers)
        {
            Assert.Equal(new HttpMethod(method), requestLine.Method);
            Assert.Equal(requestUri, requestLine.RequestUri);
            Assert.Equal(version, requestLine.Version);

            if (headers != null)
            {
                Assert.Equal(headers.Count, requestLine.HttpHeaders.Count());
                foreach (var header in headers)
                {
                    Assert.True(requestLine.HttpHeaders.Contains(header.Key), "Parsed header did not contain expected key " + header.Key);
                    Assert.Equal(header.Value, requestLine.HttpHeaders.GetValues(header.Key).ElementAt(0));
                }
            }
        }

        [Fact]
        [Trait("Description", "HttpRequestHeaderParser constructor throws on invalid arguments")]
        public void HttpRequestHeaderParserConstructorTest()
        {
            HttpUnsortedRequest result = new HttpUnsortedRequest();
            Assert.NotNull(result);

            Assert.ThrowsArgumentGreaterThanOrEqualTo(() => new HttpRequestHeaderParser(result, ParserData.MinRequestLineSize - 1, ParserData.MinHeaderSize),
                "maxRequestLineSize", ParserData.MinRequestLineSize.ToString(), ParserData.MinRequestLineSize - 1);

            Assert.ThrowsArgumentGreaterThanOrEqualTo(() => new HttpRequestHeaderParser(result, ParserData.MinRequestLineSize, ParserData.MinHeaderSize - 1),
                "maxHeaderSize", ParserData.MinHeaderSize.ToString(), ParserData.MinHeaderSize - 1);

            HttpRequestHeaderParser parser = new HttpRequestHeaderParser(result, ParserData.MinRequestLineSize, ParserData.MinHeaderSize);
            Assert.NotNull(parser);

            Assert.ThrowsArgumentNull(() => { new HttpRequestHeaderParser(null); }, "httpRequest");
        }


        [Fact]
        [Trait("Description", "HttpRequestHeaderParser.ParseBuffer throws on null buffer.")]
        public void RequestHeaderParserNullBuffer()
        {
            HttpUnsortedRequest result = new HttpUnsortedRequest();
            HttpRequestHeaderParser parser = new HttpRequestHeaderParser(result, ParserData.MinRequestLineSize, ParserData.MinHeaderSize);
            Assert.NotNull(parser);
            int bytesConsumed = 0;
            Assert.ThrowsArgumentNull(() => { parser.ParseBuffer(null, 0, ref bytesConsumed); }, "buffer");
        }

        [Fact]
        [Trait("Description", "HttpRequestHeaderParser.ParseBuffer parses minimum requestline.")]
        public void RequestHeaderParserMinimumBuffer()
        {
            byte[] data = CreateBuffer("G", "/", "HTTP/1.1", null);
            HttpUnsortedRequest result = new HttpUnsortedRequest();
            HttpRequestHeaderParser parser = new HttpRequestHeaderParser(result, ParserData.MinRequestLineSize, ParserData.MinHeaderSize);
            Assert.NotNull(parser);

            int bytesConsumed = 0;
            ParserState state = parser.ParseBuffer(data, data.Length, ref bytesConsumed);
            Assert.Equal(ParserState.Done, state);
            Assert.Equal(data.Length, bytesConsumed);

            ValidateResult(result, "G", "/", new Version("1.1"), null);
        }

        [Fact]
        [Trait("Description", "HttpRequestHeaderParser.ParseBuffer parses standard methods.")]
        public void RequestHeaderParserAcceptsStandardMethods()
        {
            foreach (HttpMethod method in HttpUnitTestDataSets.AllHttpMethods)
            {
                byte[] data = CreateBuffer(method.ToString(), "/", "HTTP/1.1", ParserData.ValidHeaders);

                for (var cnt = 1; cnt <= data.Length; cnt++)
                {
                    HttpUnsortedRequest result = new HttpUnsortedRequest();
                    HttpRequestHeaderParser parser = new HttpRequestHeaderParser(result);
                    Assert.NotNull(parser);

                    int totalBytesConsumed = 0;
                    ParserState state = ParseBufferInSteps(parser, data, cnt, out totalBytesConsumed);
                    Assert.Equal(ParserState.Done, state);
                    Assert.Equal(data.Length, totalBytesConsumed);

                    ValidateResult(result, method.ToString(), "/", new Version("1.1"), ParserData.ValidHeaders);
                }
            }
        }

        [Fact]
        [Trait("Description", "HttpRequestHeaderParser.ParseBuffer parses custom methods.")]
        public void RequestHeaderParserAcceptsCustomMethods()
        {
            foreach (HttpMethod method in HttpUnitTestDataSets.CustomHttpMethods)
            {
                byte[] data = CreateBuffer(method.ToString(), "/", "HTTP/1.1", ParserData.ValidHeaders);

                for (var cnt = 1; cnt <= data.Length; cnt++)
                {
                    HttpUnsortedRequest result = new HttpUnsortedRequest();
                    HttpRequestHeaderParser parser = new HttpRequestHeaderParser(result);
                    Assert.NotNull(parser);

                    int totalBytesConsumed = 0;
                    ParserState state = ParseBufferInSteps(parser, data, cnt, out totalBytesConsumed);
                    Assert.Equal(ParserState.Done, state);
                    Assert.Equal(data.Length, totalBytesConsumed);

                    ValidateResult(result, method.ToString(), "/", new Version("1.1"), ParserData.ValidHeaders);
                }
            }
        }

        [Fact]
        [Trait("Description", "HttpRequestHeaderParser.ParseBuffer rejects invalid method")]
        public void RequestHeaderParserRejectsInvalidMethod()
        {
            foreach (string invalidMethod in ParserData.InvalidMethods)
            {
                byte[] data = CreateBuffer(invalidMethod, "/", "HTTP/1.1", ParserData.ValidHeaders);

                for (var cnt = 1; cnt <= data.Length; cnt++)
                {
                    HttpUnsortedRequest result = new HttpUnsortedRequest();
                    HttpRequestHeaderParser parser = new HttpRequestHeaderParser(result);
                    Assert.NotNull(parser);

                    int totalBytesConsumed = 0;
                    ParserState state = ParseBufferInSteps(parser, data, cnt, out totalBytesConsumed);
                    Assert.Equal(ParserState.Invalid, state);
                }
            }
        }

        [Fact]
        [Trait("Description", "HttpRequestHeaderParser.ParseBuffer rejects invalid URI.")]
        public void RequestHeaderParserRejectsInvalidUri()
        {
            foreach (string invalidRequestUri in ParserData.InvalidRequestUris)
            {
                byte[] data = CreateBuffer("GET", invalidRequestUri, "HTTP/1.1", ParserData.ValidHeaders);

                for (var cnt = 1; cnt <= data.Length; cnt++)
                {
                    HttpUnsortedRequest result = new HttpUnsortedRequest();
                    HttpRequestHeaderParser parser = new HttpRequestHeaderParser(result);
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
        [Trait("Description", "HttpRequestHeaderParser.ParseBuffer accepts valid versions.")]
        public void RequestHeaderParserAcceptsValidVersion(Version version)
        {
            byte[] data = CreateBuffer("GET", "/", String.Format("HTTP/{0}", version.ToString(2)), ParserData.ValidHeaders);

            for (var cnt = 1; cnt <= data.Length; cnt++)
            {
                HttpUnsortedRequest result = new HttpUnsortedRequest();
                HttpRequestHeaderParser parser = new HttpRequestHeaderParser(result);
                Assert.NotNull(parser);

                int totalBytesConsumed = 0;
                ParserState state = ParseBufferInSteps(parser, data, cnt, out totalBytesConsumed);
                Assert.Equal(data.Length, totalBytesConsumed);

                ValidateResult(result, "GET", "/", version, ParserData.ValidHeaders);
            }
        }

        public static IEnumerable<object[]> InvalidVersions
        {
            get { return ParserData.InvalidVersions; }
        }

        [Theory]
        [PropertyData("InvalidVersions")]
        [Trait("Description", "HttpRequestHeaderParser.ParseBuffer rejects lower case protocol version.")]
        public void RequestHeaderParserRejectsInvalidVersion(string invalidVersion)
        {
            byte[] data = CreateBuffer("GET", "/", invalidVersion, ParserData.ValidHeaders);

            for (var cnt = 1; cnt <= data.Length; cnt++)
            {
                HttpUnsortedRequest result = new HttpUnsortedRequest();
                HttpRequestHeaderParser parser = new HttpRequestHeaderParser(result);
                Assert.NotNull(parser);

                int totalBytesConsumed = 0;
                ParserState state = ParseBufferInSteps(parser, data, cnt, out totalBytesConsumed);
                Assert.Equal(ParserState.Invalid, state);
            }
        }
    }
}