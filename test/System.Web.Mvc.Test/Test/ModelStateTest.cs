using Xunit;

namespace System.Web.Mvc.Test
{
    public class ModelStateTest
    {
        [Fact]
        public void ErrorsProperty()
        {
            // Arrange
            ModelState modelState = new ModelState();

            // Act & Assert
            Assert.NotNull(modelState.Errors);
        }
    }
}
