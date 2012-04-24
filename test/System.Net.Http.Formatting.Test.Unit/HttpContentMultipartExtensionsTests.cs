// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting.Parsers;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.TestCommon;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;
using FactAttribute = Microsoft.TestCommon.DefaultTimeoutFactAttribute;
using TheoryAttribute = Microsoft.TestCommon.DefaultTimeoutTheoryAttribute;

namespace System.Net.Http
{
    public class HttpContentMultipartExtensionsTests
    {
        private const string ValidBoundary = "-A-";
        private const string DefaultContentType = "text/plain";
        private const string DefaultContentDisposition = "form-data";
        private const string ExceptionStreamProviderMessage = "Bad Stream Provider!";
        private const string ExceptionSyncStreamMessage = "Bad Sync Stream!";
        private const string ExceptionAsyncStreamMessage = "Bad Async Stream!";
        private const string LongText = "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789";

        public static TheoryDataSet<string, bool, string, bool> IsMimeMultipartContentTestData
        {
            get
            {
                return new TheoryDataSet<string, bool, string, bool>
                {
                    { "text/plain", false, "plain", false },
                    { "application/*", false, "related", false },
                    { "*/*", false, "related", false },
                    { "multipart/form-data", false, "form-data", false },
                    { "multipart/form-data; boundary=1234", true, "related", false },
                    { "multipart/form-data; boundary=1234; charset=utf-8", true, "form-data", true },
                    { "multipart/form-data; boundary=1234; charset=utf-8", true, "form-data", true },
                    { "Multipart/Related; boundary=example-1; start=\"<950120.aaCC@XIson.com>\"; type=\"Application/X-FixedRecord\"; start-info=\"-o ps\"", true, "related", true },
                };
            }
        }

