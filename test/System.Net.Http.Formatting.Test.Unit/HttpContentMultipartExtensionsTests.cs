// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting.Parsers;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TestCommon;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;
using FactAttribute = Microsoft.TestCommon.DefaultTimeoutFactAttribute;
using TheoryAttribute = Microsoft.TestCommon.DefaultTimeoutTheoryAttribute;

namespace System.Net.Http
{
    public class HttpContentMultipartExtensionsTests
    {
        private const string DefaultContentType = "text/plain";
        private const string DefaultContentDisposition = "form-data";
        private const string ExceptionStreamProviderMessage = "Bad Stream Provider!";
        private const string ExceptionSyncStreamMessage = "Bad Sync Stream!";
        private const string ExceptionAsyncStreamMessage = "Bad Async Stream!";
        private const string LongText = "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789";

        [Fact]
        [Trait("Description", "HttpContentMultipartExtensionMethods is a public static class")]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties(
                typeof(HttpContentMultipartExtensions),
                TypeAssert.TypeProperties.IsPublicVisibleClass |
                TypeAssert.TypeProperties.IsStatic);
        }

        private static HttpContent CreateContent(string boundary, params string[] bodyEntity)
        {
            List<string> entities = new List<string>();
            int cnt = 0;
            foreach (var body in bodyEntity)
            {
                byte[] header = InternetMessageFormatHeaderParserTests.CreateBuffer(
                    String.Format("N{0}: V{0}", cnt),
                    String.Format("Content-Type: {0}", DefaultContentType),
                    String.Format("Content-Disposition: {0}; FileName=\"N{1}\"", DefaultContentDisposition, cnt));
                entities.Add(Encoding.UTF8.GetString(header) + body);
                cnt++;
            }

            byte[] message = MimeMultipartParserTests.CreateBuffer(boundary, entities.ToArray());
            HttpContent result = new ByteArrayContent(message);
            var contentType = new MediaTypeHeaderValue("multipart/form-data");
            contentType.Parameters.Add(new NameValueHeaderValue("boundary", String.Format("\"{0}\"", boundary)));
            result.Headers.ContentType = contentType;
            return result;
        }

        private static void ValidateContents(IEnumerable<HttpContent> contents)
        {
            int cnt = 0;
            foreach (var content in contents)
            {
                Assert.NotNull(content);
                Assert.NotNull(content.Headers);
                Assert.Equal(4, content.Headers.Count());

                IEnumerable<string> parsedValues = content.Headers.GetValues(String.Format("N{0}", cnt));
                Assert.Equal(1, parsedValues.Count());
                Assert.Equal(String.Format("V{0}", cnt), parsedValues.ElementAt(0));

                Assert.Equal(DefaultContentType, content.Headers.ContentType.MediaType);

                Assert.Equal(DefaultContentDisposition, content.Headers.ContentDisposition.DispositionType);
                Assert.Equal(String.Format("\"N{0}\"", cnt), content.Headers.ContentDisposition.FileName);

                cnt++;
            }
        }

        [Fact]
        public void ReadAsMultipartAsync_DetectsNonMultipartContent()
        {
            Assert.ThrowsArgumentNull(() => HttpContentMultipartExtensions.IsMimeMultipartContent(null), "content");
            Assert.ThrowsArgument(() => new ByteArrayContent(new byte[0]).ReadAsMultipartAsync().Result, "content");
            Assert.ThrowsArgument(() => new StringContent(String.Empty).ReadAsMultipartAsync().Result, "content");
            Assert.ThrowsArgument(() => new StringContent(String.Empty, Encoding.UTF8, "multipart/form-data").ReadAsMultipartAsync().Result, "content");
        }

        public static IEnumerable<object[]> Boundaries
        {
            get { return ParserData.Boundaries; }
        }

