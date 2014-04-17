// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.TestCommon;

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
                typeof(JQueryFormValueProviderFactory),
            };

            // Act
            Type[] actualTypes = ValueProviderFactories.Factories.Select(p => p.GetType()).ToArray();

            // Assert
            Assert.Equal(expectedTypes, actualTypes);
        }
    }
}