        public static TheoryDataSet<string> Boundaries
        {
            get
            {
                return new TheoryDataSet<string>
                 {
                    "1",
                    "a",
                    "'",
                    "(",
                    ")",
                    "+",
                    "_",
                    "-",
                    ".",
                    "/",
                    ":",
                    "=",
                    "?",
                    "--",
                    "--------------------01234567890123456789",
                    "--------------------01234567890123456789--------------------",
                    "--A--B--C--D--E--F--",
                 };
            }
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
        public void IsMimeMultipartContent_ThrowsOnNullContent()
        {
            Assert.ThrowsArgumentNull(() => HttpContentMultipartExtensions.IsMimeMultipartContent(null), "content");
        }

        [Fact]
        public void IsMimeMultipartContent_ThrowsOnNullSubType()
        {
            StringContent content = new StringContent(String.Empty);
            Assert.ThrowsArgumentNull(() => HttpContentMultipartExtensions.IsMimeMultipartContent(content, null), "subtype");
        }

        [Theory]
        [PropertyData("IsMimeMultipartContentTestData")]
        public void IsMimeMultipartContent_ReturnsCorrectValue(string mediaType, bool isMultipart, string subtype, bool hasSubtype)
        {
            StringContent content = new StringContent(String.Empty);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse(mediaType);

            Assert.Equal(isMultipart, content.IsMimeMultipartContent());
            Assert.Equal(hasSubtype, content.IsMimeMultipartContent(subtype));
        }

        [Fact]
        public void ReadAsMultipartAsync_ThrowsOnNullStreamProvider()
        {
            HttpContent content = CreateContent(ValidBoundary);
            Assert.ThrowsArgumentNull(() => content.ReadAsMultipartAsync((MultipartMemoryStreamProvider)null), "streamProvider");
        }

        [Fact]
        public void ReadAsMultipartAsync_ThrowsOnInvalidBufferSize()
        {
            HttpContent content = CreateContent(ValidBoundary);
            Assert.ThrowsArgumentGreaterThanOrEqualTo(
                () => content.ReadAsMultipartAsync(new MultipartMemoryStreamProvider(), ParserData.MinBufferSize - 1),
                "bufferSize", ParserData.MinBufferSize.ToString(), ParserData.MinBufferSize - 1);
        }

        [Theory]
        [PropertyData("IsMimeMultipartContentTestData")]
        public void ReadAsMultipartAsync_DetectsNonMultipartContent(string mediaType, bool isMultipart, string subtype, bool hasSubtype)
        {
            StringContent content = new StringContent(String.Empty);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse(mediaType);
            if (!isMultipart)
            {
                Assert.ThrowsArgument(() => content.ReadAsMultipartAsync().Result, "content");
            }
        }

        [Theory]
        [PropertyData("Boundaries")]
        public void ReadAsMultipartAsync_ParsesContent(string boundary)
        {
            HttpContent successContent;
            MultipartMemoryStreamProvider result;

            successContent = CreateContent(boundary, "A", "B", "C");
            result = successContent.ReadAsMultipartAsync().Result;
            Assert.Equal(3, result.Contents.Count);

            successContent = CreateContent(boundary, "A", "B", "C");
            result = successContent.ReadAsMultipartAsync(new MultipartMemoryStreamProvider()).Result;
            Assert.Equal(3, result.Contents.Count);

            successContent = CreateContent(boundary, "A", "B", "C");
            result = successContent.ReadAsMultipartAsync(new MultipartMemoryStreamProvider(), 1024).Result;
            Assert.Equal(3, result.Contents.Count);
        }

        [Theory]
        [PropertyData("Boundaries")]
        public void ReadAsMultipartAsync_ParsesEmptyContent(string boundary)
        {
            HttpContent content = CreateContent(boundary);
            MultipartMemoryStreamProvider result = content.ReadAsMultipartAsync().Result;
            Assert.Empty(result.Contents);
        }

        [Fact]
        public void ReadAsMultipartAsync_ThrowsOnBadStreamProvider()
        {
            HttpContent content = CreateContent(ValidBoundary, "A", "B", "C");
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => content.ReadAsMultipartAsync(new BadStreamProvider()).Result);
            Assert.NotNull(exception.InnerException);
            Assert.Equal(ExceptionStreamProviderMessage, exception.InnerException.Message);
        }

        [Fact]
        public void ReadAsMultipartAsync_ThrowsOnNullProvider()
        {
            HttpContent content = CreateContent(ValidBoundary, "A", "B", "C");
            Assert.Throws<InvalidOperationException>(() => content.ReadAsMultipartAsync(new NullProvider()).Result);
        }

        [Fact]
        public void ReadAsMultipartAsync_ThrowsOnReadOnlyStream()
        {
            HttpContent content = CreateContent(ValidBoundary, "A", "B", "C");
            Assert.Throws<InvalidOperationException>(() => content.ReadAsMultipartAsync(new ReadOnlyStreamProvider()).Result);
        }

        [Fact]
        public void ReadAsMultipartAsync_ThrowsOnPrematureEndOfStream()
        {
            HttpContent content = new StreamContent(Stream.Null);
            string mediaType = String.Format("multipart/form-data; boundary=\"{0}\"", ValidBoundary);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse(mediaType);
            Assert.Throws<IOException>(() => content.ReadAsMultipartAsync().Result);
        }

        [Fact]
        public void ReadAsMultipartAsync_ThrowsOnReadError()
        {
            HttpContent content = new StreamContent(new ReadErrorStream());
            string mediaType = String.Format("multipart/form-data; boundary=\"{0}\"", ValidBoundary);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse(mediaType);
            IOException exception = Assert.Throws<IOException>(() => content.ReadAsMultipartAsync().Result);
            Assert.NotNull(exception.InnerException);
            Assert.Equal(ExceptionAsyncStreamMessage, exception.InnerException.Message);
        }