        [Fact]
        public void ReadAsMultipartAsync_NullStreamProviderThrows()
        {
            HttpContent content = CreateContent("---");

            Assert.ThrowsArgumentNull(() =>
            {
                content.ReadAsMultipartAsync(null);
            }, "streamProvider");
        }

        [Theory]
        [PropertyData("Boundaries")]
        [Trait("Description", "ReadAsMultipartAsync(HttpContent, IMultipartStreamProvider, int) throws on buffersize.")]
        public void ReadAsMultipartAsyncStreamProviderThrowsOnBufferSize(string boundary)
        {
            HttpContent content = CreateContent(boundary);
            Assert.NotNull(content);

            Assert.ThrowsArgumentGreaterThanOrEqualTo(
                () => content.ReadAsMultipartAsync(new MemoryStreamProvider(), ParserData.MinBufferSize - 1),
                "bufferSize", ParserData.MinBufferSize.ToString(), ParserData.MinBufferSize - 1);
        }

        [Fact]
        [Trait("Description", "IsMimeMultipartContent(HttpContent) checks extension method arguments.")]
        public void IsMimeMultipartContentVerifyArguments()
        {
            Assert.ThrowsArgumentNull(() =>
            {
                HttpContent content = null;
                HttpContentMultipartExtensions.IsMimeMultipartContent(content);
            }, "content");
        }

        [Fact]
        public void IsMumeMultipartContentReturnsFalseForEmptyValues()
        {
            Assert.False(new ByteArrayContent(new byte[] { }).IsMimeMultipartContent(), "HttpContent should not be valid MIME multipart content");

            Assert.False(new StringContent(String.Empty).IsMimeMultipartContent(), "HttpContent should not be valid MIME multipart content");

            Assert.False(new StringContent(String.Empty, Encoding.UTF8, "multipart/form-data").IsMimeMultipartContent(), "HttpContent should not be valid MIME multipart content");
        }

        [Theory]
        [PropertyData("Boundaries")]
        [Trait("Description", "IsMimeMultipartContent(HttpContent) responds correctly to MIME multipart and other content")]
        public void IsMimeMultipartContent(string boundary)
        {
            HttpContent content = CreateContent(boundary);
            Assert.NotNull(content);
            Assert.True(content.IsMimeMultipartContent());
        }

        [Theory]
        [PropertyData("Boundaries")]
        [Trait("Description", "IsMimeMultipartContent(HttpContent, string) throws on null string.")]
        public void IsMimeMultipartContentThrowsOnNullString(string boundary)
        {
            HttpContent content = CreateContent(boundary);
            Assert.NotNull(content);
            foreach (var subtype in CommonUnitTestDataSets.EmptyStrings)
            {
                Assert.ThrowsArgumentNull(() =>
                    {
                        content.IsMimeMultipartContent(subtype);
                    }, "subtype");
            }
        }

        [Fact]
        public void ReadAsMultipartAsync_SuccessfullyParsesContent()
        {
            HttpContent successContent;
            Task<IEnumerable<HttpContent>> task;
            IEnumerable<HttpContent> result;

            successContent = CreateContent("boundary", "A", "B", "C");
            task = successContent.ReadAsMultipartAsync();
            task.Wait(TimeoutConstant.DefaultTimeout);
            result = task.Result;
            Assert.Equal(3, result.Count());

            successContent = CreateContent("boundary", "A", "B", "C");
            task = successContent.ReadAsMultipartAsync(new MemoryStreamProvider());
            task.Wait(TimeoutConstant.DefaultTimeout);
            result = task.Result;
            Assert.Equal(3, result.Count());

            successContent = CreateContent("boundary", "A", "B", "C");
            task = successContent.ReadAsMultipartAsync(new MemoryStreamProvider(), 1024);
            task.Wait(TimeoutConstant.DefaultTimeout);
            result = task.Result;
            Assert.Equal(3, result.Count());
        }

