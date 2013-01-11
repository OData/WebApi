// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;
using Moq;

namespace System.Web.Http.OData
{
    public sealed class MockPropertyInfo : Mock<PropertyInfo>
    {
        private readonly Mock<MethodInfo> _mockGetMethod = new Mock<MethodInfo>();
        private readonly Mock<MethodInfo> _mockSetMethod = new Mock<MethodInfo>();

        public static implicit operator PropertyInfo(MockPropertyInfo mockPropertyInfo)
        {
            return mockPropertyInfo.Object;
        }

        public MockPropertyInfo()
            : this(typeof(object), "P")
        {
        }

        public MockPropertyInfo(Type propertyType, string propertyName)
        {
            SetupGet(p => p.DeclaringType).Returns(typeof(object));
            SetupGet(p => p.ReflectedType).Returns(typeof(object));
            SetupGet(p => p.Name).Returns(propertyName);
            SetupGet(p => p.PropertyType).Returns(propertyType);
            SetupGet(p => p.CanRead).Returns(true);
            SetupGet(p => p.CanWrite).Returns(true);
            Setup(p => p.GetGetMethod(It.IsAny<bool>())).Returns(_mockGetMethod.Object);
            Setup(p => p.GetSetMethod(It.IsAny<bool>())).Returns(_mockSetMethod.Object);
            Setup(p => p.Equals(It.IsAny<object>())).Returns<PropertyInfo>(p => ReferenceEquals(Object, p));

            _mockGetMethod.SetupGet(m => m.Attributes).Returns(MethodAttributes.Public);
        }

        public MockPropertyInfo Abstract()
        {
            _mockGetMethod.SetupGet(m => m.Attributes)
                .Returns(_mockGetMethod.Object.Attributes | MethodAttributes.Abstract);

            return this;
        }
    }
}
