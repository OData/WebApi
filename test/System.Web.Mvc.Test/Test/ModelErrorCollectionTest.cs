// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class ModelErrorCollectionTest
    {
        [Fact]
        public void AddWithExceptionArgument()
        {
            // Arrange
            ModelErrorCollection collection = new ModelErrorCollection();
            Exception ex = new Exception("some message");

            // Act
            collection.Add(ex);

            // Assert
            ModelError modelError = Assert.Single(collection);
            Assert.Same(ex, modelError.Exception);
        }

        [Fact]
        public void AddWithStringArgument()
        {
            // Arrange
            ModelErrorCollection collection = new ModelErrorCollection();

            // Act
            collection.Add("some message");

            // Assert
            ModelError modelError = Assert.Single(collection);
            Assert.Equal("some message", modelError.ErrorMessage);
            Assert.Null(modelError.Exception);
        }
    }
}
