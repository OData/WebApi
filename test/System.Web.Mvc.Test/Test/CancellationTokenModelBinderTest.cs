// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Threading;
using Xunit;

namespace System.Web.Mvc.Test
{
    public class CancellationTokenModelBinderTest
    {
        [Fact]
        public void BinderReturnsDefaultCancellationToken()
        {
            // Arrange
            CancellationTokenModelBinder binder = new CancellationTokenModelBinder();

            // Act
            object binderResult = binder.BindModel(controllerContext: null, bindingContext: null);

            // Assert
            Assert.Equal(default(CancellationToken), binderResult);
        }
    }
}
