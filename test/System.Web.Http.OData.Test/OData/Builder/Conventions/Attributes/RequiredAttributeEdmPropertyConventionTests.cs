// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Builder.Conventions.Attributes
{
    public class RequiredAttributeEdmPropertyConventionTests
    {
        [Fact]
        public void Empty_Ctor_DoesnotThrow()
        {
            Assert.DoesNotThrow(() => new RequiredAttributeEdmPropertyConvention());
        }

        [Fact]
        public void Apply_SetsOptionalProperty()
        {
            // Arrange
            Mock<PropertyInfo> property = new Mock<PropertyInfo>();
            property.Setup(p => p.Name).Returns("Property");
            property.Setup(p => p.PropertyType).Returns(typeof(string));
            property.Setup(p => p.GetCustomAttributes(It.IsAny<bool>())).Returns(new[] { new RequiredAttribute() });

            Mock<StructuralPropertyConfiguration> structuralProperty = new Mock<StructuralPropertyConfiguration>(property.Object);
            Mock<IStructuralTypeConfiguration> structuralType = new Mock<IStructuralTypeConfiguration>();

            // Act
            new RequiredAttributeEdmPropertyConvention().Apply(structuralProperty.Object, structuralType.Object);

            // Assert
            Assert.False(structuralProperty.Object.OptionalProperty);
        }
    }
}
