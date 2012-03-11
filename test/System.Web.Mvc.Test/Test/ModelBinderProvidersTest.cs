using System.Linq;
using Xunit;

namespace System.Web.Mvc.Test
{
    public class ModelBinderProvidersTest
    {
        [Fact]
        public void CollectionDefaults()
        {
            // Act
            Type[] actualTypes = ModelBinderProviders.BinderProviders.Select(b => b.GetType()).ToArray();

            // Assert
            Assert.Equal(Enumerable.Empty<Type>(), actualTypes);
        }
    }
}
