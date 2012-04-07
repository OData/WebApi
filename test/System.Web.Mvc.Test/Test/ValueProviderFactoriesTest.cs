// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using Xunit;

namespace System.Web.Mvc.Test
{
    public class ValueProviderFactoriesTest
    {
        [Fact]
        public void CollectionDefaults()
        {
            // Arrange
            Type[] expectedTypes = new[]
            {
                typeof(ChildActionValueProviderFactory),
                typeof(FormValueProviderFactory),
                typeof(JsonValueProviderFactory),
                typeof(RouteDataValueProviderFactory),
                typeof(QueryStringValueProviderFactory),
                typeof(HttpFileCollectionValueProviderFactory),
            };

            // Act
            Type[] actualTypes = ValueProviderFactories.Factories.Select(p => p.GetType()).ToArray();

            // Assert
            Assert.Equal(expectedTypes, actualTypes);
        }
    }
}