        [Theory]
        [PropertyData("Boundaries")]
        public void ReadAsMultipartAsync_ParsesEmptyContentSuccessfully(string boundary)
        {
            HttpContent content = CreateContent(boundary);
            Task<IEnumerable<HttpContent>> task = content.ReadAsMultipartAsync();
            task.Wait(TimeoutConstant.DefaultTimeout);
            IEnumerable<HttpContent> result = task.Result;
            Assert.Equal(0, result.Count());
        }

        [Fact]
        public void ReadAsMultipartAsync_WithBadStreamProvider_Throws()
        {
            HttpContent content = CreateContent("--", "A", "B", "C");

            var invalidOperationException = Assert.Throws<InvalidOperationException>(
                () => content.ReadAsMultipartAsync(new BadStreamProvider()).Result,
                "The stream provider of type 'BadStreamProvider' threw an exception."
            );
            Assert.NotNull(invalidOperationException.InnerException);
            Assert.Equal(ExceptionStreamProviderMessage, invalidOperationException.InnerException.Message);
        }

        [Fact]
        public void ReadAsMultipartAsync_NullStreamProvider_Throws()
        {
            HttpContent content = CreateContent("--", "A", "B", "C");

            Assert.Throws<InvalidOperationException>(
                () => content.ReadAsMultipartAsync(new NullStreamProvider()).Result,
                "The stream provider of type 'NullStreamProvider' returned null. It must return a writable 'Stream' instance."
            );
        }

        [Fact]
        public void ReadAsMultipartAsync_ReadOnlyStream_Throws()
        {
            HttpContent content = CreateContent("--", "A", "B", "C");

            Assert.Throws<InvalidOperationException>(
                () => content.ReadAsMultipartAsync(new ReadOnlyStreamProvider()).Result,
                "The stream provider of type 'ReadOnlyStreamProvider' returned a read-only stream. It must return a writable 'Stream' instance."
            );
        }

        [Fact]
        public void ReadAsMultipartAsync_PrematureEndOfStream_Throws()
        {
            HttpContent content = new StreamContent(Stream.Null);
            var contentType = new MediaTypeHeaderValue("multipart/form-data");
            contentType.Parameters.Add(new NameValueHeaderValue("boundary", "\"{--\""));
            content.Headers.ContentType = contentType;

            Assert.Throws<IOException>(
                () => content.ReadAsMultipartAsync().Result,
                "Unexpected end of MIME multipart stream. MIME multipart message is not complete."
            );
        }

        [Fact]
        public void ReadAsMultipartAsync_ReadErrorOnStream_Throws()
        {
            HttpContent content = new StreamContent(new ReadErrorStream());
            var contentType = new MediaTypeHeaderValue("multipart/form-data");
            contentType.Parameters.Add(new NameValueHeaderValue("boundary", "\"--\""));
            content.Headers.ContentType = contentType;

            var ioException = Assert.Throws<IOException>(
                () => content.ReadAsMultipartAsync().Result,
                "Error reading MIME multipart body part."
            );
            Assert.NotNull(ioException.InnerException);
            Assert.Equal(ExceptionAsyncStreamMessage, ioException.InnerException.Message);
        }

        [Fact]
        public void ReadAsMultipartAsync_WriteErrorOnStream_Throws()
        {
            HttpContent content = CreateContent("--", "A", "B", "C");

            var ioException = Assert.Throws<IOException>(
                () => content.ReadAsMultipartAsync(new WriteErrorStreamProvider()).Result,
                "Error writing MIME multipart body part to output stream."
            );
            Assert.NotNull(ioException.InnerException);
            Assert.Equal(ExceptionAsyncStreamMessage, ioException.InnerException.Message);
        }

        [Theory]
        [PropertyData("Boundaries")]
        public void ReadAsMultipartAsync_SingleShortBodyPart_ParsesSuccessfully(string boundary)
        {
            HttpContent content = CreateContent(boundary, "A");
            IEnumerable<HttpContent> result = content.ReadAsMultipartAsync().Result;
            Assert.Equal(1, result.Count());
            Assert.Equal("A", result.ElementAt(0).ReadAsStringAsync().Result);
            ValidateContents(result);
        }

