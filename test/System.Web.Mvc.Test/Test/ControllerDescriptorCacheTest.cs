// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using Xunit;

namespace System.Web.Mvc.Test
{
    public class ControllerDescriptorCacheTest
    {
        [Fact]
        public void GetDescriptor()
        {
            // Arrange
            Type controllerType = typeof(object);
            ControllerDescriptorCache cache = new ControllerDescriptorCache();

            // Act
            ControllerDescriptor descriptor1 = cache.GetDescriptor(controllerType, () => new ReflectedControllerDescriptor(controllerType));
            ControllerDescriptor descriptor2 = cache.GetDescriptor(controllerType, () => new ReflectedControllerDescriptor(controllerType));

            // Assert
            Assert.Same(controllerType, descriptor1.ControllerType);
            Assert.Same(descriptor1, descriptor2);
        }
    }
}
