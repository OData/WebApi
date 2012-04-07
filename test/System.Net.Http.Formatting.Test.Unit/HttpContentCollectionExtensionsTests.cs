// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.TestCommon;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Net.Http
{
    public class HttpContentCollectionExtensionsTests
    {
        private const string contentID = "content-id";
        private const string matchContentID = "matchID";
        private const string matchContentType = "text/plain";
        private const string matchDispositionName = "N1";
        private const string quotedMatchDispositionName = "\"" + matchDispositionName + "\"";
        private const string matchDispositionType = "form-data";
        private const string quotedMatchDispositionType = "\"" + matchDispositionType + "\"";

        private const string noMatchContentID = "nomatchID";
        private const string noMatchContentType = "text/nomatch";
        private const string noMatchDispositionName = "nomatchName";
        private const string noMatchDispositionType = "nomatchType";

        [Fact]
        [Trait("Description", "IEnumerableHttpContentExtensionMethods is a public static class.")]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties(
                typeof(HttpContentCollectionExtensions),
                TypeAssert.TypeProperties.IsPublicVisibleClass |
                TypeAssert.TypeProperties.IsStatic);
        }


        private static IEnumerable<HttpContent> CreateContent()
        {
            MultipartFormDataContent multipart = new MultipartFormDataContent();

            multipart.Add(new StringContent("A", UTF8Encoding.UTF8, matchContentType), matchDispositionName);
            multipart.Add(new StringContent("B", UTF8Encoding.UTF8, matchContentType), "N2");
            multipart.Add(new StringContent("C", UTF8Encoding.UTF8, matchContentType), "N3");

            multipart.Add(new ByteArrayContent(new byte[] { 0x65 }), "N4");
            multipart.Add(new ByteArrayContent(new byte[] { 0x65 }), "N5");
            multipart.Add(new ByteArrayContent(new byte[] { 0x65 }), "N6");

            HttpContent cidContent = new StringContent("<html>A</html>", UTF8Encoding.UTF8, "text/html");
            cidContent.Headers.Add(contentID, matchContentID);
            multipart.Add(cidContent);

            return multipart;
        }

        private static void ClearHeaders(IEnumerable<HttpContent> contents)
        {
            foreach (var c in contents)
            {
                c.Headers.Clear();
            }
        }




        [Fact]
        [Trait("Description", "FindAllContentType(IEnumerable<HttpContent>, string) throws on null.")]
        public void FindAllContentTypeString()
        {
            Assert.ThrowsArgumentNull(() => { HttpContentCollectionExtensions.FindAllContentType(null, "text/plain"); }, "contents");

            IEnumerable<HttpContent> content = HttpContentCollectionExtensionsTests.CreateContent();
            Assert.ThrowsArgumentNull(() => { HttpContentCollectionExtensions.FindAllContentType(content, (string)null); }, "contentType");
            foreach (string empty in TestData.EmptyStrings)
            {
                Assert.ThrowsArgumentNull(() => { HttpContentCollectionExtensions.FindAllContentType(content, empty); }, "contentType");
            }
        }

        [Fact]
        [Trait("Description", "FindAllContentType(IEnumerable<HttpContent>, string) no match.")]
        public void FindAllContentTypeStringNoMatch()
        {
            IEnumerable<HttpContent> content = HttpContentCollectionExtensionsTests.CreateContent();
            IEnumerable<HttpContent> result = null;
            result = content.FindAllContentType(noMatchContentType);
            Assert.Equal(0, result.Count());

            ClearHeaders(content);
            result = content.FindAllContentType(noMatchContentType);
            Assert.Equal(0, result.Count());
        }

        [Fact]
        [Trait("Description", "FindAllContentType(IEnumerable<HttpContent>, string) match.")]
        public void FindAllContentTypeStringMatch()
        {
            IEnumerable<HttpContent> content = HttpContentCollectionExtensionsTests.CreateContent();
            IEnumerable<HttpContent> result = content.FindAllContentType(matchContentType);
            Assert.Equal(3, result.Count());
        }

        [Fact]
        [Trait("Description", "FindAllContentType(IEnumerable<HttpContent>, MediaTypeHeaderValue) throws on null.")]
        public void FindAllContentTypeMediaTypeThrows()
        {
            Assert.ThrowsArgumentNull(() => { HttpContentCollectionExtensions.FindAllContentType(null, new MediaTypeHeaderValue("text/plain")); }, "contents");

            IEnumerable<HttpContent> content = HttpContentCollectionExtensionsTests.CreateContent();
            Assert.ThrowsArgumentNull(() => { HttpContentCollectionExtensions.FindAllContentType(content, (MediaTypeHeaderValue)null); }, "contentType");
        }

        [Fact]
        [Trait("Description", "FindAllContentType(IEnumerable<HttpContent>, MediaTypeHeaderValue) no match.")]
        public void FindAllContentTypeMediaTypeNoMatch()
        {
            IEnumerable<HttpContent> content = HttpContentCollectionExtensionsTests.CreateContent();
            IEnumerable<HttpContent> result = null;

            result = content.FindAllContentType(new MediaTypeHeaderValue(noMatchContentType));
            Assert.Equal(0, result.Count());

            ClearHeaders(content);
            result = content.FindAllContentType(new MediaTypeHeaderValue(noMatchContentType));
            Assert.Equal(0, result.Count());
        }

        [Fact]
        [Trait("Description", "FindAllContentType(IEnumerable<HttpContent>, string) match.")]
        public void FindAllContentTypeMediaTypeMatch()
        {
            IEnumerable<HttpContent> content = HttpContentCollectionExtensionsTests.CreateContent();
            IEnumerable<HttpContent> result = content.FindAllContentType(new MediaTypeHeaderValue(matchContentType));
            Assert.Equal(3, result.Count());
        }

        [Fact]
        [Trait("Description", "FirstDispositionName(IEnumerable<HttpContent>, string) throws on null.")]
        public void FirstDispositionNameThrows()
        {
            Assert.ThrowsArgumentNull(() => { HttpContentCollectionExtensions.FirstDispositionName(null, "A"); }, "contents");

            IEnumerable<HttpContent> content = HttpContentCollectionExtensionsTests.CreateContent();
            Assert.ThrowsArgumentNull(() => { HttpContentCollectionExtensions.FirstDispositionName(content, null); }, "dispositionName");
            foreach (string empty in TestData.EmptyStrings)
            {
                Assert.ThrowsArgumentNull(() => { HttpContentCollectionExtensions.FirstDispositionName(content, empty); }, "dispositionName");
            }
        }

        [Fact]
        [Trait("Description", "FirstDispositionName(IEnumerable<HttpContent>, string) no match.")]
        public void FirstDispositionNameNoMatch()
        {
            IEnumerable<HttpContent> content = HttpContentCollectionExtensionsTests.CreateContent();
            Assert.Null(content.FirstDispositionNameOrDefault(noMatchDispositionName));

            ClearHeaders(content);
            Assert.Throws<InvalidOperationException>(() => content.FirstDispositionName(noMatchDispositionName));
        }

        [Fact]
        [Trait("Description", "FirstDispositionName(IEnumerable<HttpContent>, string) match.")]
        public void FirstDispositionNameMatch()
        {
            IEnumerable<HttpContent> content = HttpContentCollectionExtensionsTests.CreateContent();
            Assert.NotNull(content.FirstDispositionName(matchDispositionName));
            Assert.NotNull(content.FirstDispositionName(quotedMatchDispositionName));
        }

        [Fact]
        [Trait("Description", "FirstDispositionNameOrDefault(IEnumerable<HttpContent>, string) throws on null.")]
        public void FirstDispositionNameOrDefaultThrows()
        {
            Assert.ThrowsArgumentNull(() => { HttpContentCollectionExtensions.FirstDispositionNameOrDefault(null, "A"); }, "contents");

            IEnumerable<HttpContent> content = HttpContentCollectionExtensionsTests.CreateContent();
            Assert.ThrowsArgumentNull(() => { HttpContentCollectionExtensions.FirstDispositionNameOrDefault(content, null); }, "dispositionName");
            foreach (string empty in TestData.EmptyStrings)
            {
                Assert.ThrowsArgumentNull(() => { HttpContentCollectionExtensions.FirstDispositionNameOrDefault(content, empty); }, "dispositionName");
            }
        }

        [Fact]
        [Trait("Description", "FirstDispositionName(IEnumerable<HttpContent>, string) no match.")]
        public void FirstDispositionNameOrDefaultNoMatch()
        {
            IEnumerable<HttpContent> content = HttpContentCollectionExtensionsTests.CreateContent();
            Assert.Null(content.FirstDispositionNameOrDefault(noMatchDispositionName));

            ClearHeaders(content);
            Assert.Null(content.FirstDispositionNameOrDefault(noMatchDispositionName));
        }

        [Fact]
        [Trait("Description", "FirstDispositionName(IEnumerable<HttpContent>, string) match.")]
        public void FirstDispositionNameOrDefaultMatch()
        {
            IEnumerable<HttpContent> content = HttpContentCollectionExtensionsTests.CreateContent();
            Assert.NotNull(content.FirstDispositionNameOrDefault(matchDispositionName));
            Assert.NotNull(content.FirstDispositionNameOrDefault(quotedMatchDispositionName));
        }

        [Fact]
        [Trait("Description", "FirstDispositionType(IEnumerable<HttpContent>, string) throws on null.")]
        public void FirstDispositionTypeThrows()
        {
            Assert.ThrowsArgumentNull(() => { HttpContentCollectionExtensions.FirstDispositionType(null, "A"); }, "contents");

            IEnumerable<HttpContent> content = HttpContentCollectionExtensionsTests.CreateContent();
            Assert.ThrowsArgumentNull(() => { HttpContentCollectionExtensions.FirstDispositionType(content, null); }, "dispositionType");
            foreach (string empty in TestData.EmptyStrings)
            {
                Assert.ThrowsArgumentNull(() => { HttpContentCollectionExtensions.FirstDispositionType(content, empty); }, "dispositionType");
            }
        }

        [Fact]
        [Trait("Description", "FirstDispositionType(IEnumerable<HttpContent>, string) no match.")]
        public void FirstDispositionTypeNoMatch()
        {
            IEnumerable<HttpContent> content = HttpContentCollectionExtensionsTests.CreateContent();
            Assert.Throws<InvalidOperationException>(() => content.FirstDispositionType(noMatchDispositionType));

            Assert.Null(content.FirstDispositionTypeOrDefault(noMatchDispositionType));

            ClearHeaders(content);
            Assert.Throws<InvalidOperationException>(() => content.FirstDispositionType(noMatchDispositionType));
        }

        [Fact]
        [Trait("Description", "FirstDispositionType(IEnumerable<HttpContent>, string) match.")]
        public void FirstDispositionTypeMatch()
        {
            IEnumerable<HttpContent> content = HttpContentCollectionExtensionsTests.CreateContent();
            Assert.NotNull(content.FirstDispositionType(matchDispositionType));
            Assert.NotNull(content.FirstDispositionType(quotedMatchDispositionType));
        }

        [Fact]
        [Trait("Description", "FirstDispositionTypeOrDefault(IEnumerable<HttpContent>, string) throws on null.")]
        public void FirstDispositionTypeOrDefaultThrows()
        {
            Assert.ThrowsArgumentNull(() => { HttpContentCollectionExtensions.FirstDispositionTypeOrDefault(null, "A"); }, "contents");

            IEnumerable<HttpContent> content = HttpContentCollectionExtensionsTests.CreateContent();
            Assert.ThrowsArgumentNull(() => { HttpContentCollectionExtensions.FirstDispositionTypeOrDefault(content, null); }, "dispositionType");
            foreach (string empty in TestData.EmptyStrings)
            {
                Assert.ThrowsArgumentNull(() => { HttpContentCollectionExtensions.FirstDispositionTypeOrDefault(content, empty); }, "dispositionType");
            }
        }

        [Fact]
        [Trait("Description", "FirstDispositionTypeOrDefault(IEnumerable<HttpContent>, string) no match.")]
        public void FirstDispositionTypeOrDefaultNoMatch()
        {
            IEnumerable<HttpContent> content = HttpContentCollectionExtensionsTests.CreateContent();
            Assert.Null(content.FirstDispositionTypeOrDefault(noMatchDispositionType));

            ClearHeaders(content);
            Assert.Null(content.FirstDispositionTypeOrDefault(noMatchDispositionType));
        }

        [Fact]
        [Trait("Description", "FirstDispositionTypeOrDefault(IEnumerable<HttpContent>, string) match.")]
        public void FirstDispositionTypeOrDefaultMatch()
        {
            IEnumerable<HttpContent> content = HttpContentCollectionExtensionsTests.CreateContent();
            Assert.NotNull(content.FirstDispositionTypeOrDefault(matchDispositionType));
            Assert.NotNull(content.FirstDispositionTypeOrDefault(quotedMatchDispositionType));
        }

        [Fact]
        [Trait("Description", "FirstStart(IEnumerable<HttpContent>, string) throws on null.")]
        public void FirstStartThrows()
        {
            Assert.ThrowsArgumentNull(() => { HttpContentCollectionExtensions.FirstStart(null, "A"); }, "contents");

            IEnumerable<HttpContent> content = HttpContentCollectionExtensionsTests.CreateContent();
            Assert.ThrowsArgumentNull(() => { HttpContentCollectionExtensions.FirstStart(content, null); }, "start");
            foreach (string empty in TestData.EmptyStrings)
            {
                Assert.ThrowsArgumentNull(() => { HttpContentCollectionExtensions.FirstStart(content, empty); }, "start");
            }
        }

        [Fact]
        [Trait("Description", "FirstStart(IEnumerable<HttpContent>, string) no match.")]
        public void FirstStartNoMatch()
        {
            IEnumerable<HttpContent> content = HttpContentCollectionExtensionsTests.CreateContent();
            Assert.Throws<InvalidOperationException>(() => content.FirstStart(noMatchContentID));
        }

        [Fact]
        [Trait("Description", "FirstStart(IEnumerable<HttpContent>, string) match.")]
        public void FirstStartMatch()
        {
            IEnumerable<HttpContent> content = HttpContentCollectionExtensionsTests.CreateContent();
            Assert.NotNull(content.FirstStart(matchContentID));
        }

        [Fact]
        [Trait("Description", "FirstStartOrDefault(IEnumerable<HttpContent>, string) throws on null.")]
        public void FirstStartOrDefaultThrows()
        {
            Assert.ThrowsArgumentNull(() => { HttpContentCollectionExtensions.FirstStartOrDefault(null, "A"); }, "contents");

            IEnumerable<HttpContent> content = HttpContentCollectionExtensionsTests.CreateContent();
            Assert.ThrowsArgumentNull(() => { HttpContentCollectionExtensions.FirstStartOrDefault(content, null); }, "start");
            foreach (string empty in TestData.EmptyStrings)
            {
                Assert.ThrowsArgumentNull(() => { HttpContentCollectionExtensions.FirstStartOrDefault(content, empty); }, "start");
            }
        }

        [Fact]
        [Trait("Description", "FirstStartOrDefault(IEnumerable<HttpContent>, string) no match.")]
        public void FirstStartOrDefaultNoMatch()
        {
            IEnumerable<HttpContent> content = HttpContentCollectionExtensionsTests.CreateContent();
            Assert.Null(content.FirstStartOrDefault(noMatchContentID));
        }

        [Fact]
        [Trait("Description", "FirstStartOrDefault(IEnumerable<HttpContent>, string) match.")]
        public void FirstStartOrDefaultMatch()
        {
            IEnumerable<HttpContent> content = HttpContentCollectionExtensionsTests.CreateContent();
            Assert.NotNull(content.FirstStartOrDefault(matchContentID));
        }
    }
}