// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class EmptyModelValidatorProviderTest
    {
        [Fact]
        public void ReturnsNoValidators()
        {
            // Arrange
            EmptyModelValidatorProvider provider = new EmptyModelValidatorProvider();

            // Act
            IEnumerable<ModelValidator> result = provider.GetValidators(null, null);

            // Assert
            Assert.Empty(result);
        }
    }
}
