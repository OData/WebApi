using System.Linq;
using Xunit;

namespace System.Web.Mvc.Test
{
    public class ModelValidatorProvidersTest
    {
        [Fact]
        public void CollectionDefaults()
        {
            // Arrange
            Type[] expectedTypes = new Type[]
            {
                typeof(DataAnnotationsModelValidatorProvider),
                typeof(DataErrorInfoModelValidatorProvider),
                typeof(ClientDataTypeModelValidatorProvider)
            };

            // Act
            Type[] actualTypes = ModelValidatorProviders.Providers.Select(p => p.GetType()).ToArray();

            // Assert
            Assert.Equal(expectedTypes, actualTypes);
        }
    }
}
