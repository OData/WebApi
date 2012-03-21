using System.IO;
using System.Net.Http.Formatting.DataSets;
using System.Net.Http.Headers;
using Microsoft.TestCommon;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Net.Http.Formatting
{
    public class FormUrlEncodedMediaTypeFormatterTests
    {
        private const int MinBufferSize = 256;
        private const int DefaultBufferSize = 32 * 1024;

        [Fact]
        [Trait("Description", "FormUrlEncodedMediaTypeFormatter is public, concrete, and unsealed.")]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties(typeof(FormUrlEncodedMediaTypeFormatter), TypeAssert.TypeProperties.IsPublicVisibleClass);
        }

        [Theory]
        [TestDataSet(typeof(HttpUnitTestDataSets), "StandardFormUrlEncodedMediaTypes")]
        [Trait("Description", "FormUrlEncodedMediaTypeFormatter() constructor sets standard form URL encoded media types in SupportedMediaTypes.")]
        public void Constructor(MediaTypeHeaderValue mediaType)
        {
            FormUrlEncodedMediaTypeFormatter formatter = new FormUrlEncodedMediaTypeFormatter();
            Assert.True(formatter.SupportedMediaTypes.Contains(mediaType), String.Format("SupportedMediaTypes should have included {0}.", mediaType.ToString()));
        }

        [Fact]
        [Trait("Description", "DefaultMediaType property returns application/x-www-form-urlencoded.")]
        public void DefaultMediaTypeReturnsApplicationJson()
        {
            MediaTypeHeaderValue mediaType = FormUrlEncodedMediaTypeFormatter.DefaultMediaType;
            Assert.NotNull(mediaType);
            Assert.Equal("application/x-www-form-urlencoded", mediaType.MediaType);
        }

        [Fact]
        public void ReadBufferSize_RoundTrips()
        {
            Assert.Reflection.IntegerProperty(
                new FormUrlEncodedMediaTypeFormatter(),
                c => c.ReadBufferSize,
                expectedDefaultValue: 32 * 1024,
                minLegalValue: 256,
                illegalLowerValue: 255,
                maxLegalValue: null,
                illegalUpperValue: null,
                roundTripTestValue: 1024);
        }

        [Fact]
        [Trait("Description", "CanReadType() throws on null.")]
        public void CanReadTypeThrowsOnNull()
        {
            TestFormUrlEncodedMediaTypeFormatter formatter = new TestFormUrlEncodedMediaTypeFormatter();
            Assert.ThrowsArgumentNull(() => { formatter.CanReadType(null); }, "type");
        }

        [Theory]
        [InlineData(typeof(FormDataCollection))]
        [InlineData(typeof(JToken))]
        public void CanReadTypeTrue(Type type)
        {
            TestFormUrlEncodedMediaTypeFormatter formatter = new TestFormUrlEncodedMediaTypeFormatter();

            Assert.True(formatter.CanReadType(type));
        }


        [Theory]
        [TestDataSet(typeof(CommonUnitTestDataSets), "RepresentativeValueAndRefTypeTestDataCollection")]
        [Trait("Description", "CanReadType(Type) returns false.")]
        public void CanReadTypeReturnsFalse(Type variationType, object testData)
        {
            TestFormUrlEncodedMediaTypeFormatter formatter = new TestFormUrlEncodedMediaTypeFormatter();

            Assert.False(formatter.CanReadType(variationType));

            // Ask a 2nd time to probe whether the cached result is treated the same
            Assert.False(formatter.CanReadType(variationType));
        }


        [Fact]
        [Trait("Description", "CanWriteType(Type) throws on null.")]
        public void CanWriteTypeThrowsOnNull()
        {
            TestFormUrlEncodedMediaTypeFormatter formatter = new TestFormUrlEncodedMediaTypeFormatter();
            Assert.ThrowsArgumentNull(() => { formatter.CanWriteType(null); }, "type");
        }

        [Theory]
        [TestDataSet(typeof(CommonUnitTestDataSets), "RepresentativeValueAndRefTypeTestDataCollection")]
        [Trait("Description", "CanWriteType() returns false.")]
        public void CanWriteTypeReturnsFalse(Type variationType, object testData)
        {
            TestFormUrlEncodedMediaTypeFormatter formatter = new TestFormUrlEncodedMediaTypeFormatter();

            Assert.False(formatter.CanWriteType(variationType), "formatter should have returned false.");

            // Ask a 2nd time to probe whether the cached result is treated the same
            Assert.False(formatter.CanWriteType(variationType), "formatter should have returned false on 2nd try as well.");
        }

        [Fact]
        [Trait("Description", "ReadFromStreamAsync() throws on null.")]
        public void ReadFromStreamThrowsOnNull()
        {
            TestFormUrlEncodedMediaTypeFormatter formatter = new TestFormUrlEncodedMediaTypeFormatter();
            Assert.ThrowsArgumentNull(() => { formatter.ReadFromStreamAsync(null, Stream.Null, null, null); }, "type");
            Assert.ThrowsArgumentNull(() => { formatter.ReadFromStreamAsync(typeof(object), null, null, null); }, "stream");
        }

        [Fact]
        [Trait("Description", "WriteToStreamAsync() throws not implemented.")]
        public void WriteToStreamAsyncThrowsNotImplemented()
        {
            FormUrlEncodedMediaTypeFormatter formatter = new FormUrlEncodedMediaTypeFormatter();
            Assert.Throws<NotSupportedException>(
                () => formatter.WriteToStreamAsync(typeof(object), new object(), Stream.Null, null, null),
                "The media type formatter of type 'FormUrlEncodedMediaTypeFormatter' does not support writing because it does not implement the WriteToStreamAsync method.");
        }

        public class TestFormUrlEncodedMediaTypeFormatter : FormUrlEncodedMediaTypeFormatter
        {
            public new bool CanReadType(Type type)
            {
                return base.CanReadType(type);
            }

            public new bool CanWriteType(Type type)
            {
                return base.CanWriteType(type);
            }
        }
    }
}
