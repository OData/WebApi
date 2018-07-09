// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Extensions
{
    public class ODataApplicationBuilderExtensionsTest
    {
        [Fact]
        public void UseOData_ThrowsInvalidOperationException_IfODataServiceIsNotRegistered()
        {
            // Arrange
            var applicationBuilderMock = new Mock<IApplicationBuilder>();
            applicationBuilderMock.Setup(s => s.ApplicationServices).Returns(Mock.Of<IServiceProvider>());
            var edmModel = new Mock<IEdmModel>().Object;

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => applicationBuilderMock.Object.UseOData(edmModel));

            Assert.Equal(String.Format(SRResources.MissingODataServices, nameof(IPerRouteContainer)), exception.Message);
        }

        [Fact]
        public void UseODataWithNameAndPrefix_ThrowsInvalidOperationException_IfODataServiceIsNotRegistered()
        {
            // Arrange
            var applicationBuilderMock = new Mock<IApplicationBuilder>();
            applicationBuilderMock.Setup(s => s.ApplicationServices).Returns(Mock.Of<IServiceProvider>());
            var edmModel = new Mock<IEdmModel>().Object;

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(
                () => applicationBuilderMock.Object.UseOData("odata", "odata", edmModel));

            Assert.Equal(String.Format(SRResources.MissingODataServices, nameof(IPerRouteContainer)), exception.Message);
        }
    }
}
