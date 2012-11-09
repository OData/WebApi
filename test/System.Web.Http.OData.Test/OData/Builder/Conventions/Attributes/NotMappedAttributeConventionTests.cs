// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Builder.Conventions.Attributes
{
    public class NotMappedAttributeConventionTests
    {
        [Fact]
        public void Empty_Ctor_DoesnotThrow()
        {
            Assert.DoesNotThrow(() => new NotMappedAttributeConvention());
        }

        [Fact]
        public void Apply_Calls_RemovesProperty()
        {
            // Arrange
            Mock<PropertyInfo> property = new Mock<PropertyInfo>();
            property.Setup(p => p.Name).Returns("Property");
            property.Setup(p => p.PropertyType).Returns(typeof(int));
            property.Setup(p => p.GetCustomAttributes(It.IsAny<bool>())).Returns(new[] { new NotMappedAttribute() });

            Mock<StructuralTypeConfiguration> structuralType = new Mock<StructuralTypeConfiguration>(MockBehavior.Strict);
            Mock<PropertyConfiguration> primitiveProperty = new Mock<PropertyConfiguration>(property.Object, structuralType.Object);
            structuralType.Setup(e => e.RemoveProperty(property.Object)).Verifiable();

            // Act
            new NotMappedAttributeConvention().Apply(primitiveProperty.Object, structuralType.Object);

            // Assert
            structuralType.Verify();
        }
    }
}
