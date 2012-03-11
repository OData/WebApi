using System.Web.Helpers;
using System.Web.TestUtil;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Mvc.Test
{
    public class ValidateAntiForgeryTokenAttributeTest
    {
        [Fact]
        public void OnAuthorization_ThrowsIfFilterContextIsNull()
        {
            // Arrange
            ValidateAntiForgeryTokenAttribute attribute = new ValidateAntiForgeryTokenAttribute();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { attribute.OnAuthorization(null); }, "filterContext");
        }

        [Fact]
        public void OnAuthorization_ForwardsAttributes()
        {
            // Arrange
            HttpContextBase context = new Mock<HttpContextBase>().Object;
            Mock<AuthorizationContext> authorizationContextMock = new Mock<AuthorizationContext>();
            authorizationContextMock.SetupGet(ac => ac.HttpContext).Returns(context);
            bool validateCalled = false;
            Action<HttpContextBase, string> validateMethod = (c, s) =>
            {
                Assert.Same(context, c);
                Assert.Equal("some salt", s);
                validateCalled = true;
            };
            ValidateAntiForgeryTokenAttribute attribute = new ValidateAntiForgeryTokenAttribute(validateMethod)
            {
                Salt = "some salt"
            };

            // Act
            attribute.OnAuthorization(authorizationContextMock.Object);

            // Assert
            Assert.True(validateCalled);
        }

        [Fact]
        public void SaltProperty()
        {
            // Arrange
            ValidateAntiForgeryTokenAttribute attribute = new ValidateAntiForgeryTokenAttribute();

            // Act & Assert
            MemberHelper.TestStringProperty(attribute, "Salt", String.Empty);
        }

        [Fact]
        public void ValidateThunk_DefaultsToAntiForgeryMethod()
        {
            // Arrange
            ValidateAntiForgeryTokenAttribute attribute = new ValidateAntiForgeryTokenAttribute();

            // Act & Assert
            Assert.Equal(AntiForgery.Validate, attribute.ValidateAction);
        }
    }
}
