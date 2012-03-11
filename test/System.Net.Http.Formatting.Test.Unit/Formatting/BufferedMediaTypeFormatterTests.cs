using System.IO;
using System.Net.Http.Headers;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Net.Http.Formatting
{
    public class BufferedMediaTypeFormatterTests
    {
        private BufferedMediaTypeFormatter _formatter = new TestableBufferedMediaTypeFormatter();

        [Fact]
        public void WriteToStreamAsync_WhenTypeParameterIsNull_ThrowsException()
        {
            Assert.ThrowsArgumentNull(
                () => _formatter.WriteToStreamAsync(null, new object(), new MemoryStream(), null, null), "type");
        }

        [Fact]
        public void WriteToStreamAsync_WhenStreamParameterIsNull_ThrowsException()
        {
            Assert.ThrowsArgumentNull(
                () => _formatter.WriteToStreamAsync(typeof(object), new object(), null, null, null), "stream");
        }

        [Fact]
        public void ReadFromStreamAsync_WhenTypeParamterIsNull_ThrowsException()
        {
            Assert.ThrowsArgumentNull(() => _formatter.ReadFromStreamAsync(null, new MemoryStream(), null, null), "type");
        }

        [Fact]
        public void ReadFromStreamAsync_WhenStreamParamterIsNull_ThrowsException()
        {
            Assert.ThrowsArgumentNull(() => _formatter.ReadFromStreamAsync(typeof(object), null, null, null), "stream");
        }

        [Fact]
        public void BufferedWrite()
        {
            // Arrange. Specifically use the base class with async signatures. 
            MediaTypeFormatter formatter = new TestableBufferedMediaTypeFormatter();
            MemoryStream stream = new MemoryStream();

            // Act. Call the async signature.
            string dummyData = "ignored";
            formatter.WriteToStreamAsync(dummyData.GetType(), dummyData, stream, null, null).Wait();

            // Assert
            byte[] buffer = stream.GetBuffer();
            Assert.Equal(123, buffer[0]);
        }

        [Fact]
        public void BufferedRead()
        {
            // Arrange. Specifically use the base class with async signatures. 
            MediaTypeFormatter formatter = new TestableBufferedMediaTypeFormatter();
            MemoryStream stream = new MemoryStream();
            byte data = 45;
            stream.WriteByte(data);
            stream.Position = 0;

            // Act. Call the async signature.
            string dummyData = "ignored";
            object result = formatter.ReadFromStreamAsync(dummyData.GetType(), stream, null, null).Result;

            // Assert
            Assert.Equal(data, result);
        }

        class TestableBufferedMediaTypeFormatter : BufferedMediaTypeFormatter
        {
            public override bool CanReadType(Type type)
            {
                return true;
            }

            public override bool CanWriteType(Type type)
            {
                return true;
            }
            public override object OnReadFromStream(Type type, Stream stream, HttpContentHeaders contentHeaders, IFormatterLogger formatterLogger)
            {
                byte data = (byte)stream.ReadByte();
                return data;
            }

            public override void OnWriteToStream(Type type, object value, Stream stream, HttpContentHeaders contentHeaders, TransportContext transportContext)
            {
                stream.WriteByte(123);
            }
        }
    }
}
