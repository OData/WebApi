// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Data.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Builder.Conventions
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

            var mockModelBuilder = new Mock<ODataModelBuilder>(MockBehavior.Strict);

            // Act
            new EntityKeyConvention().Apply(mockEntityType.Object, mockModelBuilder.Object);

            // Assert
            mockEntityType.Verify();
        }

        [Fact]
        public void EntityKeyConvention_FiguresOutTheKeyProperty()
        {
            MockType baseType =
                new MockType("BaseType")
                .Property<uint>("ID");

            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.AddEntity(baseType);

            IEdmModel model = builder.GetEdmModel();

            IEdmEntityType entity = model.AssertHasEntityType(baseType);
            entity.AssertHasKey(model, "ID", EdmPrimitiveTypeKind.Int64);
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

            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.AddEntity(derivedType).DerivesFrom(builder.AddEntity(baseType));

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

            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.AddEntity(baseType);

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
        }
    }
}