        [Theory]
        [PropertyData("Boundaries")]
        public void ReadAsMultipartAsync_MultipleShortBodyParts_ParsesSuccessfully(string boundary)
        {
            string[] text = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

            HttpContent content = CreateContent(boundary, text);
            IEnumerable<HttpContent> result = content.ReadAsMultipartAsync().Result;
            Assert.Equal(text.Length, result.Count());
            for (var check = 0; check < text.Length; check++)
            {
                Assert.Equal(text[check], result.ElementAt(check).ReadAsStringAsync().Result);
            }

            ValidateContents(result);
        }

        [Theory]
        [PropertyData("Boundaries")]
        [Trait("Description", "ReadAsMultipartAsync(HttpContent) parses with single long body asynchronously.")]
        public void ReadAsMultipartAsyncSingleLongBodyPartAsync(string boundary)
        {
            HttpContent content = CreateContent(boundary, LongText);
            Task<IEnumerable<HttpContent>> task = content.ReadAsMultipartAsync();
            task.Wait(TimeoutConstant.DefaultTimeout);
            IEnumerable<HttpContent> result = task.Result;
            Assert.Equal(1, result.Count());
            Assert.Equal(LongText, result.ElementAt(0).ReadAsStringAsync().Result);

            ValidateContents(result);
        }

        [Theory]
        [PropertyData("Boundaries")]
        [Trait("Description", "ReadAsMultipartAsync(HttpContent) parses with multiple long bodies asynchronously.")]
        public void ReadAsMultipartAsyncMultipleLongBodyPartsAsync(string boundary)
        {
            string[] text = new string[] { 
                "A" + LongText + "A", 
                "B" + LongText + "B", 
                "C" + LongText + "C", 
                "D" + LongText + "D", 
                "E" + LongText + "E", 
                "F" + LongText + "F", 
                "G" + LongText + "G", 
                "H" + LongText + "H", 
                "I" + LongText + "I", 
                "J" + LongText + "J", 
                "K" + LongText + "K", 
                "L" + LongText + "L", 
                "M" + LongText + "M", 
                "N" + LongText + "N", 
                "O" + LongText + "O", 
                "P" + LongText + "P", 
                "Q" + LongText + "Q", 
                "R" + LongText + "R", 
                "S" + LongText + "S", 
                "T" + LongText + "T", 
                "U" + LongText + "U", 
                "V" + LongText + "V", 
                "W" + LongText + "W", 
                "X" + LongText + "X", 
                "Y" + LongText + "Y", 
                "Z" + LongText + "Z"};

            HttpContent content = CreateContent(boundary, text);
            Task<IEnumerable<HttpContent>> task = content.ReadAsMultipartAsync(new MemoryStreamProvider(), ParserData.MinBufferSize);
            task.Wait(TimeoutConstant.DefaultTimeout);
            IEnumerable<HttpContent> result = task.Result;
            Assert.Equal(text.Length, result.Count());
            for (var check = 0; check < text.Length; check++)
            {
                Assert.Equal(text[check], result.ElementAt(check).ReadAsStringAsync().Result);
            }

            ValidateContents(result);
        }

