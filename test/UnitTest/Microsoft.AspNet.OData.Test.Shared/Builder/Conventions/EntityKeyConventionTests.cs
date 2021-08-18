//-----------------------------------------------------------------------------
// <copyright file="EntityKeyConventionTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Builder.Conventions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Builder.Conventions
{
    public class EntityKeyConventionTests
    {
        [Theory]
        [InlineData("ID")]
        [InlineData("Id")]
        [InlineData("iD")]
        [InlineData("SampleEntityID")]
        public void Apply_Calls_HasKey_OnEdmType(string propertyName)
        {
            // Arrange
            Mock<EntityTypeConfiguration> mockEntityType = new Mock<EntityTypeConfiguration>();
            Mock<PropertyConfiguration> property = new Mock<PropertyConfiguration>(typeof(EntityKeyConventionTests_EntityType).GetProperty(propertyName), mockEntityType.Object);

            mockEntityType.Setup(e => e.Name).Returns("SampleEntity");
            mockEntityType.Setup(entityType => entityType.HasKey(typeof(EntityKeyConventionTests_EntityType).GetProperty(propertyName))).Returns(mockEntityType.Object).Verifiable();
            mockEntityType.Object.ExplicitProperties.Add(new MockPropertyInfo(), property.Object);

            // Act
            new EntityKeyConvention().Apply(mockEntityType.Object, null);

            // Assert
            mockEntityType.Verify();
        }

        [Fact]
        public void Apply_Calls_HasKey_ForEnumProperty_OnEdmType()
        {
            // Arrange
            Mock<EntityTypeConfiguration> mockEntityType = new Mock<EntityTypeConfiguration>();
            Mock<PropertyConfiguration> property =
                new Mock<PropertyConfiguration>(typeof(EntityKeyConventionTests_EntityType).GetProperty("ColorId"),
                    mockEntityType.Object);
            property.Setup(c => c.Kind).Returns(PropertyKind.Enum);

            mockEntityType.Setup(e => e.Name).Returns("Color");
            mockEntityType.Setup(
                entityType => entityType.HasKey(typeof(EntityKeyConventionTests_EntityType).GetProperty("ColorId")))
                .Returns(mockEntityType.Object)
                .Verifiable();

            mockEntityType.Object.ExplicitProperties.Add(new MockPropertyInfo(), property.Object);

            // Act
            new EntityKeyConvention().Apply(mockEntityType.Object, null);

            // Assert
            mockEntityType.Verify();
        }

        [Fact]
        public void EntityKeyConvention_FiguresOutTheKeyProperty()
        {
            MockType baseType =
                new MockType("BaseType")
                .Property<uint>("ID");

            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.AddEntityType(baseType);

            IEdmModel model = builder.GetEdmModel();

            IEdmEntityType entity = model.AssertHasEntityType(baseType);
            entity.AssertHasKey(model, "ID", EdmPrimitiveTypeKind.Int64);
        }

        [Fact]
        public void EntityKeyConvention_FiguresOutTheEnumKeyProperty()
        {
            // Arrange
            MockType baseType =
                new MockType("BaseType")
                .Property<Color>("ID");

            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.AddEntityType(baseType);

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmEntityType entity = model.AssertHasEntityType(baseType);

            IEdmStructuralProperty enumProperty = entity.AssertHasProperty<IEdmStructuralProperty>(model, "ID", typeof(Color), false);
            IEdmProperty enumKey = Assert.Single(entity.DeclaredKey);
            Assert.Same(enumProperty, enumKey);

            Assert.Equal(EdmTypeKind.Enum, enumKey.Type.TypeKind());
            Assert.Equal("Microsoft.AspNet.OData.Test.Builder.TestModels.Color", enumKey.Type.Definition.FullTypeName());
        }

        [Fact]
        public void EntityKeyConvention_DoesnotFigureOutKeyPropertyOnDerivedTypes()
        {
            MockType baseType =
                new MockType("BaseType")
                .Property<uint>("ID");

            MockType derivedType =
                new MockType("DerivedType")
                .Property<int>("DerivedTypeID")
                .BaseType(baseType);

            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.AddEntityType(derivedType).DerivesFrom(builder.AddEntityType(baseType));

            IEdmModel model = builder.GetEdmModel();

            IEdmEntityType baseEntity = model.AssertHasEntityType(baseType);
            baseEntity.AssertHasKey(model, "ID", EdmPrimitiveTypeKind.Int64);

            IEdmEntityType derivedEntity = model.AssertHasEntityType(derivedType);
            derivedEntity.AssertHasPrimitiveProperty(model, "DerivedTypeID", EdmPrimitiveTypeKind.Int32, isNullable: false);
        }

        [Fact]
        public void EntityKeyConvention_DoesnotFigureOutKeyPropertyIfIgnored()
        {
            MockType baseType =
                new MockType("BaseType")
                .Property(typeof(int), "ID", new NotMappedAttribute());

            ODataConventionModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataConventionModelBuilder>();
            builder.AddEntityType(baseType);

            IEdmModel model = builder.GetEdmModel();

            IEdmEntityType baseEntity = model.AssertHasEntityType(baseType);
            Assert.Empty(baseEntity.Properties());
            Assert.Empty(baseEntity.Key());
        }

        class EntityKeyConventionTests_EntityType
        {
            public string ID { get; set; }

            public string Id { get; set; }

            public string iD { get; set; }

            public string SampleEntityID { get; set; }

            public Color ColorId { get; set; }
        }
    }
}
