//-----------------------------------------------------------------------------
// <copyright file="DefaultValueAttributeEdmPropertyConventionTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Builder.Conventions.Attributes;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Builder.Conventions.Attributes
{
    public class DefaultValueAttributeEdmPropertyConventionTests
    {
        enum TestEnum
        {
            Member1,
            Member2
        }

        [Fact]
        public void Empty_Ctor_DoesnotThrow()
        {
            ExceptionAssert.DoesNotThrow(() => new DefaultValueAttributeEdmPropertyConvention());
        }

        [Fact]
        public void Apply_SetsDefaultValueProperty()
        {
            // Arrange
            Mock<PropertyInfo> property = new Mock<PropertyInfo>();
            property.Setup(p => p.Name).Returns("Property");
            property.Setup(p => p.PropertyType).Returns(typeof(string));
            property.Setup(p => p.GetCustomAttributes(It.IsAny<bool>())).Returns(new[] { new DefaultValueAttribute("defaultValue") });

            Mock<StructuralTypeConfiguration> structuralType = new Mock<StructuralTypeConfiguration>();
            Mock<PrimitivePropertyConfiguration> structuralProperty = new Mock<PrimitivePropertyConfiguration>(property.Object, structuralType.Object);
            structuralProperty.Object.AddedExplicitly = false;

            // Act
            new DefaultValueAttributeEdmPropertyConvention().Apply(structuralProperty.Object, structuralType.Object, ODataConventionModelBuilderFactory.Create());

            // Assert
            Assert.Equal("defaultValue", structuralProperty.Object.DefaultValueString);
        }

        [Fact]
        public void DefaultValueAttributeEdmPropertyConvention_ConfiguresDefaultValuePropertyAsDefaultValue()
        {
            MockType type =
                new MockType("Entity")
                .Property(typeof(int), "ID")
                .Property(typeof(int), "Count", new DefaultValueAttribute(0))
                .Property(typeof(TestEnum), "Kind", new DefaultValueAttribute(TestEnum.Member2));

            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.AddEntityType(type);

            IEdmModel model = builder.GetEdmModel();
            IEdmEntityType entity = model.AssertHasEntityType(type);
            IEdmStructuralProperty property = entity.AssertHasPrimitiveProperty(model, "Count", EdmPrimitiveTypeKind.Int32, isNullable: false);
            Assert.Equal("0", property.DefaultValueString);
            IEdmStructuralProperty enumProperty = entity.AssertHasProperty<IEdmStructuralProperty>(model, "Kind", typeof(TestEnum), isNullable: false);
            Assert.Equal("Member2", enumProperty.DefaultValueString);
        }

        [Fact]
        public void DefaultValueAttributeEdmPropertyConvention_DoesnotOverwriteExistingConfiguration()
        {
            MockType type =
                new MockType("Entity")
                .Property(typeof(int), "ID")
                .Property(typeof(int), "Count", new DefaultValueAttribute("10"));

            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.AddEntityType(type).AddProperty(type.GetProperty("Count")).DefaultValueString="0";

            IEdmModel model = builder.GetEdmModel();
            IEdmEntityType entity = model.AssertHasEntityType(type);
            IEdmStructuralProperty property = entity.AssertHasPrimitiveProperty(model, "Count", EdmPrimitiveTypeKind.Int32, isNullable: false);
            Assert.Equal("0", property.DefaultValueString); 
        }
    }
}
