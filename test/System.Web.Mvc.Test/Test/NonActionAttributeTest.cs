using Xunit;

namespace System.Web.Mvc.Test
{
    public class NonActionAttributeTest
    {
        [Fact]
        public void InValidActionForRequestReturnsFalse()
        {
            // Arrange
            NonActionAttribute attr = new NonActionAttribute();

            // Act & Assert
            Assert.False(attr.IsValidForRequest(null, null));
        }
    }
}