        [Theory]
        [PropertyData("Boundaries")]
        [Trait("Description", "ReadAsMultipartAsync(HttpContent) parses content generated by MultipartContent asynchronously.")]
        public void ReadAsMultipartAsyncUsingMultipartContentAsync(string boundary)
        {
            MultipartContent content = new MultipartContent("mixed", boundary);
            content.Add(new StringContent("A"));
            content.Add(new StringContent("B"));
            content.Add(new StringContent("C"));

            MemoryStream memStream = new MemoryStream();
            content.CopyToAsync(memStream).Wait();
            memStream.Position = 0;
            byte[] data = memStream.ToArray();
            var byteContent = new ByteArrayContent(data);
            byteContent.Headers.ContentType = content.Headers.ContentType;

            Task<IEnumerable<HttpContent>> task = byteContent.ReadAsMultipartAsync();
            task.Wait(TimeoutConstant.DefaultTimeout);
            IEnumerable<HttpContent> result = task.Result;
            Assert.Equal(3, result.Count());
            Assert.Equal("A", result.ElementAt(0).ReadAsStringAsync().Result);
            Assert.Equal("B", result.ElementAt(1).ReadAsStringAsync().Result);
            Assert.Equal("C", result.ElementAt(2).ReadAsStringAsync().Result);
        }

        [Theory]
        [PropertyData("Boundaries")]
        [Trait("Description", "ReadAsMultipartAsync(HttpContent) parses nested content generated by MultipartContent asynchronously.")]
        public void ReadAsMultipartAsyncNestedMultipartContentAsync(string boundary)
        {
            const int nesting = 10;
            const string innerText = "Content";

            MultipartContent innerContent = new MultipartContent("mixed", boundary);
            innerContent.Add(new StringContent(innerText));
            for (var cnt = 0; cnt < nesting; cnt++)
            {
                string outerBoundary = String.Format("{0}_{1}", boundary, cnt);
                MultipartContent outerContent = new MultipartContent("mixed", outerBoundary);
                outerContent.Add(innerContent);
                innerContent = outerContent;
            }

            MemoryStream memStream = new MemoryStream();
            innerContent.CopyToAsync(memStream).Wait();
            memStream.Position = 0;
            byte[] data = memStream.ToArray();
            HttpContent content = new ByteArrayContent(data);
            content.Headers.ContentType = innerContent.Headers.ContentType;

            for (var cnt = 0; cnt < nesting + 1; cnt++)
            {
                Task<IEnumerable<HttpContent>> task = content.ReadAsMultipartAsync();
                task.Wait(TimeoutConstant.DefaultTimeout);
                IEnumerable<HttpContent> result = task.Result;
                Assert.Equal(1, result.Count());
                content = result.ElementAt(0);
                Assert.NotNull(content);
            }

            string text = content.ReadAsStringAsync().Result;
            Assert.Equal(innerText, text);
        }

        public class ReadOnlyStream : MemoryStream
        {
            public override bool CanWrite
            {
                get
                {
                    return false;
                }
            }
        }

        public class ReadErrorStream : MemoryStream
        {
            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new Exception(ExceptionSyncStreamMessage);
            }

            public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                throw new Exception(ExceptionAsyncStreamMessage);
            }
        }

        public class WriteErrorStream : MemoryStream
        {
            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new Exception(ExceptionSyncStreamMessage);
            }

            public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                throw new Exception(ExceptionAsyncStreamMessage);
            }
        }

        public class MemoryStreamProvider : IMultipartStreamProvider
        {
            public Stream GetStream(HttpContentHeaders headers)
            {
                return new MemoryStream();
            }
        }

        public class BadStreamProvider : IMultipartStreamProvider
        {
            public Stream GetStream(HttpContentHeaders headers)
            {
                throw new Exception(ExceptionStreamProviderMessage);
            }
        }

        public class NullStreamProvider : IMultipartStreamProvider
        {
            public Stream GetStream(HttpContentHeaders headers)
            {
                return null;
            }
        }

        public class ReadOnlyStreamProvider : IMultipartStreamProvider
        {
            public Stream GetStream(HttpContentHeaders headers)
            {
                return new ReadOnlyStream();
            }
        }

        public class WriteErrorStreamProvider : IMultipartStreamProvider
        {
            public Stream GetStream(HttpContentHeaders headers)
            {
                return new WriteErrorStream();
            }
        }
    }
}
