using System.Web.Mvc;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Helpers.Test
{
    public class AntiForgeryDataSerializerTest
    {
        [Fact]
        public void GuardClauses()
        {
            // Arrange
            AntiForgeryDataSerializer serializer = new AntiForgeryDataSerializer();

            // Act & assert
            Assert.ThrowsArgumentNull(
                () => serializer.Serialize(null),
                "token"
                );
            Assert.ThrowsArgumentNullOrEmptyString(
                () => serializer.Deserialize(null),
                "serializedToken"
                );
            Assert.ThrowsArgumentNullOrEmptyString(
                () => serializer.Deserialize(String.Empty),
                "serializedToken"
                );
            Assert.Throws<HttpAntiForgeryException>(
                () => serializer.Deserialize("Corrupted Base-64 Value"),
                "A required anti-forgery token was not supplied or was invalid."
                );
        }

        [Fact]
        public void DeserializationExceptionDoesNotContainInnerException()
        {
            // Arrange
            AntiForgeryDataSerializer serializer = new AntiForgeryDataSerializer();

            // Act & assert
            HttpAntiForgeryException exception = null;
            try
            {
                serializer.Deserialize("Can't deserialize this.");
            }
            catch (HttpAntiForgeryException ex)
            {
                exception = ex;
            }

            Assert.NotNull(exception);
            Assert.Null(exception.InnerException);
        }

        [Fact]
        public void CanRoundTripData()
        {
            // Arrange
            AntiForgeryDataSerializer serializer = new AntiForgeryDataSerializer
            {
                Decoder = value => Convert.FromBase64String(value),
                Encoder = bytes => Convert.ToBase64String(bytes),
            };
            AntiForgeryData input = new AntiForgeryData
            {
                Salt = "The Salt",
                Username = "The Username",
                Value = "The Value",
                CreationDate = DateTime.Now,
            };

            // Act
            AntiForgeryData output = serializer.Deserialize(serializer.Serialize(input));

            // Assert
            Assert.NotNull(output);
            Assert.Equal(input.Salt, output.Salt);
            Assert.Equal(input.Username, output.Username);
            Assert.Equal(input.Value, output.Value);
            Assert.Equal(input.CreationDate, output.CreationDate);
        }

        [Fact]
        public void HexDigitConvertsIntegersToHexCharsCorrectly()
        {
            for (int i = 0; i < 0x10; i++)
            {
                Assert.Equal(i.ToString("X")[0], AntiForgeryDataSerializer.HexDigit(i));
            }
        }

        [Fact]
        public void HexValueConvertsCharValuesToIntegersCorrectly()
        {
            for (int i = 0; i < 0x10; i++)
            {
                var hexChar = i.ToString("X")[0];
                Assert.Equal(i, AntiForgeryDataSerializer.HexValue(hexChar));
            }
        }
    }
}
