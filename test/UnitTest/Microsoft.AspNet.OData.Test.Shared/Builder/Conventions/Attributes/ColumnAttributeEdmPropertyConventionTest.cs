//-----------------------------------------------------------------------------
// <copyright file="ColumnAttributeEdmPropertyConventionTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.DataAnnotations.Schema;
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
    public class ColumnAttributeEdmPropertyConventionTest
    {
        [Fact]
        public void Empty_Ctor_DoesnotThrow()
        {
            ExceptionAssert.DoesNotThrow(() => new ColumnAttributeEdmPropertyConvention());
        }

        [Theory]
        [InlineData("date", true)]
        [InlineData("DaTe", true)]
        [InlineData("edm.date", false)]
        [InlineData("eDm.daTe", false)]
        [InlineData("any", false)]
        public void Apply_SetsDateTimeProperty_AsEdmDate(string typeName, bool expect)
        {
            // Arrange
            MockType type = new MockType("Customer")
                .Property(typeof(DateTime), "Birthday", new[] {new ColumnAttribute {TypeName = typeName}});

            Mock<StructuralTypeConfiguration> structuralType = new Mock<StructuralTypeConfiguration>();
            structuralType.Setup(t => t.ClrType).Returns(type);

            PropertyInfo property = type.GetProperty("Birthday");
            Mock<PrimitivePropertyConfiguration> primitiveProperty = new Mock<PrimitivePropertyConfiguration>(property,
                structuralType.Object);
            primitiveProperty.Setup(p => p.RelatedClrType).Returns(typeof(DateTime));
            primitiveProperty.Object.AddedExplicitly = false;

            // Act
            new ColumnAttributeEdmPropertyConvention().Apply(primitiveProperty.Object, structuralType.Object, ODataConventionModelBuilderFactory.Create());

            // Assert
            if (expect)
            {
                Assert.NotNull(primitiveProperty.Object.TargetEdmTypeKind);
                Assert.Equal(EdmPrimitiveTypeKind.Date, primitiveProperty.Object.TargetEdmTypeKind);
            }
            else
            {
                Assert.Null(primitiveProperty.Object.TargetEdmTypeKind);
            }
        }

        [Theory]
        [InlineData("time", true)]
        [InlineData("tIme", true)]
        [InlineData("edm.timeofday", false)]
        [InlineData("eDm.TimeOfDay", false)]
        [InlineData("any", false)]
        public void Apply_SetsTimeSpanProperty_AsEdmTimeOfDay(string typeName, bool expect)
        {
            // Arrange
            MockType type = new MockType("Customer")
                .Property(typeof(TimeSpan), "CreatedTime", new[] {new ColumnAttribute {TypeName = typeName}});

            Mock<StructuralTypeConfiguration> structuralType = new Mock<StructuralTypeConfiguration>();
            structuralType.Setup(t => t.ClrType).Returns(type);

            PropertyInfo property = type.GetProperty("CreatedTime");
            Mock<PrimitivePropertyConfiguration> primitiveProperty = new Mock<PrimitivePropertyConfiguration>(property, structuralType.Object);
            primitiveProperty.Setup(p => p.RelatedClrType).Returns(typeof(TimeSpan));
            primitiveProperty.Object.AddedExplicitly = false;

            // Act
            new ColumnAttributeEdmPropertyConvention().Apply(primitiveProperty.Object, structuralType.Object,
                ODataConventionModelBuilderFactory.Create());

            // Assert
            if (expect)
            {
                Assert.NotNull(primitiveProperty.Object.TargetEdmTypeKind);
                Assert.Equal(EdmPrimitiveTypeKind.TimeOfDay, primitiveProperty.Object.TargetEdmTypeKind);
            }
            else
            {
                Assert.Null(primitiveProperty.Object.TargetEdmTypeKind);
            }
        }
    }
}
