// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http.Formatting;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Net.Http
{
    public class ObjectContentOfTTests
    {
        [Fact]
        public void Constructor_WhenFormatterParameterIsNull_Throws()
        {
            Assert.ThrowsArgumentNull(() => new ObjectContent<string>("", formatter: null), "formatter");
            Assert.ThrowsArgumentNull(() => new ObjectContent<string>("", formatter: null, mediaType: "foo/bar"), "formatter");
        }

        [Fact]
        public void Constructor_SetsFormatterProperty()
        {
            var formatter = new Mock<MediaTypeFormatter>().Object;

            var content = new ObjectContent<string>(null, formatter, mediaType: null);

            Assert.Same(formatter, content.Formatter);
        }

        [Fact]
        public void Constructor_CallsFormattersGetDefaultContentHeadersMethod()
        {
            var formatterMock = new Mock<MediaTypeFormatter>();
            formatterMock.Setup(f => f.CanWriteType(typeof(String))).Returns(true);

            var content = new ObjectContent(typeof(string), "", formatterMock.Object, "foo/bar");

            formatterMock.Verify(f => f.SetDefaultContentHeaders(typeof(string), content.Headers, "foo/bar"),
                                 Times.Once());
        }
    }
}