        [Fact]
        public void ReadAsMultipartAsync_ThrowsOnWriteError()
        {
            HttpContent content = CreateContent(ValidBoundary, "A", "B", "C");
            IOException exception = Assert.Throws<IOException>(() => content.ReadAsMultipartAsync(new WriteErrorStreamProvider()).Result);
            Assert.NotNull(exception.InnerException);
            Assert.Equal(ExceptionAsyncStreamMessage, exception.InnerException.Message);
        }

        [Theory]
        [PropertyData("Boundaries")]
        public void ReadAsMultipartAsync_SingleShortBodyPart(string boundary)
        {
            HttpContent content = CreateContent(boundary, "A");

            MultipartMemoryStreamProvider result = content.ReadAsMultipartAsync().Result;
            Assert.Equal(1, result.Contents.Count);
            Assert.Equal("A", result.Contents[0].ReadAsStringAsync().Result);
            ValidateContents(result.Contents);
        }

        [Theory]
        [PropertyData("Boundaries")]
        public void ReadAsMultipartAsync_MultipleShortBodyParts(string boundary)
        {
            string[] text = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

            HttpContent content = CreateContent(boundary, text);
            MultipartMemoryStreamProvider result = content.ReadAsMultipartAsync().Result;
            Assert.Equal(text.Length, result.Contents.Count);
            for (var check = 0; check < text.Length; check++)
            {
                Assert.Equal(text[check], result.Contents[check].ReadAsStringAsync().Result);
            }

            ValidateContents(result.Contents);
        }

        [Theory]
        [PropertyData("Boundaries")]
        public void ReadAsMultipartAsync_SingleLongBodyPart(string boundary)
        {
            HttpContent content = CreateContent(boundary, LongText);

            MultipartMemoryStreamProvider result = content.ReadAsMultipartAsync().Result;
            Assert.Equal(1, result.Contents.Count);
            Assert.Equal(LongText, result.Contents[0].ReadAsStringAsync().Result);
            ValidateContents(result.Contents);
        }

        [Theory]
        [PropertyData("Boundaries")]
        public void ReadAsMultipartAsync_MultipleLongBodyParts(string boundary)
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
            MultipartMemoryStreamProvider result = content.ReadAsMultipartAsync(new MultipartMemoryStreamProvider(), ParserData.MinBufferSize).Result;
            Assert.Equal(text.Length, result.Contents.Count);
            for (var check = 0; check < text.Length; check++)
            {
                Assert.Equal(text[check], result.Contents[check].ReadAsStringAsync().Result);
            }

            ValidateContents(result.Contents);
        }

        [Theory]
        [PropertyData("Boundaries")]
        public void ReadAsMultipartAsync_UsingMultipartContent(string boundary)
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

            MultipartMemoryStreamProvider result = byteContent.ReadAsMultipartAsync().Result;
            Assert.Equal(3, result.Contents.Count);
            Assert.Equal("A", result.Contents[0].ReadAsStringAsync().Result);
            Assert.Equal("B", result.Contents[1].ReadAsStringAsync().Result);
            Assert.Equal("C", result.Contents[2].ReadAsStringAsync().Result);
        }

        [Theory]
        [PropertyData("Boundaries")]
        public void ReadAsMultipartAsync_NestedMultipartContent(string boundary)
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
                MultipartMemoryStreamProvider result = content.ReadAsMultipartAsync().Result;
                Assert.Equal(1, result.Contents.Count);
                content = result.Contents[0];
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

        public class BadStreamProvider : MultipartStreamProvider
        {
            public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
            {
                throw new Exception(ExceptionStreamProviderMessage);
            }
        }

        public class NullProvider : MultipartStreamProvider
        {
            public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
            {
                return null;
            }
        }

        public class ReadOnlyStreamProvider : MultipartStreamProvider
        {
            public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
            {
                return new ReadOnlyStream();
            }
        }

        public class WriteErrorStreamProvider : MultipartStreamProvider
        {
            public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
            {
                return new WriteErrorStream();
            }
        }
    }
}
