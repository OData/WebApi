using System.Security.Principal;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Helpers.Test
{
    public class AntiForgeryDataTest
    {
        [Fact]
        public void CopyConstructor()
        {
            // Arrange
            AntiForgeryData originalToken = new AntiForgeryData()
            {
                CreationDate = DateTime.Now,
                Salt = "some salt",
                Value = "some value"
            };

            // Act
            AntiForgeryData newToken = new AntiForgeryData(originalToken);

            // Assert
            Assert.Equal(originalToken.CreationDate, newToken.CreationDate);
            Assert.Equal(originalToken.Salt, newToken.Salt);
            Assert.Equal(originalToken.Value, newToken.Value);
        }

        [Fact]
        public void CopyConstructorThrowsIfTokenIsNull()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { new AntiForgeryData(null); }, "token");
        }

        [Fact]
        public void CreationDateProperty()
        {
            // Arrange
            AntiForgeryData token = new AntiForgeryData();

            // Act & Assert
            var now = DateTime.UtcNow;
            token.CreationDate = now;
            Assert.Equal(now, token.CreationDate);
        }

        [Fact]
        public void GetAntiForgeryTokenNameReturnsEncodedCookieNameIfAppPathIsNotEmpty()
        {
            // Arrange    
            // the string below (as UTF-8 bytes) base64-encodes to "Pz4/Pj8+Pz4/Pj8+Pz4/Pg=="
            string original = "?>?>?>?>?>?>?>?>";

            // Act
            string tokenName = AntiForgeryData.GetAntiForgeryTokenName(original);

            // Assert
            Assert.Equal("__RequestVerificationToken_Pz4-Pj8.Pz4-Pj8.Pz4-Pg__", tokenName);
        }

        [Fact]
        public void GetAntiForgeryTokenNameReturnsFieldNameIfAppPathIsNull()
        {
            // Act
            string tokenName = AntiForgeryData.GetAntiForgeryTokenName(null);

            // Assert
            Assert.Equal("__RequestVerificationToken", tokenName);
        }

        [Fact]
        public void GetUsername_ReturnsEmptyStringIfIdentityIsNull()
        {
            // Arrange
            Mock<IPrincipal> mockPrincipal = new Mock<IPrincipal>();
            mockPrincipal.Setup(o => o.Identity).Returns((IIdentity)null);

            // Act
            string username = AntiForgeryData.GetUsername(mockPrincipal.Object);

            // Assert
            Assert.Equal("", username);
        }

        [Fact]
        public void GetUsername_ReturnsEmptyStringIfPrincipalIsNull()
        {
            // Act
            string username = AntiForgeryData.GetUsername(null);

            // Assert
            Assert.Equal("", username);
        }

        [Fact]
        public void GetUsername_ReturnsEmptyStringIfUserNotAuthenticated()
        {
            // Arrange
            Mock<IPrincipal> mockPrincipal = new Mock<IPrincipal>();
            mockPrincipal.Setup(o => o.Identity.IsAuthenticated).Returns(false);
            mockPrincipal.Setup(o => o.Identity.Name).Returns("SampleName");

            // Act
            string username = AntiForgeryData.GetUsername(mockPrincipal.Object);

            // Assert
            Assert.Equal("", username);
        }

        [Fact]
        public void GetUsername_ReturnsUsernameIfUserIsAuthenticated()
        {
            // Arrange
            Mock<IPrincipal> mockPrincipal = new Mock<IPrincipal>();
            mockPrincipal.Setup(o => o.Identity.IsAuthenticated).Returns(true);
            mockPrincipal.Setup(o => o.Identity.Name).Returns("SampleName");

            // Act
            string username = AntiForgeryData.GetUsername(mockPrincipal.Object);

            // Assert
            Assert.Equal("SampleName", username);
        }

        [Fact]
        public void NewToken()
        {
            // Act
            AntiForgeryData token = AntiForgeryData.NewToken();

            // Assert
            int valueLength = Convert.FromBase64String(token.Value).Length;
            Assert.Equal(16, valueLength);
            Assert.NotEqual(default(DateTime), token.CreationDate);
        }

        [Fact]
        public void SaltProperty()
        {
            // Arrange
            AntiForgeryData token = new AntiForgeryData();

            // Act & Assert
            Assert.Equal(String.Empty, token.Salt);
            token.Salt = null;
            Assert.Equal(String.Empty, token.Salt);
            token.Salt = String.Empty;
            Assert.Equal(String.Empty, token.Salt);
        }

        [Fact]
        public void ValueProperty()
        {
            // Arrange
            AntiForgeryData token = new AntiForgeryData();

            // Act & Assert
            Assert.Equal(String.Empty, token.Value);
            token.Value = null;
            Assert.Equal(String.Empty, token.Value);
            token.Value = String.Empty;
            Assert.Equal(String.Empty, token.Value);
        }
    }
}
