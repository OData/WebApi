// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.TestCommon;

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
