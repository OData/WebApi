// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

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
