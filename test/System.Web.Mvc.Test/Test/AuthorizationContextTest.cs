using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Mvc.Test
{
    public class AuthorizationContextTest
    {
        [Fact]
        public void ConstructorThrowsIfActionDescriptorIsNull()
        {
            // Arrange
            ControllerContext controllerContext = new Mock<ControllerContext>().Object;
            ActionDescriptor actionDescriptor = null;

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { new AuthorizationContext(controllerContext, actionDescriptor); }, "actionDescriptor");
        }

        [Fact]
        public void PropertiesAreSetByConstructor()
        {
            // Arrange
            ControllerContext controllerContext = new Mock<ControllerContext>().Object;
            ActionDescriptor actionDescriptor = new Mock<ActionDescriptor>().Object;

            // Act
            AuthorizationContext authorizationContext = new AuthorizationContext(controllerContext, actionDescriptor);

            // Assert
            Assert.Equal(actionDescriptor, authorizationContext.ActionDescriptor);
        }
    }
}
