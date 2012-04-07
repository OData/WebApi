// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

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
