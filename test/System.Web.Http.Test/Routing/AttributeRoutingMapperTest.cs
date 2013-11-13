// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Web.Http.Controllers;
using System.Web.Http.Internal;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Routing
{
    public class AttributeRoutingMapperTest
    {
        [Fact]
        public void GetRoutePrefix_WithMultiRoutePrefix_ThrowsInvalidOperationException()
        {
            // Arrange
            var httpControllerDescriptor = new MultiRoutePrefixControllerDescripter();
            var typeMock = new Mock<Type>();
            typeMock.SetupGet(t => t.FullName).Returns("Namespace.TypeFullName");
            httpControllerDescriptor.ControllerType = typeMock.Object;

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => AttributeRoutingMapper.GetRoutePrefix(httpControllerDescriptor),
                "Only one route prefix attribute is supported. Remove extra attributes from the controller of type 'Namespace.TypeFullName'.");
        }

        [Fact]
        public void GetRoutePrefix_WithNullPrefix_ThrowsInvalidOperationException()
        {
            // Arrange
            var httpControllerDescriptor = new NullRoutePrefixControllerDescripter();
            var typeMock = new Mock<Type>();
            typeMock.SetupGet(t => t.FullName).Returns("Namespace.TypeFullName");
            httpControllerDescriptor.ControllerType = typeMock.Object;

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => AttributeRoutingMapper.GetRoutePrefix(httpControllerDescriptor),
                "The property 'prefix' from route prefix attribute on controller of type 'Namespace.TypeFullName' cannot be null.");
        }

        private class MultiRoutePrefixControllerDescripter : HttpControllerDescriptor
        {
            public override Collection<T> GetCustomAttributes<T>(bool inherit)
            {
                object[] attributes = new object[] { new ExtendedRoutePrefixAttribute(), new RoutePrefixAttribute("Prefix") };
                return new Collection<T>(TypeHelper.OfType<T>(attributes));
            }
        }

        private class NullRoutePrefixControllerDescripter : HttpControllerDescriptor
        {
            public override Collection<T> GetCustomAttributes<T>(bool inherit)
            {
                object[] attributes = new object[] { new ExtendedRoutePrefixAttribute() };
                return new Collection<T>(TypeHelper.OfType<T>(attributes));
            }
        }

        private class ExtendedRoutePrefixAttribute : RoutePrefixAttribute
        {
        }
    }
}
