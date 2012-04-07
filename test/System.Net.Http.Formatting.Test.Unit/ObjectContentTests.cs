// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.TestCommon;
using Moq;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Net.Http
{
    public class ObjectContentTests
    {
        private readonly object _value = new object();
        private readonly MediaTypeFormatter _formatter = new TestableMediaTypeFormatter();

        [Fact]
        public void Constructor_WhenTypeArgumentIsNull_ThrowsEsxception()
        {
            Assert.ThrowsArgumentNull(() => new ObjectContent(null, _value, _formatter), "type");
            Assert.ThrowsArgumentNull(() => new ObjectContent(null, _value, _formatter, mediaType: "foo/bar"), "type");
        }

        [Fact]
        public void Constructor_WhenFormatterArgumentIsNull_ThrowsEsxception()
        {
            Assert.ThrowsArgumentNull(() => new ObjectContent(typeof(Object), _value, formatter: null), "formatter");
            Assert.ThrowsArgumentNull(() => new ObjectContent(typeof(Object), _value, formatter: null, mediaType: "foo/bar"), "formatter");
        }

        [Fact]
        public void Constructor_WhenValueIsNullAndTypeIsNotCompatible_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                new ObjectContent(typeof(int), null, new JsonMediaTypeFormatter());
            }, "The 'ObjectContent' type cannot accept a null value for the value type 'Int32'.");
        }

        [Fact]
        public void Constructor_WhenValueIsNotNullButTypeDoesNotMatch_ThrowsException()
        {
            Assert.ThrowsArgument(() =>
            {
                new ObjectContent(typeof(IList<string>), new Dictionary<string, string>(), new JsonMediaTypeFormatter());
            }, "value", "An object of type 'Dictionary`2' cannot be used with a type parameter of 'IList`1'.");
        }

        [Fact]
        public void Constructor_WhenValueIsNotSupportedByFormatter_ThrowsException()
        {
            Mock<MediaTypeFormatter> formatterMock = new Mock<MediaTypeFormatter>();
            formatterMock.Setup(f => f.CanWriteType(typeof(List<string>))).Returns(false).Verifiable();

            Assert.Throws<InvalidOperationException>(() =>
            {
                new ObjectContent(typeof(List<string>), new List<string>(), formatterMock.Object);
            }, "The configured formatter 'Castle.Proxies.MediaTypeFormatterProxy' cannot write an object of type 'List`1'.");

            formatterMock.Verify();
        }

        [Fact]
        public void Constructor_SetsFormatterProperty()
        {
            var content = new ObjectContent(typeof(object), _value, _formatter, mediaType: null);

            Assert.Same(_formatter, content.Formatter);
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

        [Theory]
        [PropertyData("ValidValueTypePairs")]
        public void Constructor_WhenValueAndTypeAreCompatible_SetsValue(Type type, object value)
        {
            var oc = new ObjectContent(type, value, new JsonMediaTypeFormatter());

            Assert.Same(value, oc.Value);
            Assert.Equal(type, oc.ObjectType);
        }

        public static TheoryDataSet<Type, object> ValidValueTypePairs
        {
            get
            {
                return new TheoryDataSet<Type, object>
                {
                    { typeof(Nullable<int>), null },
                    { typeof(string), null },
                    { typeof(int), 42 },
                    //{ typeof(int), (short)42 }, TODO should this work?
                    { typeof(object), "abc" },
                    { typeof(string), "abc" },
                    { typeof(IList<string>), new List<string>() },
                };
            }
        }

        [Fact]
        public void SerializeToStreamAsync_CallsUnderlyingFormatter()
        {
            var stream = Stream.Null;
            var context = new Mock<TransportContext>().Object;
            var formatterMock = new Mock<TestableMediaTypeFormatter> { CallBase = true };
            var oc = new TestableObjectContent(typeof(string), "abc", formatterMock.Object);
            var task = new Task(() => { });
            formatterMock.Setup(f => f.WriteToStreamAsync(typeof(string), "abc", stream, oc.Headers, context))
                .Returns(task).Verifiable();

            var result = oc.CallSerializeToStreamAsync(stream, context);

            Assert.Same(task, result);
            formatterMock.VerifyAll();
        }

        [Fact]
        public void TryComputeLength_ReturnsFalseAnd0()
        {
            var oc = new TestableObjectContent(typeof(string), null, _formatter);
            long length;

            var result = oc.CallTryComputeLength(out length);

            Assert.False(result);
            Assert.Equal(-1, length);
        }

        [Fact]
        public void Value_WhenValueIsNotSupportedByFormatter_ThrowsException()
        {
            Mock<MediaTypeFormatter> formatterMock = new Mock<MediaTypeFormatter>();
            formatterMock.Setup(f => f.CanWriteType(typeof(string))).Returns(true);
            formatterMock.Setup(f => f.CanWriteType(typeof(List<string>))).Returns(false).Verifiable();
            var content = new ObjectContent(typeof(object), "", formatterMock.Object);

            Assert.Throws<InvalidOperationException>(() =>
            {
                content.Value = new List<string>();
            }, "The configured formatter 'Castle.Proxies.MediaTypeFormatterProxy' cannot write an object of type 'List`1'.");

            formatterMock.Verify();
        }

        public class TestableObjectContent : ObjectContent
        {
            public TestableObjectContent(Type type, object value, MediaTypeFormatter formatter)
                : base(type, value, formatter)
            {
            }

            public bool CallTryComputeLength(out long length)
            {
                return TryComputeLength(out length);
            }

            public Task CallSerializeToStreamAsync(Stream stream, TransportContext context)
            {
                return SerializeToStreamAsync(stream, context);
            }
        }

        public class TestableMediaTypeFormatter : MediaTypeFormatter
        {
            public TestableMediaTypeFormatter()
            {
                SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));
            }

            public override bool CanReadType(Type type)
            {
                return true;
            }

            public override bool CanWriteType(Type type)
            {
                return true;
            }

            public override Task WriteToStreamAsync(Type type, object value, Stream stream, HttpContentHeaders contentHeaders, TransportContext transportContext)
            {
                throw new NotImplementedException();
            }
        }
    }
}
