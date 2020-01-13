// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Formatter;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OData.Edm.Vocabularies.V1;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Builder.Conventions
{
    public class ODataConventionModelBuilderTests
    {
        private const int _totalExpectedSchemaTypesForVehiclesModel = 10;

        [Fact]
        public void Ctor_ThrowsForNullConfiguration()
        {
#if NETCORE
            ExceptionAssert.ThrowsArgumentNull(
                () => new ODataConventionModelBuilder(provider: null),
                "provider");
#else
            ExceptionAssert.ThrowsArgumentNull(
                () => new ODataConventionModelBuilder(configuration: null),
                "configuration");
#endif 
        }

        [Fact]
        public void Ignore_Should_AddToListOfIgnoredTypes()
        {
            var builder = ODataConventionModelBuilderFactory.Create();
            builder.Ignore(typeof(object));

            Assert.True(builder.IsIgnoredType(typeof(object)));
        }

        [Fact]
        public void IgnoreOfT_Should_AddToListOfIgnoredTypes()
        {
            var builder = ODataConventionModelBuilderFactory.Create();
            builder.Ignore<object>();

            Assert.True(builder.IsIgnoredType(typeof(object)));
        }

        [Fact]
        public void CanCallIgnore_MultipleTimes_WithDuplicates()
        {
            var builder = ODataConventionModelBuilderFactory.Create();
            builder.Ignore<object>();
            builder.Ignore<object>();
            builder.Ignore(typeof(object), typeof(object), typeof(object));

            Assert.True(builder.IsIgnoredType(typeof(object)));
        }

        [Fact]
        public void DiscoverInheritanceRelationships_PatchesBaseType()
        {
            var mockType1 = new MockType("Foo");
            var mockType2 = new MockType("Bar").BaseType(mockType1);
            var configuration = RoutingConfigurationFactory.CreateWithTypes(mockType1, mockType2);
            var builder = ODataConventionModelBuilderFactory.Create(configuration);

            var entity1 = builder.AddEntityType(mockType1);
            var entity2 = builder.AddEntityType(mockType2);

            builder.DiscoverInheritanceRelationships();

            Assert.Equal(entity1, entity2.BaseType);
        }

        [Fact]
        public void DiscoverInheritanceRelationships_PatchesBaseType_EvenIfTheyAreSeperated()
        {
            var mockType1 = new MockType("Foo");
            var mockType2 = new MockType("Bar").BaseType(mockType1);
            var mockType3 = new MockType("ThirdLevel").BaseType(mockType2);

            var configuration = RoutingConfigurationFactory.CreateWithTypes(mockType1, mockType2, mockType3);
            var builder = ODataConventionModelBuilderFactory.Create(configuration);

            var entity1 = builder.AddEntityType(mockType1);
            var entity3 = builder.AddEntityType(mockType3);

            builder.DiscoverInheritanceRelationships();

            Assert.Equal(entity1, entity3.BaseType);
        }

        [Fact]
        public void RemoveBaseTypeProperties_RemovesAllBaseTypePropertiesFromDerivedTypes()
        {
            var mockType1 = new MockType("Foo").Property<int>("P1");
            var mockType2 = new MockType("Bar").BaseType(mockType1).Property<int>("P1").Property<int>("P2");
            var mockType3 = new MockType("ThirdLevel").BaseType(mockType2).Property<int>("P1").Property<int>("P2");

            var configuration = RoutingConfigurationFactory.CreateWithTypes(mockType1, mockType2, mockType3);
            var builder = ODataConventionModelBuilderFactory.Create(configuration);

            var entity1 = builder.AddEntityType(mockType1);
            entity1.AddProperty(mockType1.GetProperty("P1"));

            var entity2 = builder.AddEntityType(mockType2).DerivesFrom(entity1);
            entity2.AddProperty(mockType2.GetProperty("P2"));

            var entity3 = builder.AddEntityType(mockType3);
            entity3.AddProperty(mockType3.GetProperty("P1"));
            entity3.AddProperty(mockType3.GetProperty("P2"));

            builder.RemoveBaseTypeProperties(entity3, entity2);

            Assert.Empty(entity3.Properties);
        }

        [Fact]
        public void MapDerivedTypes_BringsAllDerivedTypes_InTheAssembly()
        {
            var mockType1 = new MockType("BaseLevel");
            var mockType2 = new MockType("Foo").BaseType(mockType1);
            var mockType3 = new MockType("Fo").BaseType(mockType2);
            var mockType4 = new MockType("Bar").BaseType(mockType1);

            var configuration = RoutingConfigurationFactory.CreateWithTypes(mockType1, mockType2, mockType3, mockType4);
            var builder = ODataConventionModelBuilderFactory.Create(configuration);

            var entity1 = builder.AddEntityType(mockType1);
            builder.MapDerivedTypes(entity1);

            Assert.Equal(
                new[] { "BaseLevel", "Foo", "Fo", "Bar" }.OrderBy(name => name),
                builder.StructuralTypes.Select(t => t.Name).OrderBy(name => name));
        }

        [Fact]
        public void ModelBuilder_Products()
        {
            // Arrange
            var modelBuilder = ODataConventionModelBuilderFactory.Create();
            modelBuilder.EntitySet<Product>("Products");
            modelBuilder.Singleton<Product>("Book"); // singleton

            // Act
            var model = modelBuilder.GetEdmModel();

            // Assert
            Assert.Equal(3, model.SchemaElements.OfType<IEdmSchemaType>().Count());

            var product = model.AssertHasEntitySet(entitySetName: "Products", mappedEntityClrType: typeof(Product));
            Assert.Equal(6, product.StructuralProperties().Count());
            Assert.Single(product.NavigationProperties());
            product.AssertHasKey(model, "ID", EdmPrimitiveTypeKind.Int32);
            product.AssertHasPrimitiveProperty(model, "ID", EdmPrimitiveTypeKind.Int32, isNullable: false);
            product.AssertHasPrimitiveProperty(model, "Name", EdmPrimitiveTypeKind.String, isNullable: true);
            product.AssertHasPrimitiveProperty(model, "ReleaseDate", EdmPrimitiveTypeKind.DateTimeOffset, isNullable: true);
            product.AssertHasPrimitiveProperty(model, "PublishDate", EdmPrimitiveTypeKind.Date, isNullable: false);
            product.AssertHasPrimitiveProperty(model, "ShowTime", EdmPrimitiveTypeKind.TimeOfDay, isNullable: true);
            product.AssertHasComplexProperty(model, "Version", typeof(ProductVersion), isNullable: true);
            product.AssertHasNavigationProperty(model, "Category", typeof(Category), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne);

            var singletonProduct = model.AssertHasSingleton(singletonName: "Book", mappedEntityClrType: typeof(Product));
            Assert.Same(singletonProduct, product);

            var category = model.AssertHasEntityType(mappedEntityClrType: typeof(Category));
            Assert.Equal(2, category.StructuralProperties().Count());
            Assert.Single(category.NavigationProperties());
            category.AssertHasKey(model, "ID", EdmPrimitiveTypeKind.String);
            category.AssertHasPrimitiveProperty(model, "ID", EdmPrimitiveTypeKind.String, isNullable: false);
            category.AssertHasPrimitiveProperty(model, "Name", EdmPrimitiveTypeKind.String, isNullable: true);
            category.AssertHasNavigationProperty(model, "Products", typeof(Product), isNullable: false, multiplicity: EdmMultiplicity.Many);

            var version = model.AssertHasComplexType(typeof(ProductVersion));
            Assert.Equal(2, version.StructuralProperties().Count());
            version.AssertHasPrimitiveProperty(model, "Major", EdmPrimitiveTypeKind.Int32, isNullable: false);
            version.AssertHasPrimitiveProperty(model, "Minor", EdmPrimitiveTypeKind.Int32, isNullable: false);
        }

        [Fact]
        public void ModelBuilder_ProductsWithCategoryComplexTypeAttribute()
        {
            // Arrange
            ODataModelBuilder modelBuilder = ODataConventionModelBuilderFactory.Create();
            modelBuilder.EntitySet<ProductWithCategoryComplexTypeAttribute>("Products");

            // Act
            IEdmModel model = modelBuilder.GetEdmModel();
            Assert.Equal(2, model.SchemaElements.OfType<IEdmSchemaType>().Count());

            // Assert
            IEdmComplexType category = model.AssertHasComplexType(typeof(CategoryWithComplexTypeAttribute));
        }

        [Fact]
        public void ModelBuilder_ProductsWithKeyAttribute()
        {
            var modelBuilder = ODataConventionModelBuilderFactory.Create();
            modelBuilder.EntitySet<ProductWithKeyAttribute>("Products");

            var model = modelBuilder.GetEdmModel();
            Assert.Equal(3, model.SchemaElements.OfType<IEdmSchemaType>().Count());

            var product = model.AssertHasEntitySet(entitySetName: "Products", mappedEntityClrType: typeof(ProductWithKeyAttribute));
            Assert.Equal(4, product.StructuralProperties().Count());
            Assert.Single(product.NavigationProperties());
            product.AssertHasKey(model, "IdOfProduct", EdmPrimitiveTypeKind.Int32);
            product.AssertHasPrimitiveProperty(model, "IdOfProduct", EdmPrimitiveTypeKind.Int32, isNullable: false);
            product.AssertHasPrimitiveProperty(model, "Name", EdmPrimitiveTypeKind.String, isNullable: true);
            product.AssertHasPrimitiveProperty(model, "ReleaseDate", EdmPrimitiveTypeKind.DateTimeOffset, isNullable: true);
            product.AssertHasComplexProperty(model, "Version", typeof(ProductVersion), isNullable: true);
            product.AssertHasNavigationProperty(model, "Category", typeof(CategoryWithKeyAttribute), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne);


            var category = model.AssertHasEntityType(mappedEntityClrType: typeof(CategoryWithKeyAttribute));
            Assert.Equal(2, category.StructuralProperties().Count());
            Assert.Single(category.NavigationProperties());
            category.AssertHasKey(model, "IdOfCategory", EdmPrimitiveTypeKind.Guid);
            category.AssertHasPrimitiveProperty(model, "IdOfCategory", EdmPrimitiveTypeKind.Guid, isNullable: false);
            category.AssertHasPrimitiveProperty(model, "Name", EdmPrimitiveTypeKind.String, isNullable: true);
            category.AssertHasNavigationProperty(model, "Products", typeof(ProductWithKeyAttribute), isNullable: false, multiplicity: EdmMultiplicity.Many);

            var version = model.AssertHasComplexType(typeof(ProductVersion));
            Assert.Equal(2, version.StructuralProperties().Count());
            version.AssertHasPrimitiveProperty(model, "Major", EdmPrimitiveTypeKind.Int32, isNullable: false);
            version.AssertHasPrimitiveProperty(model, "Minor", EdmPrimitiveTypeKind.Int32, isNullable: false);
        }

        [Fact]
        public void ModelBuilder_ProductsWithEnumKeyAttribute()
        {
            // Arrange
            ODataConventionModelBuilder modelBuilder = ODataConventionModelBuilderFactory.Create();
            modelBuilder.EntityType<ProductWithEnumKeyAttribute>();

            // Act
            IEdmModel model = modelBuilder.GetEdmModel();

            // Assert
            Assert.NotNull(model);
            Assert.Equal(2, model.SchemaElements.OfType<IEdmSchemaType>().Count());

            IEdmEntityType product = model.AssertHasEntityType(mappedEntityClrType: typeof(ProductWithEnumKeyAttribute));
            product.AssertHasPrimitiveProperty(model, "Name", EdmPrimitiveTypeKind.String, isNullable: true);
            Assert.Equal(2, product.StructuralProperties().Count());
            Assert.Empty(product.NavigationProperties());

            IEdmStructuralProperty enumKey = Assert.Single(product.DeclaredKey);
            Assert.Equal("ProductColor", enumKey.Name);
            Assert.Equal(EdmTypeKind.Enum, enumKey.Type.Definition.TypeKind);
            Assert.Equal("Microsoft.AspNet.OData.Test.Builder.TestModels.Color", enumKey.Type.Definition.FullTypeName());
            Assert.False(enumKey.Type.IsNullable);

            IEdmEnumType colorType = Assert.Single(model.SchemaElements.OfType<IEdmEnumType>());
            Assert.Equal("Microsoft.AspNet.OData.Test.Builder.TestModels.Color", enumKey.Type.Definition.FullTypeName());
            Assert.Equal(3, colorType.Members.Count());
            Assert.Single(colorType.Members.Where(m => m.Name == "Red"));
            Assert.Single(colorType.Members.Where(m => m.Name == "Green"));
            Assert.Single(colorType.Members.Where(m => m.Name == "Blue"));
        }

        [Fact]
        public void ModelBuilder_ProductsWithFilterSortable()
        {
            var modelBuilder = ODataConventionModelBuilderFactory.Create();
            var entityTypeConf = modelBuilder.EntityType<ProductWithFilterSortable>();
            modelBuilder.EntitySet<ProductWithFilterSortable>("Products");
            var model = modelBuilder.GetEdmModel();

            var prop = entityTypeConf.Properties.FirstOrDefault(p => p.Name == "Name");
            Assert.False(prop.NotFilterable);

            prop = entityTypeConf.Properties.FirstOrDefault(p => p.Name == "NotFilterableProperty");
            Assert.True(prop.NotFilterable);

            prop = entityTypeConf.Properties.FirstOrDefault(p => p.Name == "NonFilterableProperty");
            Assert.True(prop.NotFilterable);

            prop = entityTypeConf.Properties.FirstOrDefault(p => p.Name == "NotSortableProperty");
            Assert.True(prop.NotSortable);

            prop = entityTypeConf.Properties.FirstOrDefault(p => p.Name == "UnsortableProperty");
            Assert.True(prop.NotSortable);

            prop = entityTypeConf.Properties.FirstOrDefault(p => p.Name == "Category");
            Assert.True(prop.NotNavigable);
            Assert.True(prop.NotExpandable);

            NavigationPropertyConfiguration naviProp = entityTypeConf.NavigationProperties.FirstOrDefault(p => p.Name == "Category2");
            Assert.True(naviProp.AutoExpand);

            prop = entityTypeConf.Properties.FirstOrDefault(p => p.Name == "CountableProperty");
            Assert.False(prop.NotCountable);

            prop = entityTypeConf.Properties.FirstOrDefault(p => p.Name == "NotCountableProperty");
            Assert.True(prop.NotCountable);
        }

        [Fact]
        public void ModelBuilder_ProductsWithFilterSortableExplicitly()
        {
            var modelBuilder = ODataConventionModelBuilderFactory.Create();
            var entityTypeConf = modelBuilder.AddEntityType(typeof(ProductWithFilterSortable));
            entityTypeConf.AddProperty(typeof(ProductWithFilterSortable).GetProperty("NotFilterableProperty"));
            entityTypeConf.AddProperty(typeof(ProductWithFilterSortable).GetProperty("NotSortableProperty"));
            entityTypeConf.AddProperty(typeof(ProductWithFilterSortable).GetProperty("NonFilterableProperty"));
            entityTypeConf.AddProperty(typeof(ProductWithFilterSortable).GetProperty("UnsortableProperty"));
            entityTypeConf.AddNavigationProperty(typeof(ProductWithFilterSortable).GetProperty("Category"), EdmMultiplicity.One);
            entityTypeConf.AddCollectionProperty(typeof(ProductWithFilterSortable).GetProperty("NotCountableProperty"));

            var model = modelBuilder.GetEdmModel();

            var prop = entityTypeConf.Properties.FirstOrDefault(p => p.Name == "NotFilterableProperty");
            Assert.False(prop.NotFilterable);

            prop = entityTypeConf.Properties.FirstOrDefault(p => p.Name == "NonFilterableProperty");
            Assert.False(prop.NotFilterable);

            prop = entityTypeConf.Properties.FirstOrDefault(p => p.Name == "NotSortableProperty");
            Assert.False(prop.NotSortable);

            prop = entityTypeConf.Properties.FirstOrDefault(p => p.Name == "UnsortableProperty");
            Assert.False(prop.NotSortable);

            prop = entityTypeConf.Properties.FirstOrDefault(p => p.Name == "Category");
            Assert.False(prop.NotNavigable);
            Assert.False(prop.NotExpandable);

            prop = entityTypeConf.Properties.FirstOrDefault(p => p.Name == "NotCountableProperty");
            Assert.False(prop.NotCountable);
        }

        [Fact]
        public void ModelBuilder_ProductsWithConcurrencyCheckAttribute()
        {
            // Arrange
            var modelBuilder = ODataConventionModelBuilderFactory.Create();
            modelBuilder.EntitySet<ProductWithConcurrencyCheckAttribute>("Products");

            // Act
            var model = modelBuilder.GetEdmModel();

            // Assert
            Assert.Single(model.SchemaElements.OfType<IEdmSchemaType>());

            var product = model.AssertHasEntitySet(entitySetName: "Products",
                mappedEntityClrType: typeof(ProductWithConcurrencyCheckAttribute));
            Assert.Equal(2, product.StructuralProperties().Count());
            Assert.Empty(product.NavigationProperties());
            product.AssertHasKey(model, "ID", EdmPrimitiveTypeKind.Int32);

            IEdmStructuralProperty nameProperty =
                product.AssertHasPrimitiveProperty(model, "Name", EdmPrimitiveTypeKind.String, isNullable: true);

            IEdmEntitySet products = model.EntityContainer.FindEntitySet("Products");
            Assert.NotNull(products);

            IEnumerable<IEdmStructuralProperty> currencyProperties = model.GetConcurrencyProperties(products);
            IEdmStructuralProperty currencyProperty = Assert.Single(currencyProperties);
            Assert.Same(currencyProperty, nameProperty);
        }

        [Fact]
        public void ModelBuilder_ProductsWithConcurrencyCheckAttribute_HasVocabuaryAnnotation()
        {
            // Arrange
            var modelBuilder = ODataConventionModelBuilderFactory.Create();
            modelBuilder.EntitySet<ProductWithConcurrencyCheckAttribute>("Products");

            // Act
            var model = modelBuilder.GetEdmModel();

            // Assert
            Assert.Single(model.SchemaElements.OfType<IEdmSchemaType>());

            var entitySet = model.FindDeclaredEntitySet("Products");
            Assert.NotNull(entitySet);

            var annotations = model.FindVocabularyAnnotations<IEdmVocabularyAnnotation>(entitySet, CoreVocabularyModel.ConcurrencyTerm);
            IEdmVocabularyAnnotation concurrencyAnnotation = Assert.Single(annotations);

            IEdmCollectionExpression properties = concurrencyAnnotation.Value as IEdmCollectionExpression;
            Assert.NotNull(properties);

            Assert.Single(properties.Elements);
            var element = properties.Elements.First() as IEdmPathExpression;
            Assert.NotNull(element);

            string path = Assert.Single(element.PathSegments);
            Assert.Equal("Name", path);
        }

        public class BaseTypeWithConcurrencyCheckAttribute
        {
            public int ID { get; set; }

            [ConcurrencyCheck]
            public string Name { get; set; }
        }

        public class SubTypeWithoutConcurrencyCheckAttribute : BaseTypeWithConcurrencyCheckAttribute
        {
            public string SubName { get; set; }
        }

        [Fact]
        public void ModelBuilder_BaseTypeWithConcurrencyCheckAttribute_HasVocabuaryAnnotationOnDerivedEntitySet()
        {
            // Arrange
            var modelBuilder = ODataConventionModelBuilderFactory.Create();
            modelBuilder.EntitySet<SubTypeWithoutConcurrencyCheckAttribute>("SubEntities");

            // Act
            var model = modelBuilder.GetEdmModel();

            // Assert
            var entitySet = model.FindDeclaredEntitySet("SubEntities");
            Assert.NotNull(entitySet);

            var annotations = model.FindVocabularyAnnotations<IEdmVocabularyAnnotation>(entitySet, CoreVocabularyModel.ConcurrencyTerm);
            IEdmVocabularyAnnotation concurrencyAnnotation = Assert.Single(annotations);

            IEdmCollectionExpression properties = concurrencyAnnotation.Value as IEdmCollectionExpression;
            Assert.NotNull(properties);

            Assert.Single(properties.Elements);
            var element = properties.Elements.First() as IEdmPathExpression;
            Assert.NotNull(element);

            string path = Assert.Single(element.PathSegments);
            Assert.Equal("Name", path);
        }

        [Fact]
        public void ModelBuilder_ProductWithTimestampAttribute()
        {
            // Arrange
            var modelBuilder = ODataConventionModelBuilderFactory.Create();
            modelBuilder.EntitySet<ProductWithTimestampAttribute>("Products");

            // Act
            var model = modelBuilder.GetEdmModel();

            // Assert
            Assert.Single(model.SchemaElements.OfType<IEdmSchemaType>());
            var product = model.AssertHasEntitySet(entitySetName: "Products", mappedEntityClrType: typeof(ProductWithTimestampAttribute));
            IEdmStructuralProperty nameProperty =
                product.AssertHasPrimitiveProperty(model, "Name", EdmPrimitiveTypeKind.String, isNullable: true);

            IEdmEntitySet products = model.EntityContainer.FindEntitySet("Products");
            Assert.NotNull(products);

            IEnumerable<IEdmStructuralProperty> currencyProperties = model.GetConcurrencyProperties(products);
            IEdmStructuralProperty currencyProperty = Assert.Single(currencyProperties);
            Assert.Same(currencyProperty, nameProperty);
        }

        [Fact]
        public void ModelBuilder_ProductWithTimestampAttribute_HasVocabuaryAnnotation()
        {
            // Arrange
            var modelBuilder = ODataConventionModelBuilderFactory.Create();
            modelBuilder.EntitySet<ProductWithTimestampAttribute>("Products");

            // Act
            var model = modelBuilder.GetEdmModel();

            // Assert
            Assert.Single(model.SchemaElements.OfType<IEdmSchemaType>());

            var entitySet = model.FindDeclaredEntitySet("Products");
            Assert.NotNull(entitySet);

            var annotations = model.FindVocabularyAnnotations<IEdmVocabularyAnnotation>(entitySet, CoreVocabularyModel.ConcurrencyTerm);
            IEdmVocabularyAnnotation concurrencyAnnotation = Assert.Single(annotations);

            IEdmCollectionExpression properties = concurrencyAnnotation.Value as IEdmCollectionExpression;
            Assert.NotNull(properties);

            Assert.Single(properties.Elements);
            var element = properties.Elements.First() as IEdmPathExpression;
            Assert.NotNull(element);

            string path = Assert.Single(element.PathSegments);
            Assert.Equal("Name", path);
        }

        [Theory]
        [InlineData(typeof(Version[]))]
        [InlineData(typeof(IEnumerable<Version>))]
        [InlineData(typeof(List<Version>))]
        public void ModelBuilder_SupportsComplexCollectionWhenNotToldElementTypeIsComplex(Type complexCollectionPropertyType)
        {
            var modelBuilder = ODataConventionModelBuilderFactory.Create();
            Type entityType =
                new MockType("SampleType")
                .Property<int>("ID")
                .Property(complexCollectionPropertyType, "Property1");

            modelBuilder.AddEntityType(entityType);
            IEdmModel model = modelBuilder.GetEdmModel();
            IEdmEntityType entity = model.GetEdmType(entityType) as IEdmEntityType;

            Assert.NotNull(entity);
            Assert.Equal(2, entity.DeclaredProperties.Count());

            IEdmStructuralProperty property1 = entity.DeclaredProperties.OfType<IEdmStructuralProperty>().SingleOrDefault(p => p.Name == "Property1");
            Assert.NotNull(property1);
            Assert.Equal(EdmTypeKind.Collection, property1.Type.Definition.TypeKind);
            Assert.Equal(EdmTypeKind.Complex, (property1.Type.Definition as IEdmCollectionType).ElementType.Definition.TypeKind);
        }

        [Theory]
        [InlineData(typeof(Version[]))]
        [InlineData(typeof(IEnumerable<Version>))]
        [InlineData(typeof(List<Version>))]
        public void ModelBuilder_SupportsComplexCollectionWhenToldElementTypeIsComplex(Type complexCollectionPropertyType)
        {
            var modelBuilder = ODataConventionModelBuilderFactory.Create();
            Type entityType =
                new MockType("SampleType")
                .Property<int>("ID")
                .Property(complexCollectionPropertyType, "Property1");

            modelBuilder.AddEntityType(entityType);
            modelBuilder.AddComplexType(typeof(Version));
            IEdmModel model = modelBuilder.GetEdmModel();
            IEdmEntityType entity = model.GetEdmType(entityType) as IEdmEntityType;

            Assert.NotNull(entity);
            Assert.Equal(2, entity.DeclaredProperties.Count());

            IEdmStructuralProperty property1 = entity.DeclaredProperties.OfType<IEdmStructuralProperty>().SingleOrDefault(p => p.Name == "Property1");
            Assert.NotNull(property1);
            Assert.Equal(EdmTypeKind.Collection, property1.Type.Definition.TypeKind);
            Assert.Equal(EdmTypeKind.Complex, (property1.Type.Definition as IEdmCollectionType).ElementType.Definition.TypeKind);
        }

        [Theory]
        [InlineData(typeof(int[]))]
        [InlineData(typeof(string[]))]
        public void ModelBuilder_SupportsPrimitiveCollection(Type primitiveCollectionPropertyType)
        {
            var modelBuilder = ODataConventionModelBuilderFactory.Create();
            Type entityType =
                new MockType("SampleType")
                .Property<int>("ID")
                .Property(primitiveCollectionPropertyType, "Property1");

            modelBuilder.AddEntityType(entityType);
            IEdmModel model = modelBuilder.GetEdmModel();
            IEdmEntityType entity = model.GetEdmType(entityType) as IEdmEntityType;

            Assert.NotNull(entity);
            Assert.Equal(2, entity.DeclaredProperties.Count());

            IEdmStructuralProperty property1 = entity.DeclaredProperties.OfType<IEdmStructuralProperty>().SingleOrDefault(p => p.Name == "Property1");
            Assert.NotNull(property1);
            Assert.Equal(EdmTypeKind.Collection, property1.Type.Definition.TypeKind);
            Assert.Equal(EdmTypeKind.Primitive, (property1.Type.Definition as IEdmCollectionType).ElementType.Definition.TypeKind);
        }

        [Theory]
        [InlineData(typeof(Product[]))]
        [InlineData(typeof(ICollection<ProductWithKeyAttribute>))]
        [InlineData(typeof(List<Product>))]
        public void ModelBuilder_DoesnotThrow_ForEntityCollection(Type collectionType)
        {
            var modelBuilder = ODataConventionModelBuilderFactory.Create();
            Type entityType =
                new MockType("SampleType")
                .Property<int>("ID")
                .Property(collectionType, "Products");

            modelBuilder.AddEntityType(entityType);

            ExceptionAssert.DoesNotThrow(
               () => modelBuilder.GetEdmModel());
        }

        [Fact]
        public void ModelBuilder_CanBuild_ModelWithInheritance()
        {
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<Vehicle>("Vehicles");

            IEdmModel model = builder.GetEdmModel();

            Assert.Equal(_totalExpectedSchemaTypesForVehiclesModel, model.SchemaElements.Count());
            Assert.Single(model.EntityContainer.EntitySets());
            model.AssertHasEntitySet("Vehicles", typeof(Vehicle));

            var vehicle = model.AssertHasEntityType(typeof(Vehicle));
            Assert.Equal(2, vehicle.Key().Count());
            Assert.Equal(3, vehicle.Properties().Count());
            vehicle.AssertHasKey(model, "Model", EdmPrimitiveTypeKind.Int32);
            vehicle.AssertHasKey(model, "Name", EdmPrimitiveTypeKind.String);
            vehicle.AssertHasPrimitiveProperty(model, "WheelCount", EdmPrimitiveTypeKind.Int32, isNullable: false);

            var motorcycle = model.AssertHasEntityType(typeof(Motorcycle));
            Assert.Equal(vehicle, motorcycle.BaseEntityType());
            Assert.Equal(2, motorcycle.Key().Count());
            Assert.Equal(5, motorcycle.Properties().Count());
            motorcycle.AssertHasPrimitiveProperty(model, "CanDoAWheelie", EdmPrimitiveTypeKind.Boolean, isNullable: false);
            motorcycle.AssertHasNavigationProperty(model, "Manufacturer", typeof(MotorcycleManufacturer), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne);

            var car = model.AssertHasEntityType(typeof(Car));
            Assert.Equal(vehicle, car.BaseEntityType());
            Assert.Equal(2, car.Key().Count());
            Assert.Equal(5, car.Properties().Count());
            car.AssertHasPrimitiveProperty(model, "SeatingCapacity", EdmPrimitiveTypeKind.Int32, isNullable: false);
            car.AssertHasNavigationProperty(model, "Manufacturer", typeof(CarManufacturer), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne);

            var sportbike = model.AssertHasEntityType(typeof(SportBike));
            Assert.Equal(motorcycle, sportbike.BaseEntityType());
            Assert.Equal(2, sportbike.Key().Count());
            Assert.Equal(5, sportbike.Properties().Count());

            model.AssertHasEntityType(typeof(MotorcycleManufacturer));
            model.AssertHasEntityType(typeof(CarManufacturer));
        }

        [Fact]
        public void ModelBuilder_CanAddEntitiesInAnyOrder()
        {
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntityType<SportBike>();
            builder.EntityType<Car>();
            builder.EntityType<Vehicle>();

            IEdmModel model = builder.GetEdmModel();

            Assert.Equal(_totalExpectedSchemaTypesForVehiclesModel, model.SchemaElements.Count());
        }

        [Fact]
        public void ModelBuilder_Ignores_IgnoredTypeAndTheirDerivedTypes()
        {
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<Vehicle>("Vehicles");
            builder.Ignore<Motorcycle>();

            IEdmModel model = builder.GetEdmModel();

            // ignore motorcycle, sportbike and MotorcycleManufacturer
            Assert.Equal(_totalExpectedSchemaTypesForVehiclesModel - 3, model.SchemaElements.Count());
            Assert.Single(model.EntityContainer.EntitySets());
            model.AssertHasEntitySet("Vehicles", typeof(Vehicle));

            var vehicle = model.AssertHasEntityType(typeof(Vehicle));
            Assert.Equal(2, vehicle.Key().Count());
            Assert.Equal(3, vehicle.Properties().Count());
            vehicle.AssertHasKey(model, "Model", EdmPrimitiveTypeKind.Int32);
            vehicle.AssertHasKey(model, "Name", EdmPrimitiveTypeKind.String);
            vehicle.AssertHasPrimitiveProperty(model, "WheelCount", EdmPrimitiveTypeKind.Int32, isNullable: false);

            var car = model.AssertHasEntityType(typeof(Car));
            Assert.Equal(vehicle, car.BaseEntityType());
            Assert.Equal(2, car.Key().Count());
            Assert.Equal(5, car.Properties().Count());
            car.AssertHasPrimitiveProperty(model, "SeatingCapacity", EdmPrimitiveTypeKind.Int32, isNullable: false);
            car.AssertHasNavigationProperty(model, "Manufacturer", typeof(CarManufacturer), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne);
        }

        [Fact]
        public void ModelBuilder_Can_Add_DerivedTypeOfAnIgnoredType()
        {
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<Vehicle>("Vehicles");
            builder.Ignore<Motorcycle>();
            builder.EntityType<SportBike>();

            IEdmModel model = builder.GetEdmModel();

            Assert.Equal(_totalExpectedSchemaTypesForVehiclesModel - 1, model.SchemaElements.Count());
            Assert.Single(model.EntityContainer.EntitySets());
            model.AssertHasEntitySet("Vehicles", typeof(Vehicle));

            var vehicle = model.AssertHasEntityType(typeof(Vehicle));
            Assert.Equal(2, vehicle.Key().Count());
            Assert.Equal(3, vehicle.Properties().Count());
            vehicle.AssertHasKey(model, "Model", EdmPrimitiveTypeKind.Int32);
            vehicle.AssertHasKey(model, "Name", EdmPrimitiveTypeKind.String);
            vehicle.AssertHasPrimitiveProperty(model, "WheelCount", EdmPrimitiveTypeKind.Int32, isNullable: false);

            var car = model.AssertHasEntityType(typeof(Car));
            Assert.Equal(vehicle, car.BaseEntityType());
            Assert.Equal(2, car.Key().Count());
            Assert.Equal(5, car.Properties().Count());
            car.AssertHasPrimitiveProperty(model, "SeatingCapacity", EdmPrimitiveTypeKind.Int32, isNullable: false);
            car.AssertHasNavigationProperty(model, "Manufacturer", typeof(CarManufacturer), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne);

            var sportbike = model.AssertHasEntityType(typeof(SportBike));
            Assert.Equal(vehicle, sportbike.BaseEntityType());
            Assert.Equal(2, sportbike.Key().Count());
            Assert.Equal(5, sportbike.Properties().Count());
            sportbike.AssertHasNavigationProperty(model, "Manufacturer", typeof(MotorcycleManufacturer), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne);
        }

        [Fact]
        public void ModelBuilder_Patches_BaseType_IfBaseTypeIsNotExplicitlySet()
        {
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntityType<Vehicle>();
            builder.EntityType<Car>();
            builder.EntityType<Motorcycle>();
            builder.EntityType<SportBike>();

            IEdmModel model = builder.GetEdmModel();

            Assert.Equal(_totalExpectedSchemaTypesForVehiclesModel, model.SchemaElements.Count());

            var vehicle = model.AssertHasEntityType(typeof(Vehicle));
            Assert.Null(vehicle.BaseEntityType());

            var motorcycle = model.AssertHasEntityType(typeof(Motorcycle));
            Assert.Equal(vehicle, motorcycle.BaseEntityType());

            var car = model.AssertHasEntityType(typeof(Car));
            Assert.Equal(vehicle, car.BaseEntityType());

            var sportbike = model.AssertHasEntityType(typeof(SportBike));
            Assert.Equal(motorcycle, sportbike.BaseEntityType());

            var motorcycleManufacturer = model.AssertHasEntityType(typeof(MotorcycleManufacturer));
            Assert.Null(motorcycleManufacturer.BaseEntityType());

            var carManufacturer = model.AssertHasEntityType(typeof(CarManufacturer));
            Assert.Null(carManufacturer.BaseEntityType());
        }

        [Fact]
        public void ModelBuilder_DoesnotPatch_BaseType_IfBaseTypeIsExplicitlySet()
        {
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntityType<Vehicle>();
            builder.EntityType<Car>().DerivesFromNothing();
            builder.EntityType<Motorcycle>().DerivesFromNothing();
            builder.EntityType<SportBike>();

            IEdmModel model = builder.GetEdmModel();

            Assert.Equal(_totalExpectedSchemaTypesForVehiclesModel, model.SchemaElements.Count());

            var vehicle = model.AssertHasEntityType(typeof(Vehicle));
            Assert.Null(vehicle.BaseEntityType());
            Assert.Equal(2, vehicle.Key().Count());

            var motorcycle = model.AssertHasEntityType(typeof(Motorcycle));
            Assert.Null(motorcycle.BaseEntityType());
            Assert.Equal(2, motorcycle.Key().Count());
            Assert.Equal(5, motorcycle.Properties().Count());

            var car = model.AssertHasEntityType(typeof(Car));
            Assert.Null(car.BaseEntityType());
            Assert.Equal(2, car.Key().Count());
            Assert.Equal(5, car.Properties().Count());

            var sportbike = model.AssertHasEntityType(typeof(SportBike));
            Assert.Equal(motorcycle, sportbike.BaseEntityType());
            Assert.Equal(2, sportbike.Key().Count());
            Assert.Equal(5, sportbike.Properties().Count());
        }

        [Fact]
        public void ModelBuilder_Figures_AbstractnessOfEntityTypes()
        {
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntityType<Vehicle>();

            IEdmModel model = builder.GetEdmModel();

            Assert.Equal(_totalExpectedSchemaTypesForVehiclesModel, model.SchemaElements.Count());
            Assert.True(model.AssertHasEntityType(typeof(Vehicle)).IsAbstract);
            Assert.False(model.AssertHasEntityType(typeof(Motorcycle)).IsAbstract);
            Assert.False(model.AssertHasEntityType(typeof(Car)).IsAbstract);
            Assert.False(model.AssertHasEntityType(typeof(SportBike)).IsAbstract);
            Assert.False(model.AssertHasEntityType(typeof(CarManufacturer)).IsAbstract);
            Assert.False(model.AssertHasEntityType(typeof(MotorcycleManufacturer)).IsAbstract);
        }

        [Fact]
        public void ModelBuilder_Figures_AbstractnessOfComplexTypes()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.ComplexType<Vehicle>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.Equal(_totalExpectedSchemaTypesForVehiclesModel, model.SchemaElements.Count());
            Assert.True(model.AssertHasComplexType(typeof(Vehicle)).IsAbstract);
            Assert.False(model.AssertHasComplexType(typeof(Motorcycle)).IsAbstract);
            Assert.False(model.AssertHasComplexType(typeof(Car)).IsAbstract);
            Assert.False(model.AssertHasComplexType(typeof(SportBike)).IsAbstract);
            Assert.False(model.AssertHasEntityType(typeof(CarManufacturer)).IsAbstract);
            Assert.False(model.AssertHasEntityType(typeof(MotorcycleManufacturer)).IsAbstract);
            Assert.False(model.AssertHasComplexType(typeof(ManufacturerAddress)).IsAbstract);
            Assert.False(model.AssertHasComplexType(typeof(CarManufacturerAddress)).IsAbstract);
            Assert.False(model.AssertHasComplexType(typeof(MotorcycleManufacturerAddress)).IsAbstract);
        }

        [Fact]
        public void ModelBuilder_Figures_IfOnlyMappedTheEntityTypeExplicitly()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntityType<Zoo>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.Equal(5, model.SchemaElements.Count());
            model.AssertHasEntityType(typeof(Zoo));
            model.AssertHasEntityType(typeof(Animal));
            model.AssertHasEntityType(typeof(Human), typeof(Animal));
            model.AssertHasEntityType(typeof(Horse), typeof(Animal));

            IEdmStructuredType creatureType = model.SchemaElements.OfType<IEdmStructuredType>()
                .SingleOrDefault(t => model.GetEdmType(typeof(Creature)).IsEquivalentTo(t));
            Assert.Null(creatureType);
        }

        [Fact]
        public void ModelBuilder_Figures_IfOnlyMappedTheComplexTypeExplicitly()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.ComplexType<Zoo>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.Equal(5, model.SchemaElements.Count());
            model.AssertHasComplexType(typeof(Zoo));
            model.AssertHasEntityType(typeof(Animal));
            model.AssertHasEntityType(typeof(Human), typeof(Animal));
            model.AssertHasEntityType(typeof(Horse), typeof(Animal));

            IEdmStructuredType creatureType = model.SchemaElements.OfType<IEdmStructuredType>()
                .SingleOrDefault(t => model.GetEdmType(typeof(Creature)).IsEquivalentTo(t));
            Assert.Null(creatureType);
        }

        [Fact]
        public void ModelBuilder_Figures_IfOnlyMappedTheEntityTypeExplicitly_WithBaseAndDerivedTypeProperties()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntityType<Park>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.Equal(5, model.SchemaElements.Count());
            model.AssertHasEntityType(typeof(Park));
            model.AssertHasEntityType(typeof(Animal));
            model.AssertHasEntityType(typeof(Human), typeof(Animal));
            model.AssertHasEntityType(typeof(Horse), typeof(Animal));

            IEdmStructuredType creatureType = model.SchemaElements.OfType<IEdmStructuredType>()
                .SingleOrDefault(t => model.GetEdmType(typeof(Creature)).IsEquivalentTo(t));
            Assert.Null(creatureType);
        }

        [Fact]
        public void ModelBuilder_Figures_IfOnlyMappedTheComplexTypeExplicitly_WithBaseAndDerivedTypeProperties()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.ComplexType<Park>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.Equal(5, model.SchemaElements.Count());
            model.AssertHasComplexType(typeof(Park));
            model.AssertHasEntityType(typeof(Animal));
            model.AssertHasEntityType(typeof(Human), typeof(Animal));
            model.AssertHasEntityType(typeof(Horse), typeof(Animal));

            IEdmStructuredType creatureType = model.SchemaElements.OfType<IEdmStructuredType>()
                .SingleOrDefault(t => model.GetEdmType(typeof(Creature)).IsEquivalentTo(t));
            Assert.Null(creatureType);
        }

#region ClassInheritance
        //    Zoo {  Id (int), SpecialAnimal (Animal) }
        //
        //                 Creature
        //                    |
        //                  Animal
        //                   /  \
        //               Human   Horse
#endregion

        [Fact]
        public void ModelBuilder_Figures_EntityType_AndBaseTypeMappedAsEntityTypeExplicitly()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntityType<Zoo>();
            builder.EntityType<Creature>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.Equal(6, model.SchemaElements.Count()); // 5 types + entity container
            model.AssertHasEntityType(typeof(Zoo));
            model.AssertHasEntityType(typeof(Creature));
            model.AssertHasEntityType(typeof(Animal), typeof(Creature));
            model.AssertHasEntityType(typeof(Human), typeof(Animal));
            model.AssertHasEntityType(typeof(Horse), typeof(Animal));
        }

        [Fact]
        public void ModelBuilder_Figures_EntityType_AndBaseTypeMappedAsComplexTypeExplicitly()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntityType<Zoo>();
            builder.ComplexType<Creature>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.Equal(6, model.SchemaElements.Count());  // 5 types + entity container
            model.AssertHasEntityType(typeof(Zoo));
            model.AssertHasComplexType(typeof(Creature));
            model.AssertHasComplexType(typeof(Animal), typeof(Creature));
            model.AssertHasComplexType(typeof(Human), typeof(Animal));
            model.AssertHasComplexType(typeof(Horse), typeof(Animal));
        }

        [Fact]
        public void ModelBuilder_Figures_EntityType_AndBaseTypeMappedAsComplexTypeExplicitly_InReverseOrder()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.ComplexType<Creature>();
            builder.EntityType<Zoo>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.Equal(6, model.SchemaElements.Count());
            model.AssertHasEntityType(typeof(Zoo));
            model.AssertHasComplexType(typeof(Creature));
            model.AssertHasComplexType(typeof(Animal), typeof(Creature));
            model.AssertHasComplexType(typeof(Human), typeof(Animal));
            model.AssertHasComplexType(typeof(Horse), typeof(Animal));
        }

        [Fact]
        public void ModelBuilder_Figures_Complex_AndBaseTypeMappedAsEntityTypeExplicitly()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.ComplexType<Zoo>();
            builder.EntityType<Creature>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.Equal(6, model.SchemaElements.Count());  // 5 types + entity container
            model.AssertHasComplexType(typeof(Zoo));
            model.AssertHasEntityType(typeof(Creature));
            model.AssertHasEntityType(typeof(Animal), typeof(Creature));
            model.AssertHasEntityType(typeof(Human), typeof(Animal));
            model.AssertHasEntityType(typeof(Horse), typeof(Animal));
        }

        [Fact]
        public void ModelBuilder_Figures_Complex_AndBaseTypeMappedAsEntityTypeExplicitly_InReverseOrder()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntityType<Creature>();
            builder.ComplexType<Zoo>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.Equal(6, model.SchemaElements.Count());  // 5 types + entity container
            model.AssertHasComplexType(typeof(Zoo));
            model.AssertHasEntityType(typeof(Creature));
            model.AssertHasEntityType(typeof(Animal), typeof(Creature));
            model.AssertHasEntityType(typeof(Human), typeof(Animal));
            model.AssertHasEntityType(typeof(Horse), typeof(Animal));
        }

        [Fact]
        public void ModelBuilder_Figures_Complex_AndBaseTypeMappedAsComplexTypeExplicitly()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.ComplexType<Zoo>();
            builder.ComplexType<Creature>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.Equal(6, model.SchemaElements.Count());  // 5 types + entity container
            model.AssertHasComplexType(typeof(Zoo));
            model.AssertHasComplexType(typeof(Creature));
            model.AssertHasComplexType(typeof(Animal), typeof(Creature));
            model.AssertHasComplexType(typeof(Human), typeof(Animal));
            model.AssertHasComplexType(typeof(Horse), typeof(Animal));
        }

        [Fact]
        public void ModelBuilder_Figures_EntityType_AndDerivedTypeMappedAsEntityTypeExplicitly()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntityType<Zoo>();
            builder.EntityType<Human>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.Equal(5, model.SchemaElements.Count());
            model.AssertHasEntityType(typeof(Zoo));
            model.AssertHasEntityType(typeof(Animal));
            model.AssertHasEntityType(typeof(Human), typeof(Animal));
            model.AssertHasEntityType(typeof(Horse), typeof(Animal));
        }

        [Fact]
        public void ModelBuilder_Figures_EntityType_AndDerivedTypeMappedAsEntityTypeExplicitly_InReverseOrder()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntityType<Human>();
            builder.EntityType<Zoo>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.Equal(5, model.SchemaElements.Count());
            model.AssertHasEntityType(typeof(Zoo));
            model.AssertHasEntityType(typeof(Animal));
            model.AssertHasEntityType(typeof(Human), typeof(Animal));
            model.AssertHasEntityType(typeof(Horse), typeof(Animal));
        }

        [Fact]
        public void ModelBuilder_Figures_EntityType_AndDerivedTypeMappedAsEntityTypeExplicitly_DerivedTypeWithoutKey()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntityType<Zoo>();
            builder.EntityType<Human>().Ignore(c => c.HumanId);

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.Equal(5, model.SchemaElements.Count());
            model.AssertHasEntityType(typeof(Zoo));
            model.AssertHasEntityType(typeof(Animal));
            model.AssertHasEntityType(typeof(Human), typeof(Animal));
            model.AssertHasEntityType(typeof(Horse), typeof(Animal));
        }

        [Fact]
        public void ModelBuilder_Figures_EntityType_AndDerivedTypeMappedAsComplexTypeExplicitly()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntityType<Zoo>();
            builder.ComplexType<Human>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.Equal(5, model.SchemaElements.Count());
            model.AssertHasEntityType(typeof(Zoo));
            model.AssertHasComplexType(typeof(Animal));
            model.AssertHasComplexType(typeof(Human), typeof(Animal));
            model.AssertHasComplexType(typeof(Horse), typeof(Animal));

            IEdmStructuredType creatureType = model.SchemaElements.OfType<IEdmStructuredType>()
                .SingleOrDefault(t => model.GetEdmType(typeof(Creature)).IsEquivalentTo(t));
            Assert.Null(creatureType);
        }

        [Fact]
        public void ModelBuilder_Figures_EntityType_AndDerivedTypeMappedAsComplexTypeExplicitly_InReverseOrder()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.ComplexType<Human>();
            builder.EntityType<Zoo>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.Equal(5, model.SchemaElements.Count());
            model.AssertHasEntityType(typeof(Zoo));
            model.AssertHasComplexType(typeof(Animal));
            model.AssertHasComplexType(typeof(Human), typeof(Animal));
            model.AssertHasComplexType(typeof(Horse), typeof(Animal));

            IEdmStructuredType creatureType = model.SchemaElements.OfType<IEdmStructuredType>()
                .SingleOrDefault(t => model.GetEdmType(typeof(Creature)).IsEquivalentTo(t));
            Assert.Null(creatureType);
        }

        [Fact]
        public void ModelBuilder_Figures_ComplexType_AndDerivedTypeMappedAsEntityTypeExplicitly()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.ComplexType<Zoo>();
            builder.EntityType<Human>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Act & Assert
            Assert.Equal(5, model.SchemaElements.Count()); // 4 types + entity container
            model.AssertHasComplexType(typeof(Zoo));
            model.AssertHasEntityType(typeof(Animal));
            model.AssertHasEntityType(typeof(Human), typeof(Animal));
            model.AssertHasEntityType(typeof(Horse), typeof(Animal));
        }

        [Fact]
        public void ModelBuilder_Figures_ComplexType_AndDerivedTypeMappedAsEntityTypeExplicitly_InReverseOrder()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntityType<Human>();
            builder.ComplexType<Zoo>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Act & Assert
            Assert.Equal(5, model.SchemaElements.Count()); // 4 types + entity container
            model.AssertHasComplexType(typeof(Zoo));
            model.AssertHasEntityType(typeof(Animal));
            model.AssertHasEntityType(typeof(Human), typeof(Animal));
            model.AssertHasEntityType(typeof(Horse), typeof(Animal));
        }

        [Fact]
        public void ModelBuilder_Figures_ComplexType_AndDerivedTypeMappedAsComplexTypeExplicitly()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.ComplexType<Zoo>();
            builder.ComplexType<Human>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.Equal(5, model.SchemaElements.Count());
            model.AssertHasComplexType(typeof(Zoo));
            model.AssertHasComplexType(typeof(Animal));
            model.AssertHasComplexType(typeof(Human), typeof(Animal));
            model.AssertHasComplexType(typeof(Horse), typeof(Animal));
        }

        [Fact]
        public void ModelBuilder_Figures_ComplexType_AndBaseTypeMappedAsComplexTypeExplicitly()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.ComplexType<Zoo>();
            builder.ComplexType<Creature>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.Equal(6, model.SchemaElements.Count());
            model.AssertHasComplexType(typeof(Zoo));
            model.AssertHasComplexType(typeof(Creature));
            model.AssertHasComplexType(typeof(Animal), typeof(Creature));
            model.AssertHasComplexType(typeof(Human), typeof(Animal));
            model.AssertHasComplexType(typeof(Horse), typeof(Animal));
        }

        [Fact]
        public void ModelBuilder_Figures_EntityTypeWithBaseAndDerivedProperties_AndDerivedTypeMappedAsComplexTypeExplicitly()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntityType<ZooHorse>();
            builder.ComplexType<Human>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.Equal(5, model.SchemaElements.Count());
            IEdmEntityType zooHorse = model.AssertHasEntityType(typeof(ZooHorse));
            model.AssertHasComplexType(typeof(Animal));
            model.AssertHasComplexType(typeof(Human), typeof(Animal));
            model.AssertHasComplexType(typeof(Horse), typeof(Animal));

            IEdmProperty horseProperty = zooHorse.FindProperty("Horse");
            Assert.NotNull(horseProperty);
            Assert.Equal(EdmPropertyKind.Structural, horseProperty.PropertyKind);
            Assert.IsType<EdmComplexType>(horseProperty.Type.Definition);
            Assert.DoesNotContain(zooHorse.NavigationProperties(), (c) => c.Name == "Horse");

            IEdmProperty animalProperty = zooHorse.FindProperty("Animal");
            Assert.NotNull(animalProperty);
            Assert.Equal(EdmPropertyKind.Structural, animalProperty.PropertyKind);
            Assert.IsType<EdmComplexType>(animalProperty.Type.Definition);
            Assert.DoesNotContain(zooHorse.NavigationProperties(), (c) => c.Name == "Animal");
        }

        [Fact]
        public void ModelBuilder_ThrowsException_EntityType_AndDerivedTypesMappedDifferentTypesExplicitly()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntityType<Zoo>();
            builder.ComplexType<Human>();
            builder.EntityType<Horse>();

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(() => builder.GetEdmModel(),
                "Cannot determine the Edm type for the CLR type 'Microsoft.AspNet.OData.Test.Builder.TestModels.Animal' " +
                "because the derived type 'Microsoft.AspNet.OData.Test.Builder.TestModels.Horse' is configured as entity type and another " +
                "derived type 'Microsoft.AspNet.OData.Test.Builder.TestModels.Human' is configured as complex type.");
        }

        [Fact]
        public void ModelBuilder_Figures_EntityTypeWithBaseAndSilbingProperties_AndDerivedTypeMappedAsComplexTypeExplicitly()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntityType<PlantParkWithOceanPlantAndJasmine>();
            builder.ComplexType<Mangrove>();
            builder.EntityType<Flower>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Act & Assert
            Assert.Equal(7, model.SchemaElements.Count());

            // Verify Ocean sub-tree as complex types
            model.AssertHasComplexType(typeof(OceanPlant));
            model.AssertHasComplexType(typeof(Phycophyta), typeof(OceanPlant));
            model.AssertHasComplexType(typeof(Mangrove), typeof(OceanPlant));

            // Verify Land sub-tree as entity types
            model.AssertHasEntityType(typeof(Flower));
            model.AssertHasEntityType(typeof(Jasmine), typeof(Flower));

            // Verify other types not in the model
            Assert.DoesNotContain(model.SchemaElements.OfType<IEdmStructuredType>(),
                (t) => model.GetEdmType(typeof(Plant)).IsEquivalentTo(t));

            Assert.DoesNotContain(model.SchemaElements.OfType<IEdmStructuredType>(),
                (t) => model.GetEdmType(typeof(LandPlant)).IsEquivalentTo(t));

            Assert.DoesNotContain(model.SchemaElements.OfType<IEdmStructuredType>(),
                (t) => model.GetEdmType(typeof(Tree)).IsEquivalentTo(t));

            // Verify the properties
            IEdmEntityType entityType = model.AssertHasEntityType(typeof(PlantParkWithOceanPlantAndJasmine));

            IEdmProperty oceanProperty = entityType.FindProperty("OceanPlant");
            Assert.NotNull(oceanProperty);
            Assert.Equal(EdmPropertyKind.Structural, oceanProperty.PropertyKind);
            Assert.IsType<EdmComplexType>(oceanProperty.Type.Definition);
            Assert.DoesNotContain(entityType.NavigationProperties(), (c) => c.Name == "OceanPlant");

            IEdmProperty jaemineProperty = entityType.FindProperty("Jasmine");
            Assert.NotNull(jaemineProperty);
            Assert.Equal(EdmPropertyKind.Navigation, jaemineProperty.PropertyKind);
            Assert.IsType<EdmEntityType>(jaemineProperty.Type.Definition);
            Assert.Contains(entityType.NavigationProperties(), (c) => c.Name == "Jasmine");
        }

        [Fact]
        public void ModelBuilder_ThrowsException_EntityType_AndSubDerivedTypesMappedDifferentTypesExplicitly()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntityType<PlantPark>();
            builder.ComplexType<Phycophyta>();
            builder.EntityType<Jasmine>();

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(() => builder.GetEdmModel(),
                "Cannot determine the Edm type for the CLR type 'Microsoft.AspNet.OData.Test.Builder.TestModels.Plant' " +
                "because the derived type 'Microsoft.AspNet.OData.Test.Builder.TestModels.Jasmine' is configured as entity type and another " +
                "derived type 'Microsoft.AspNet.OData.Test.Builder.TestModels.Phycophyta' is configured as complex type.");
        }

        [Fact]
        public void ModelBuilder_Doesnot_Override_AbstractnessOfEntityTypes_IfSet()
        {
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntityType<Vehicle>();
            builder.EntityType<Motorcycle>().Abstract();

            IEdmModel model = builder.GetEdmModel();

            Assert.Equal(_totalExpectedSchemaTypesForVehiclesModel, model.SchemaElements.Count());
            Assert.True(model.AssertHasEntityType(typeof(Motorcycle)).IsAbstract);
            Assert.False(model.AssertHasEntityType(typeof(SportBike)).IsAbstract);
        }

        [Fact]
        public void ModelBuilder_Doesnot_Override_AbstractnessOfComplexTypes_IfSet()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.ComplexType<Vehicle>();
            builder.ComplexType<Motorcycle>().Abstract();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.Equal(_totalExpectedSchemaTypesForVehiclesModel, model.SchemaElements.Count());
            Assert.True(model.AssertHasComplexType(typeof(Motorcycle)).IsAbstract);
            Assert.False(model.AssertHasComplexType(typeof(SportBike)).IsAbstract);
        }

        [Fact]
        public void ModelBuilder_CanHaveAnAbstractDerivedTypeOfConcreteBaseType()
        {
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntityType<Vehicle>();
            builder.EntityType<SportBike>().Abstract();

            IEdmModel model = builder.GetEdmModel();

            Assert.Equal(_totalExpectedSchemaTypesForVehiclesModel, model.SchemaElements.Count());
            Assert.False(model.AssertHasEntityType(typeof(Motorcycle)).IsAbstract);
            Assert.True(model.AssertHasEntityType(typeof(SportBike)).IsAbstract);

            Assert.Equal(model.AssertHasEntityType(typeof(SportBike)).BaseEntityType(), model.AssertHasEntityType(typeof(Motorcycle)));
        }

        [Fact]
        public void ModelBuilder_TypesInInheritanceCanHaveComplexTypes()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<Vehicle>("vehicles");

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.Equal(_totalExpectedSchemaTypesForVehiclesModel, model.SchemaElements.Count());
            model.AssertHasComplexType(typeof(ManufacturerAddress));
            model.AssertHasComplexType(typeof(CarManufacturerAddress));
            model.AssertHasComplexType(typeof(MotorcycleManufacturerAddress));
        }

        [Fact]
        public void ModelBuilder_TypesInInheritance_CanSetBaseComplexTypes()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.ComplexType<ManufacturerAddress>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.Equal(4, model.SchemaElements.Count());
            Assert.Null(model.AssertHasComplexType(typeof(ManufacturerAddress)).BaseComplexType());
            model.AssertHasComplexType(typeof(CarManufacturerAddress), typeof(ManufacturerAddress));
            model.AssertHasComplexType(typeof(MotorcycleManufacturerAddress), typeof(ManufacturerAddress));
        }

        [Fact]
        public void ModelBuilder_ModelAliased_IfModelAliasingEnabled()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderHelper.CreateWithModelAliasing(true);
            builder.EntitySet<ModelAlias>("ModelAliases");

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmEntityType entityType = model.AssertHasEntityType(typeof(ModelAlias));
            Assert.Equal("ModelAlias2", entityType.Name);
            Assert.Equal("com.contoso", entityType.Namespace);
            Assert.Equal("com.contoso.ModelAlias2", entityType.FullName());

            // make sure we find the correct IEdmType, by verifing members
            entityType.AssertHasKey(model, "Id", EdmPrimitiveTypeKind.Int32);
            entityType.AssertHasPrimitiveProperty(model, "FirstName", EdmPrimitiveTypeKind.String, true);
        }

        [Fact]
        public void ModelBuilder_PropertyAliased_IfModelAliasingEnabled()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderHelper.CreateWithModelAliasing(true);
            builder.EntitySet<PropertyAlias>("PropertyAliases");

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmEntityType entityType = model.AssertHasEntityType(typeof(PropertyAlias));
            Assert.Equal("PropertyAlias2", entityType.Name);
            Assert.Equal("com.contoso", entityType.Namespace);
            Assert.Equal("com.contoso.PropertyAlias2", entityType.FullName());

            // make sure we find the correct IEdmType, by verifing members
            entityType.AssertHasKey(model, "Id", EdmPrimitiveTypeKind.Int32);
            entityType.AssertHasPrimitiveProperty(model, "FirstNameAlias", EdmPrimitiveTypeKind.String, true);
        }

        [Fact]
        public void ModelBuilder_DerivedClassPropertyAliased_IfModelAliasingEnabled()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderHelper.CreateWithModelAliasing(true);
            builder.EntitySet<PropertyAlias>("PropertyAliases");

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmEntityType entityType = model.AssertHasEntityType(typeof(PropertyAliasDerived));
            Assert.Equal("PropertyAliasDerived2", entityType.Name);
            Assert.Equal("com.contoso", entityType.Namespace);
            Assert.Equal("com.contoso.PropertyAliasDerived2", entityType.FullName());

            // make sure we find the correct IEdmType, by verifing members
            entityType.AssertHasKey(model, "Id", EdmPrimitiveTypeKind.Int32);
            entityType.AssertHasPrimitiveProperty(model, "FirstNameAlias", EdmPrimitiveTypeKind.String, true);
            entityType.AssertHasPrimitiveProperty(model, "LastNameAlias", EdmPrimitiveTypeKind.String, true);
        }

        [Fact]
        public void ModelBuilder_PropertyNotAliased_IfPropertyAddedExplicitly()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderHelper.CreateWithModelAliasing(true);
            EntitySetConfiguration<PropertyAlias> entitySet = builder.EntitySet<PropertyAlias>("PropertyAliases");
            entitySet.EntityType.Property(p => p.FirstName).Name = "GivenName";
            entitySet.EntityType.Property(p => p.Points).Name = "Score";

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmEntityType entityType = model.AssertHasEntityType(typeof(PropertyAlias));
            entityType.AssertHasKey(model, "Id", EdmPrimitiveTypeKind.Int32);
            entityType.AssertHasPrimitiveProperty(model, "GivenName", EdmPrimitiveTypeKind.String, true);
            entityType.AssertHasPrimitiveProperty(model, "Score", EdmPrimitiveTypeKind.Int32, false);
        }

        [Fact]
        public void ModelBuilder_DerivedClassPropertyNotAliased_IfPropertyAddedExplicitly()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderHelper.CreateWithModelAliasing(true);
            EntityTypeConfiguration<PropertyAliasDerived> derived = builder.EntityType<PropertyAliasDerived>()
                .DerivesFrom<PropertyAlias>();
            derived.Property(p => p.LastName).Name = "FamilyName";
            derived.Property(p => p.Age).Name = "CurrentAge";

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmEntityType entityType = model.AssertHasEntityType(typeof(PropertyAliasDerived));
            entityType.AssertHasKey(model, "Id", EdmPrimitiveTypeKind.Int32);
            entityType.AssertHasPrimitiveProperty(model, "FamilyName", EdmPrimitiveTypeKind.String, true);
            entityType.AssertHasPrimitiveProperty(model, "CurrentAge", EdmPrimitiveTypeKind.Int32, false);
        }

        [Fact]
        public void ModelBuilder_Figures_Bindings_For_DerivedNavigationProperties()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<Vehicle>("vehicles");
            builder.Singleton<Vehicle>("MyVehicle");
            builder.EntitySet<Manufacturer>("manufacturers");

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            model.AssertHasEntitySet("vehicles", typeof(Vehicle));
            IEdmEntitySet vehicles = model.EntityContainer.FindEntitySet("vehicles");

            model.AssertHasSingleton("MyVehicle", typeof(Vehicle));
            IEdmSingleton singleton = model.EntityContainer.FindSingleton("MyVehicle");

            IEdmEntityType car = model.AssertHasEntityType(typeof(Car));
            IEdmEntityType motorcycle = model.AssertHasEntityType(typeof(Motorcycle));
            IEdmEntityType sportbike = model.AssertHasEntityType(typeof(SportBike));

            // for entity set
            Assert.Equal(2, vehicles.NavigationPropertyBindings.Count());
            vehicles.AssertHasNavigationTarget(
                car.AssertHasNavigationProperty(model, "Manufacturer", typeof(CarManufacturer), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne),
                "manufacturers", "Microsoft.AspNet.OData.Test.Builder.TestModels.Car/Manufacturer");
            vehicles.AssertHasNavigationTarget(
                motorcycle.AssertHasNavigationProperty(model, "Manufacturer", typeof(MotorcycleManufacturer), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne),
                "manufacturers", "Microsoft.AspNet.OData.Test.Builder.TestModels.Motorcycle/Manufacturer");
            vehicles.AssertHasNavigationTarget(
                sportbike.AssertHasNavigationProperty(model, "Manufacturer", typeof(MotorcycleManufacturer), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne),
                "manufacturers", "Microsoft.AspNet.OData.Test.Builder.TestModels.Motorcycle/Manufacturer");

            // for singleton
            Assert.Equal(2, singleton.NavigationPropertyBindings.Count());
            singleton.AssertHasNavigationTarget(
                car.AssertHasNavigationProperty(model, "Manufacturer", typeof(CarManufacturer), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne),
                "manufacturers", "Microsoft.AspNet.OData.Test.Builder.TestModels.Car/Manufacturer");
            singleton.AssertHasNavigationTarget(
                motorcycle.AssertHasNavigationProperty(model, "Manufacturer", typeof(MotorcycleManufacturer), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne),
                "manufacturers", "Microsoft.AspNet.OData.Test.Builder.TestModels.Motorcycle/Manufacturer");
            singleton.AssertHasNavigationTarget(
                sportbike.AssertHasNavigationProperty(model, "Manufacturer", typeof(MotorcycleManufacturer), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne),
                "manufacturers", "Microsoft.AspNet.OData.Test.Builder.TestModels.Motorcycle/Manufacturer");
        }

        [Fact]
        public void ModelBuilder_BindsToTheClosestEntitySet_ForNavigationProperties()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<Vehicle>("vehicles");
            builder.Singleton<Vehicle>("MyVehicle");
            builder.EntitySet<CarManufacturer>("car_manufacturers");
            builder.EntitySet<MotorcycleManufacturer>("motorcycle_manufacturers");

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            model.AssertHasEntitySet("vehicles", typeof(Vehicle));
            IEdmEntitySet vehicles = model.EntityContainer.FindEntitySet("vehicles");

            model.AssertHasSingleton("MyVehicle", typeof(Vehicle));
            IEdmSingleton singleton = model.EntityContainer.FindSingleton("MyVehicle");

            IEdmEntityType car = model.AssertHasEntityType(typeof(Car));
            IEdmEntityType motorcycle = model.AssertHasEntityType(typeof(Motorcycle));
            IEdmEntityType sportbike = model.AssertHasEntityType(typeof(SportBike));

            // for entity set
            Assert.Equal(2, vehicles.NavigationPropertyBindings.Count());
            vehicles.AssertHasNavigationTarget(
                car.AssertHasNavigationProperty(model, "Manufacturer", typeof(CarManufacturer), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne),
                "car_manufacturers", "Microsoft.AspNet.OData.Test.Builder.TestModels.Car/Manufacturer");
            vehicles.AssertHasNavigationTarget(
                motorcycle.AssertHasNavigationProperty(model, "Manufacturer", typeof(MotorcycleManufacturer), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne),
                "motorcycle_manufacturers", "Microsoft.AspNet.OData.Test.Builder.TestModels.Motorcycle/Manufacturer");
            vehicles.AssertHasNavigationTarget(
                sportbike.AssertHasNavigationProperty(model, "Manufacturer", typeof(MotorcycleManufacturer), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne),
                "motorcycle_manufacturers", "Microsoft.AspNet.OData.Test.Builder.TestModels.Motorcycle/Manufacturer");

            // for singleton
            Assert.Equal(2, singleton.NavigationPropertyBindings.Count());
            singleton.AssertHasNavigationTarget(
                car.AssertHasNavigationProperty(model, "Manufacturer", typeof(CarManufacturer), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne),
                "car_manufacturers", "Microsoft.AspNet.OData.Test.Builder.TestModels.Car/Manufacturer");
            singleton.AssertHasNavigationTarget(
                motorcycle.AssertHasNavigationProperty(model, "Manufacturer", typeof(MotorcycleManufacturer), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne),
                "motorcycle_manufacturers", "Microsoft.AspNet.OData.Test.Builder.TestModels.Motorcycle/Manufacturer");
            singleton.AssertHasNavigationTarget(
                sportbike.AssertHasNavigationProperty(model, "Manufacturer", typeof(MotorcycleManufacturer), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne),
                "motorcycle_manufacturers", "Microsoft.AspNet.OData.Test.Builder.TestModels.Motorcycle/Manufacturer");
        }

        [Fact]
        public void ModelBuilder_BindsToAllEntitySets()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();

            builder.EntitySet<Vehicle>("vehicles");
            builder.EntitySet<Car>("cars");
            builder.EntitySet<Motorcycle>("motorcycles");
            builder.EntitySet<SportBike>("sportbikes");
            builder.EntitySet<CarManufacturer>("car_manufacturers");
            builder.EntitySet<MotorcycleManufacturer>("motorcycle_manufacturers");

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            // one for motorcycle manufacturer and one for car manufacturer
            IEdmEntitySet vehicles = model.EntityContainer.FindEntitySet("vehicles");
            Assert.Equal(2, vehicles.NavigationPropertyBindings.Count());
            Assert.Equal("Microsoft.AspNet.OData.Test.Builder.TestModels.Car/Manufacturer", vehicles.NavigationPropertyBindings.First().Path.Path);
            Assert.Equal("Microsoft.AspNet.OData.Test.Builder.TestModels.Motorcycle/Manufacturer", vehicles.NavigationPropertyBindings.Last().Path.Path);

            // one for car manufacturer
            IEdmEntitySet cars = model.EntityContainer.FindEntitySet("cars");
            IEdmNavigationPropertyBinding binding = Assert.Single(cars.NavigationPropertyBindings);
            Assert.Equal("Manufacturer", binding.Path.Path);

            // one for motorcycle manufacturer
            IEdmEntitySet motorcycles = model.EntityContainer.FindEntitySet("motorcycles");
            binding = Assert.Single(motorcycles.NavigationPropertyBindings);
            Assert.Equal("Manufacturer", binding.Path.Path);

            // one for motorcycle manufacturer
            IEdmEntitySet sportbikes = model.EntityContainer.FindEntitySet("sportbikes");
            binding = Assert.Single(sportbikes.NavigationPropertyBindings);
            Assert.Equal("Manufacturer", binding.Path.Path);

            // no navigations
            IEdmEntitySet carManufacturers = model.EntityContainer.FindEntitySet("car_manufacturers");
            Assert.Empty(carManufacturers.NavigationPropertyBindings);

            //  no navigations
            IEdmEntitySet motorcycleManufacturers = model.EntityContainer.FindEntitySet("motorcycle_manufacturers");
            Assert.Empty(motorcycleManufacturers.NavigationPropertyBindings);
        }

        [Fact]
        public void ModelBuilder_OnSingleton_BindsToAllEntitySets()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<CarManufacturer>("CarManfacturers");
            builder.EntitySet<MotorcycleManufacturer>("MotoerCycleManfacturers");
            builder.Singleton<Vehicle>("MyVehicle");
            builder.Singleton<Car>("Contoso");
            builder.Singleton<Motorcycle>("MyMotorcycle");
            builder.Singleton<SportBike>("Gianta");
            builder.Singleton<CarManufacturer>("Fordo");
            builder.Singleton<MotorcycleManufacturer>("Yayaham");

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            // one for motorcycle manufacturer and one for car manufacturer
            IEdmSingleton vehicle = model.EntityContainer.FindSingleton("MyVehicle");
            Assert.Equal(2, vehicle.NavigationPropertyBindings.Count());
            Assert.Equal("Microsoft.AspNet.OData.Test.Builder.TestModels.Car/Manufacturer", vehicle.NavigationPropertyBindings.First().Path.Path);
            Assert.Equal("Microsoft.AspNet.OData.Test.Builder.TestModels.Motorcycle/Manufacturer", vehicle.NavigationPropertyBindings.Last().Path.Path);

            // one for car manufacturer
            IEdmSingleton car = model.EntityContainer.FindSingleton("Contoso");
            IEdmNavigationPropertyBinding binding = Assert.Single(car.NavigationPropertyBindings);
            Assert.Equal("Manufacturer", binding.Path.Path);

            // one for motorcycle manufacturer
            IEdmSingleton motorcycle = model.EntityContainer.FindSingleton("MyMotorcycle");
            binding = Assert.Single(motorcycle.NavigationPropertyBindings);
            Assert.Equal("Manufacturer", binding.Path.Path);

            // one for motorcycle manufacturer
            IEdmSingleton sportbike = model.EntityContainer.FindSingleton("Gianta");
            binding = Assert.Single(sportbike.NavigationPropertyBindings);
            Assert.Equal("Manufacturer", binding.Path.Path);

            // no navigation
            IEdmSingleton carManufacturer = model.EntityContainer.FindSingleton("Fordo");
            Assert.Empty(carManufacturer.NavigationPropertyBindings);

            //  no navigation
            IEdmSingleton motorcycleManufacturer = model.EntityContainer.FindSingleton("Yayaham");
            Assert.Empty(motorcycleManufacturer.NavigationPropertyBindings);
        }

        [Fact]
        public void ModelBuilder_BindingsTo_WithComplexTypePath()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntityType<ManufacturerAddress>().HasKey(a => a.City);
            builder.ComplexType<Manufacturer>(); // Manufacturer is a complex type

            builder.EntitySet<Vehicle>("Vehicles");
            builder.Singleton<Vehicle>("MyVehicle");
            builder.EntitySet<ManufacturerAddress>("Addresses");

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            model.AssertHasEntitySet("Vehicles", typeof(Vehicle));
            IEdmEntitySet vehicles = model.EntityContainer.FindEntitySet("Vehicles");

            model.AssertHasSingleton("MyVehicle", typeof(Vehicle));
            IEdmSingleton singleton = model.EntityContainer.FindSingleton("MyVehicle");

            model.AssertHasEntityType(typeof(Car), typeof(Vehicle));
            model.AssertHasEntityType(typeof(Motorcycle), typeof(Vehicle));
            model.AssertHasEntityType(typeof(SportBike), typeof(Motorcycle));

            model.AssertHasEntityType(typeof(ManufacturerAddress));
            IEdmComplexType manufacturer = model.AssertHasComplexType(typeof(Manufacturer));

            IEdmNavigationProperty addressNav = manufacturer.AssertHasNavigationProperty(model, "Address", typeof(ManufacturerAddress), isNullable: true,
                multiplicity: EdmMultiplicity.ZeroOrOne);

            // for entity set
            Assert.Equal(2, vehicles.NavigationPropertyBindings.Count());
            vehicles.AssertHasNavigationTarget(addressNav, "Addresses", "Microsoft.AspNet.OData.Test.Builder.TestModels.Car/Manufacturer/Address");
            vehicles.AssertHasNavigationTarget(addressNav, "Addresses", "Microsoft.AspNet.OData.Test.Builder.TestModels.Motorcycle/Manufacturer/Address");

            // for singleton
            Assert.Equal(2, singleton.NavigationPropertyBindings.Count());
            singleton.AssertHasNavigationTarget(addressNav, "Addresses", "Microsoft.AspNet.OData.Test.Builder.TestModels.Car/Manufacturer/Address");
            singleton.AssertHasNavigationTarget(addressNav, "Addresses", "Microsoft.AspNet.OData.Test.Builder.TestModels.Motorcycle/Manufacturer/Address");
        }

        [Fact]
        public void ModelBuilder_BindingsTo_WithComplexTypePath_BindAllPathToEntitySet()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<BindingCustomer>("Customers");
            builder.EntitySet<BindingCity>("Cities");

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            model.AssertHasEntitySet("Customers", typeof(BindingCustomer));
            IEdmEntitySet customers = model.EntityContainer.FindEntitySet("Customers");

            model.AssertHasEntitySet("Cities", typeof(BindingCity));

            IEdmComplexType address = model.AssertHasComplexType(typeof(BindingAddress));
            IEdmComplexType usAddress = model.AssertHasComplexType(typeof(BindingUsAddress), typeof(BindingAddress));

            IEdmNavigationProperty cityNav = address.AssertHasNavigationProperty(model, "City", typeof(BindingCity), isNullable: true,
                multiplicity: EdmMultiplicity.ZeroOrOne);

            IEdmNavigationProperty citiesNav = address.AssertHasNavigationProperty(model, "Cities", typeof(BindingCity), isNullable: true,
                multiplicity: EdmMultiplicity.Many);

            IEdmNavigationProperty usCityNav = usAddress.AssertHasNavigationProperty(model, "UsCity", typeof(BindingCity), isNullable: true,
                multiplicity: EdmMultiplicity.ZeroOrOne);

            IEdmNavigationProperty usCitiesNav = usAddress.AssertHasNavigationProperty(model, "UsCities", typeof(BindingCity), isNullable: true,
                multiplicity: EdmMultiplicity.Many);

            Assert.Equal(20, customers.NavigationPropertyBindings.Count());
            foreach (var edmNavigationPropertyBinding in customers.NavigationPropertyBindings)
            {
                Console.WriteLine(edmNavigationPropertyBinding.NavigationProperty.Name + "," + edmNavigationPropertyBinding.Target.Name + ", " +
                    edmNavigationPropertyBinding.Path.Path);
            }

            foreach (var navigation in new[] { "City", "Cities" })
            {
                IEdmNavigationProperty nav = navigation == "City" ? cityNav : citiesNav;
                customers.AssertHasNavigationTarget(nav, "Cities", "Location/" + navigation);
                customers.AssertHasNavigationTarget(nav, "Cities", "Address/" + navigation);
                customers.AssertHasNavigationTarget(nav, "Cities", "Addresses/" + navigation);
                customers.AssertHasNavigationTarget(nav, "Cities", "Microsoft.AspNet.OData.Test.Formatter.BindingVipCustomer/VipLocation/" + navigation);
                customers.AssertHasNavigationTarget(nav, "Cities", "Microsoft.AspNet.OData.Test.Formatter.BindingVipCustomer/VipAddresses/" + navigation);
            }

            foreach (var navigation in new[] { "UsCity", "UsCities" })
            {
                IEdmNavigationProperty nav = navigation == "UsCity" ? usCityNav : usCitiesNav;
                customers.AssertHasNavigationTarget(nav, "Cities", "Location/Microsoft.AspNet.OData.Test.Formatter.BindingUsAddress/" + navigation);
                customers.AssertHasNavigationTarget(nav, "Cities", "Address/Microsoft.AspNet.OData.Test.Formatter.BindingUsAddress/" + navigation);
                customers.AssertHasNavigationTarget(nav, "Cities", "Addresses/Microsoft.AspNet.OData.Test.Formatter.BindingUsAddress/" + navigation);
                customers.AssertHasNavigationTarget(nav, "Cities", "Microsoft.AspNet.OData.Test.Formatter.BindingVipCustomer/VipLocation/Microsoft.AspNet.OData.Test.Formatter.BindingUsAddress/" + navigation);
                customers.AssertHasNavigationTarget(nav, "Cities", "Microsoft.AspNet.OData.Test.Formatter.BindingVipCustomer/VipAddresses/Microsoft.AspNet.OData.Test.Formatter.BindingUsAddress/" + navigation);
            }
        }

        [Fact]
        public void ModelBuilder_OnSingleton_OnlyHasOneBinding_WithoutAnyEntitySets()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.Singleton<Employee>("Gibs");

            // Act
            IEdmModel model = builder.GetEdmModel();
            Assert.NotNull(model); // Guard
            IEdmSingleton singleton = model.EntityContainer.FindSingleton("Gibs");

            // Assert
            Assert.NotNull(singleton);
            Assert.Single(singleton.NavigationPropertyBindings);
            Assert.Equal("Boss", singleton.NavigationPropertyBindings.Single().NavigationProperty.Name);
            Assert.Equal("Boss", singleton.NavigationPropertyBindings.Single().Path.Path);
        }

        [Fact]
        public void ModelBuilder_OnSingleton_HasBindings_WithEntitySet()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.Singleton<Employee>("Gates");
            builder.EntitySet<Customer>("Customers");

            // Act
            IEdmModel model = builder.GetEdmModel();
            Assert.NotNull(model); // Guard
            IEdmSingleton singleton = model.EntityContainer.FindSingleton("Gates");
            Assert.NotNull(singleton); // Guard

            // Assert
            Assert.Equal(2, singleton.NavigationPropertyBindings.Count());
            var employeeType = model.AssertHasEntityType(typeof(Employee));
            var salePersonType = model.AssertHasEntityType(typeof(SalesPerson));
            model.AssertHasEntityType(typeof(Customer));
            var bossProperty = employeeType.AssertHasNavigationProperty(model, "Boss", typeof(Employee), true, EdmMultiplicity.ZeroOrOne);
            var customerProperty = salePersonType.AssertHasNavigationProperty(model, "Customers", typeof(Customer), true, EdmMultiplicity.Many);

            Assert.Equal(EdmNavigationSourceKind.Singleton, singleton.FindNavigationTarget(bossProperty).NavigationSourceKind());
            Assert.Equal(EdmNavigationSourceKind.EntitySet, singleton.FindNavigationTarget(customerProperty).NavigationSourceKind());
        }

        [Fact]
        public void ModelBuilder_DerivedTypeDeclaringKeyThrows()
        {
            // Arrange
            MockType baseType =
                  new MockType("BaseType")
                  .Property(typeof(int), "BaseTypeID");

            MockType derivedType =
                new MockType("DerivedType")
                .Property(typeof(int), "DerivedTypeId")
                .BaseType(baseType);

            var configuration = RoutingConfigurationFactory.CreateWithTypes(baseType, derivedType);
            var builder = ODataConventionModelBuilderFactory.Create(configuration);

            builder.AddEntitySet("bases", builder.AddEntityType(baseType));

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(
                () => builder.GetEdmModel(),
            "Cannot define keys on type 'DefaultNamespace.DerivedType' deriving from 'DefaultNamespace.BaseType'. " +
            "The base type in the entity inheritance hierarchy already contains keys.");
        }

        [Fact]
        public void ModelBuilder_CanDeclareAbstractEntityTypeWithoutKey()
        {
            // Arrange
            var builder = ODataConventionModelBuilderFactory.Create();
            builder.AddEntityType(typeof(AbstractEntityType));

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Act & Assert
            Assert.NotNull(model);
            IEdmEntityType abstractType =
                model.SchemaElements.OfType<IEdmEntityType>().FirstOrDefault(e => e.Name == "AbstractEntityType");
            Assert.NotNull(abstractType);
            Assert.True(abstractType.IsAbstract);
            Assert.Null(abstractType.DeclaredKey);
            Assert.Empty(abstractType.Properties());
        }

        [Fact]
        public void ModelBuilder_CanDeclareKeyOnDerivedType_IfBaseEntityTypeWithoutKey()
        {
            // Arrange
            var builder = ODataConventionModelBuilderFactory.Create();
            builder.EntityType<BaseAbstractEntityType>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Act & Assert
            Assert.NotNull(model);
            IEdmEntityType abstractType =
                model.SchemaElements.OfType<IEdmEntityType>().FirstOrDefault(e => e.Name == "BaseAbstractEntityType");
            Assert.NotNull(abstractType);
            Assert.True(abstractType.IsAbstract);
            Assert.Null(abstractType.DeclaredKey);
            Assert.Empty(abstractType.Properties());

            IEdmEntityType derivedType =
                model.SchemaElements.OfType<IEdmEntityType>().FirstOrDefault(e => e.Name == "DerivedEntityTypeWithOwnKey");
            Assert.NotNull(derivedType);
            Assert.False(derivedType.IsAbstract);

            Assert.NotNull(derivedType.DeclaredKey);
            IEdmStructuralProperty keyProperty = Assert.Single(derivedType.DeclaredKey);
            Assert.Equal("DerivedEntityTypeWithOwnKeyId", keyProperty.Name);
        }

        [Fact]
        public void ModelBuilder_DeclareKeyOnDerivedTypeAndSubDerivedType_Throws()
        {
            // Arrange
            var builder = ODataConventionModelBuilderFactory.Create();
            builder.EntityType<BaseAbstractEntityType2>();

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(
                () => builder.GetEdmModel(),
            "Cannot define keys on type 'Microsoft.AspNet.OData.Test.Builder.Conventions.SubSubEntityTypeWithOwnKey' deriving from 'Microsoft.AspNet.OData.Test.Builder.Conventions.SubEntityTypeWithOwnKey'. " +
            "The base type in the entity inheritance hierarchy already contains keys.");
        }

        [Fact]
        public void ModelBuilder_DeclareEntitySetOnAbstractEntityTypeWithoutKeyThrows()
        {
            // Arrange
            var builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<AbstractEntityType>("entitySet");

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(
                () => builder.GetEdmModel(),
            "The entity set 'entitySet' is based on type 'Microsoft.AspNet.OData.Test.Builder.Conventions.AbstractEntityType' that has no keys defined.");
        }

        [Fact]
        public void ModelBuilder_DeclareSingletonOnAbstractEntityTypeWithoutKeyWorks()
        {
            // Arrange
            var builder = ODataConventionModelBuilderFactory.Create();
            builder.Singleton<AbstractEntityType>("Me");

            // Act
            IEdmModel modle = builder.GetEdmModel();

            // Assert
            Assert.NotNull(modle);
            Assert.NotNull(modle.FindDeclaredSingleton("Me"));
        }

        [Fact]
        public void DerivedTypes_Can_DefineKeys_InQueryCompositionMode()
        {
            // Arrange
            MockType baseType =
                 new MockType("BaseType")
                 .Property(typeof(int), "ID");

            MockType derivedType =
                new MockType("DerivedType")
                .Property(typeof(int), "DerivedTypeId")
                .BaseType(baseType);

            var configuration = RoutingConfigurationFactory.CreateWithTypes(baseType, derivedType);
            var builder = ODataConventionModelBuilderFactory.Create(configuration, isQueryCompositionMode: true);

            builder.AddEntitySet("bases", builder.AddEntityType(baseType));

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            model.AssertHasEntitySet("bases", baseType);
            IEdmEntityType baseEntityType = model.AssertHasEntityType(baseType);
            IEdmEntityType derivedEntityType = model.AssertHasEntityType(derivedType, baseType);
            baseEntityType.AssertHasKey(model, "ID", EdmPrimitiveTypeKind.Int32);
            derivedEntityType.AssertHasPrimitiveProperty(model, "DerivedTypeId", EdmPrimitiveTypeKind.Int32, isNullable: false);
        }

        [Fact]
        public void DerivedTypes_Can_DefineEnumKeys_InQueryCompositionMode()
        {
            // Arrange
            MockType baseType = new MockType("BaseType")
                .Property(typeof(Color), "ID");

            MockType derivedType = new MockType("DerivedType")
                .Property(typeof(Color), "DerivedTypeId")
                .BaseType(baseType);

            var configuration = RoutingConfigurationFactory.CreateWithTypes(baseType, derivedType);
            var builder = ODataConventionModelBuilderFactory.Create(configuration, isQueryCompositionMode: true);

            builder.AddEntitySet("bases", builder.AddEntityType(baseType));

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            model.AssertHasEntitySet("bases", baseType);
            IEdmEntityType baseEntityType = model.AssertHasEntityType(baseType);
            IEdmStructuralProperty key = Assert.Single(baseEntityType.DeclaredKey);
            Assert.Equal(EdmTypeKind.Enum, key.Type.TypeKind());
            Assert.Equal("Microsoft.AspNet.OData.Test.Builder.TestModels.Color", key.Type.Definition.FullTypeName());

            IEdmEntityType derivedEntityType = model.AssertHasEntityType(derivedType, baseType);
            Assert.Null(derivedEntityType.DeclaredKey);
            IEdmProperty derivedId = Assert.Single(derivedEntityType.DeclaredProperties);
            Assert.Equal(EdmTypeKind.Enum, derivedId.Type.TypeKind());
            Assert.Equal("Microsoft.AspNet.OData.Test.Builder.TestModels.Color", derivedId.Type.Definition.FullTypeName());
        }

        [Fact]
        public void ModelBuilder_DerivedComplexTypeHavingKeys_Throws()
        {
            // Arrange
            MockType baseComplexType = new MockType("BaseComplexType")
                .Property(typeof(int), "BaseComplexTypeId");

            MockType derivedComplexType =
                new MockType("DerivedComplexType")
                .Property(typeof(int), "DerivedComplexTypeId")
                .BaseType(baseComplexType);

            MockType entityType =
                new MockType("EntityType")
                .Property(typeof(int), "ID")
                .Property(baseComplexType.Object, "ComplexProperty");

            var configuration = RoutingConfigurationFactory.CreateWithTypes(baseComplexType, derivedComplexType, entityType);
            var builder = ODataConventionModelBuilderFactory.Create(configuration);

            builder.AddEntitySet("entities", builder.AddEntityType(entityType));

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(
                () => builder.GetEdmModel(),
            "Cannot define keys on type 'DefaultNamespace.DerivedComplexType' deriving from 'DefaultNamespace.BaseComplexType'. " +
            "The base type in the entity inheritance hierarchy already contains keys.");
        }

        [Fact]
        public void ModelBuilder_DerivedComplexTypeHavingKeys_SuccedsIfToldToBeComplex()
        {
            MockType baseComplexType = new MockType("BaseComplexType");

            MockType derivedComplexType =
                new MockType("DerivedComplexType")
                .Property(typeof(int), "DerivedComplexTypeId")
                .BaseType(baseComplexType);

            MockType entityType =
                new MockType("EntityType")
                .Property(typeof(int), "ID")
                .Property(baseComplexType.Object, "ComplexProperty");

            var configuration = RoutingConfigurationFactory.CreateWithTypes(baseComplexType, derivedComplexType, entityType);
            var builder = ODataConventionModelBuilderFactory.Create(configuration);

            builder.AddEntitySet("entities", builder.AddEntityType(entityType));
            builder.AddComplexType(baseComplexType);

            IEdmModel model = builder.GetEdmModel();
            Assert.Equal(4, model.SchemaElements.Count());
            Assert.NotNull(model.FindType("DefaultNamespace.EntityType"));
            Assert.NotNull(model.FindType("DefaultNamespace.BaseComplexType"));
            Assert.NotNull(model.FindType("DefaultNamespace.DerivedComplexType"));
        }

        public static TheoryDataSet<MockType> ModelBuilder_PrunesUnReachableTypes_Data
        {
            get
            {
                MockType ignoredType =
                    new MockType("IgnoredType")
                    .Property<int>("Property");

                return new TheoryDataSet<MockType>
                {
                    new MockType("SampleType")
                    .Property<int>("ID")
                    .Property(ignoredType, "IgnoredProperty", new NotMappedAttribute()),

                    new MockType("SampleType")
                    .Property<int>("ID")
                    .Property(
                        new MockType("AnotherType")
                        .Property(ignoredType, "IgnoredProperty"),
                        "IgnoredProperty", new NotMappedAttribute()),

                    new MockType("SampleType")
                    .Property<int>("ID")
                    .Property(
                        new MockType("AnotherType")
                        .Property(ignoredType, "IgnoredProperty", new NotMappedAttribute()),
                        "AnotherProperty")
                };
            }
        }

        [Theory]
        [MemberData(nameof(ModelBuilder_PrunesUnReachableTypes_Data))]
        public void ModelBuilder_PrunesUnReachableTypes(MockType type)
        {
            var modelBuilder = ODataConventionModelBuilderFactory.Create();
            modelBuilder.AddEntityType(type);

            var model = modelBuilder.GetEdmModel();
            Assert.True(model.FindType("DefaultNamespace.IgnoredType") == null);
        }

        [Fact]
        public void ModelBuilder_DeepChainOfComplexTypes()
        {
            var modelBuilder = ODataConventionModelBuilderFactory.Create();

            MockType entityType =
                new MockType("SampleType")
                .Property<int>("ID")
                .Property(
                    new MockType("ComplexType1")
                    .Property(
                        new MockType("ComplexType2")
                        .Property(
                            new MockType("ComplexType3")
                            .Property<int>("Property"),
                            "Property"),
                        "Property"),
                    "Property");

            modelBuilder.AddEntityType(entityType);

            var model = modelBuilder.GetEdmModel();
            Assert.NotNull(model.FindType("DefaultNamespace.SampleType") as IEdmEntityType);
            Assert.NotNull(model.FindType("DefaultNamespace.ComplexType1") as IEdmComplexType);
            Assert.NotNull(model.FindType("DefaultNamespace.ComplexType2") as IEdmComplexType);
            Assert.NotNull(model.FindType("DefaultNamespace.ComplexType3") as IEdmComplexType);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ComplexType_Containing_EntityNavigation_Works(bool optional)
        {
            // Arrange
            MockType entityType = new MockType("EntityType").Property<int>("Id");

            MockType complexType = new MockType("ComplexType");
            if (optional)
            {
                complexType.Property(entityType, "NavProperty");
            }
            else
            {
                complexType.Property(entityType, "NavProperty", new RequiredAttribute());
            }

            var modelBuilder = ODataConventionModelBuilderFactory.Create();
            modelBuilder.AddEntityType(entityType);
            modelBuilder.AddComplexType(complexType);

            // Act
            IEdmModel model = modelBuilder.GetEdmModel();

            // Assert
            // entity
            IEdmEntityType entity = Assert.Single(model.SchemaElements.OfType<IEdmEntityType>());
            Assert.Equal("EntityType", entity.Name);
            IEdmStructuralProperty key = Assert.Single(entity.DeclaredKey);
            Assert.Equal("Id", key.Name);
            Assert.Equal("Edm.Int32", key.Type.FullName());
            IEdmProperty property = Assert.Single(entity.DeclaredProperties);
            Assert.Same(key, property);

            // complex
            IEdmComplexType complex = Assert.Single(model.SchemaElements.OfType<IEdmComplexType>());
            Assert.Equal("ComplexType", complex.Name);

            property = Assert.Single(complex.DeclaredProperties);
            Assert.Equal(EdmPropertyKind.Navigation, property.PropertyKind); // navigation property
            EdmNavigationProperty navProperty = Assert.IsType<EdmNavigationProperty>(property);
            Assert.Equal("NavProperty", navProperty.Name);
            Assert.Equal(optional, navProperty.Type.IsNullable);
            Assert.Same(entity, navProperty.Type.Definition);
        }

        [Fact]
        public void ComplexType_Containing_EntityCollectionNavigation_Works()
        {
            // Arrange
            MockType entityType = new MockType("EntityType").Property<int>("Id");
            MockType complexType = new MockType("ComplexType").Property(entityType.AsCollection(), "CollectionProperty");

            var modelBuilder = ODataConventionModelBuilderFactory.Create();
            modelBuilder.AddEntityType(entityType);
            modelBuilder.AddComplexType(complexType);

            // Act
            IEdmModel model = modelBuilder.GetEdmModel();

            // Assert
            // entity
            IEdmEntityType entity = Assert.Single(model.SchemaElements.OfType<IEdmEntityType>());
            Assert.Equal("EntityType", entity.Name);
            IEdmStructuralProperty key = Assert.Single(entity.DeclaredKey);
            Assert.Equal("Id", key.Name);
            Assert.Equal("Edm.Int32", key.Type.FullName());
            IEdmProperty property = Assert.Single(entity.DeclaredProperties);
            Assert.Same(key, property);

            // complex
            IEdmComplexType complex = Assert.Single(model.SchemaElements.OfType<IEdmComplexType>());
            Assert.Equal("ComplexType", complex.Name);

            property = Assert.Single(complex.DeclaredProperties);
            Assert.Equal(EdmPropertyKind.Navigation, property.PropertyKind); // navigation property
            EdmNavigationProperty navProperty = Assert.IsType<EdmNavigationProperty>(property);
            Assert.Equal("CollectionProperty", navProperty.Name);
            Assert.True(navProperty.Type.IsCollection());
            Assert.Same(entity, navProperty.Type.AsCollection().ElementType().Definition);
        }

        [Fact]
        public void ComplexType_Containing_ComplexCollection_works()
        {
            Type complexType =
                new MockType("ComplexTypeWithComplexCollection")
                .Property<Version[]>("CollectionProperty");

            var modelBuilder = ODataConventionModelBuilderFactory.Create();
            modelBuilder.AddComplexType(complexType);

            var model = modelBuilder.GetEdmModel();

            IEdmComplexType complexEdmType = model.AssertHasComplexType(complexType);
            model.AssertHasComplexType(typeof(Version));
            var collectionProperty = complexEdmType.DeclaredProperties.Where(p => p.Name == "CollectionProperty").SingleOrDefault();
            Assert.NotNull(collectionProperty);
            Assert.True(collectionProperty.Type.IsCollection());
            Assert.Equal("System.Version", collectionProperty.Type.AsCollection().ElementType().FullName());
        }

        [Fact]
        public void EntityType_Containing_ComplexCollection_Works()
        {
            Type entityType =
                new MockType("EntityTypeWithComplexCollection")
                .Property<int>("ID")
                .Property<Version[]>("CollectionProperty");

            var modelBuilder = ODataConventionModelBuilderFactory.Create();
            modelBuilder.AddEntityType(entityType);

            var model = modelBuilder.GetEdmModel();

            IEdmEntityType entityEdmType = model.AssertHasEntityType(entityType);
            model.AssertHasComplexType(typeof(Version));
            var collectionProperty = entityEdmType.DeclaredProperties.Where(p => p.Name == "CollectionProperty").SingleOrDefault();
            Assert.NotNull(collectionProperty);
            Assert.True(collectionProperty.Type.IsCollection());
            Assert.Equal("System.Version", collectionProperty.Type.AsCollection().ElementType().FullName());
        }

        [Fact]
        public void EntityType_Containing_ComplexTypeContainingComplexCollection_Works()
        {
            Type complexTypeWithComplexCollection =
                new MockType("ComplexType")
                .Property<Version[]>("ComplexCollectionProperty");

            Type entityType =
                new MockType("EntityTypeWithComplexCollection")
                .Property<int>("ID")
                .Property(complexTypeWithComplexCollection, "ComplexProperty");

            var modelBuilder = ODataConventionModelBuilderFactory.Create();
            modelBuilder.AddEntityType(entityType);

            var model = modelBuilder.GetEdmModel();

            IEdmEntityType entityEdmType = model.AssertHasEntityType(entityType);
            model.AssertHasComplexType(typeof(Version));
            IEdmComplexType edmComplexType = model.AssertHasComplexType(complexTypeWithComplexCollection);

            var collectionProperty = edmComplexType.DeclaredProperties.Where(p => p.Name == "ComplexCollectionProperty").SingleOrDefault();
            Assert.NotNull(collectionProperty);
            Assert.True(collectionProperty.Type.IsCollection());
            Assert.Equal("System.Version", collectionProperty.Type.AsCollection().ElementType().FullName());
        }

        [Fact]
        public void ModelBuilder_Doesnot_Override_NavigationPropertyConfiguration()
        {
            MockType type1 =
                new MockType("Entity1")
                .Property<int>("ID");

            MockType type2 =
                new MockType("Entity2")
                .Property<int>("ID")
                .Property(type1, "Relation");

            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.AddEntityType(type2).AddNavigationProperty(type2.GetProperty("Relation"), EdmMultiplicity.One);

            IEdmModel model = builder.GetEdmModel();
            IEdmEntityType entity = model.AssertHasEntityType(type2);

            entity.AssertHasNavigationProperty(model, "Relation", type1, isNullable: false, multiplicity: EdmMultiplicity.One);
        }

        [Fact]
        public void ODataConventionModelBuilder_IgnoresIndexerProperties()
        {
            MockType type =
                new MockType("ComplexType")
                .Property<int>("Item");

            MockPropertyInfo pi = type.GetProperty("Item");
            pi.Setup(p => p.GetIndexParameters()).Returns(new[] { new Mock<ParameterInfo>().Object }); // make it indexer

            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.AddComplexType(type);

            IEdmModel model = builder.GetEdmModel();
            IEdmComplexType complexType = model.AssertHasComplexType(type);
            Assert.Empty(complexType.Properties());
        }

        [Fact]
        public void CanBuildModelForAnonymousTypes()
        {
            Type entityType = new
            {
                ID = default(int),
                NavigationCollection = new[]
                {
                    new { ID = default(int) }
                }
            }.GetType();

            var configuration = RoutingConfigurationFactory.Create();
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create(configuration, isQueryCompositionMode: true);
            builder.AddEntitySet("entityset", builder.AddEntityType(entityType));

            IEdmModel model = builder.GetEdmModel();

            IEdmEntityType entity = model.AssertHasEntitySet("entityset", entityType);
            entity.AssertHasKey(model, "ID", EdmPrimitiveTypeKind.Int32);
            entity.AssertHasNavigationProperty(model, "NavigationCollection", new { ID = default(int) }.GetType(), isNullable: false, multiplicity: EdmMultiplicity.Many);
        }

        [Theory]
        [InlineData(typeof(object[]))]
        [InlineData(typeof(IEnumerable<object>))]
        [InlineData(typeof(List<object>))]
        public void ObjectCollectionsAreIgnoredByDefault(Type propertyType)
        {
            MockType type =
                new MockType("entity")
                .Property<int>("ID")
                .Property(propertyType, "Collection");

            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            var entityType = builder.AddEntityType(type);
            builder.AddEntitySet("entityset", entityType);

            IEdmModel model = builder.GetEdmModel();
            Assert.Equal(2, model.SchemaElements.Count());
            var entityEdmType = model.AssertHasEntitySet("entityset", type);
        }

        [Fact]
        public void CanMapObjectArrayAsAComplexProperty()
        {
            MockType type =
                new MockType("entity")
                .Property<int>("ID")
                .Property<object[]>("Collection");

            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            var entityType = builder.AddEntityType(type);
            entityType.AddCollectionProperty(type.GetProperty("Collection"));
            builder.AddEntitySet("entityset", entityType);

            IEdmModel model = builder.GetEdmModel();
            Assert.Equal(3, model.SchemaElements.Count());
            var entityEdmType = model.AssertHasEntitySet("entityset", type);
            model.AssertHasComplexType(typeof(object));
            entityEdmType.AssertHasCollectionProperty(model, "Collection", typeof(object), isNullable: true);
        }

        [Fact]
        public void ComplexTypes_In_Inheritance_AreFlattened()
        {
            // Arrange
            MockType complexBase =
                new MockType("ComplexBase")
                .Property<int>("BaseProperty");

            MockType complexDerived =
                new MockType("ComplexDerived")
                .BaseType(complexBase)
                .Property<int>("DerivedProperty");

            MockType entity =
                new MockType("entity")
                .Property<int>("ID")
                .Property(complexBase, "ComplexBase")
                .Property(complexDerived, "ComplexDerived");

            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.AddEntityType(entity);

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.Equal(4, model.SchemaElements.Count());
            model.AssertHasEntityType(entity);

            var complexBaseEdmType = model.AssertHasComplexType(complexBase);
            complexBaseEdmType.AssertHasPrimitiveProperty(model, "BaseProperty", EdmPrimitiveTypeKind.Int32, isNullable: false);

            var complexDerivedEdmType = model.AssertHasComplexType(complexDerived);
            complexDerivedEdmType.AssertHasPrimitiveProperty(model, "BaseProperty", EdmPrimitiveTypeKind.Int32, isNullable: false);
            complexDerivedEdmType.AssertHasPrimitiveProperty(model, "DerivedProperty", EdmPrimitiveTypeKind.Int32, isNullable: false);
        }

        [Fact]
        public void DerivedComplexTypes_AreFlattened()
        {
            // Arrange
            MockType complexBase = new MockType("ComplexBase").Property<string>("BaseProperty");
            MockType complexDerived = new MockType("ComplexBase").BaseType(complexBase).Property<int>("DerivedProperty");

            var configuration = RoutingConfigurationFactory.CreateWithTypes(complexBase, complexDerived);
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create(configuration);
            builder.AddComplexType(complexBase);

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.Equal(3, model.SchemaElements.Count());

            IEdmComplexType complexBaseEdmType = model.AssertHasComplexType(complexBase);
            complexBaseEdmType.AssertHasPrimitiveProperty(model, "BaseProperty", EdmPrimitiveTypeKind.String, true);

            IEdmComplexType complexDerivedEdmType = model.AssertHasComplexType(complexDerived, complexBase);
            complexDerivedEdmType.AssertHasPrimitiveProperty(model, "BaseProperty", EdmPrimitiveTypeKind.String, true);
            complexDerivedEdmType.AssertHasPrimitiveProperty(model, "DerivedProperty", EdmPrimitiveTypeKind.Int32, false);
        }

        [Fact]
        public void OnModelCreating_IsInvoked_AfterConventionsAreRun()
        {
            // Arrange
            MockType entity =
                new MockType("entity")
                .Property<int>("ID");

            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.AddEntitySet("entities", builder.AddEntityType(entity));
            builder.OnModelCreating = (modelBuilder) =>
                {
                    var entityConfiguration = modelBuilder.StructuralTypes.OfType<EntityTypeConfiguration>().Single();
                    Assert.Single(entityConfiguration.Keys);
                    var key = entityConfiguration.Keys.Single();
                    Assert.Equal("ID", key.Name);

                    // mark the key as optional just to verify later.
                    key.OptionalProperty = true;
                };

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.True(model.SchemaElements.OfType<IEdmEntityType>().Single().Key().Single().Type.IsNullable);
        }

        [Fact]
        public void IgnoredPropertyOnBaseType_DoesnotShowupOnDerivedType()
        {
            // Arrange
            var baseType =
                new MockType("BaseType")
                .Property<int>("BaseTypeProperty");

            var derivedType =
                new MockType("DerivedType")
                .BaseType(baseType)
                .Property<int>("DerivedTypeProperty");

            var configuration = RoutingConfigurationFactory.CreateWithTypes(baseType, derivedType);
            var builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataConventionModelBuilder>(configuration);

            // Act
            var baseEntity = builder.AddEntityType(baseType);
            baseEntity.RemoveProperty(baseType.GetProperty("BaseTypeProperty"));
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmEntityType baseEntityType = model.AssertHasEntityType(derivedType);
            Assert.DoesNotContain("BaseTypeProperty", baseEntityType.Properties().Select(p => p.Name));
            IEdmEntityType derivedEntityType = model.AssertHasEntityType(derivedType);
            Assert.DoesNotContain("BaseTypeProperty", derivedEntityType.Properties().Select(p => p.Name));
        }

        [Fact]
        public void IgnoredPropertyOnBaseComplexType_DoesnotShowupOnDerivedComplexType()
        {
            // Arrange
            MockType baseType = new MockType("BaseType").Property<int>("BaseProperty");
            MockType derivedType = new MockType("DerivedType").BaseType(baseType).Property<int>("DerivedProperty");

            var configuration = RoutingConfigurationFactory.CreateWithTypes(baseType, derivedType);
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create(configuration);

            // Act
            ComplexTypeConfiguration baseComplex = builder.AddComplexType(baseType);
            baseComplex.RemoveProperty(baseType.GetProperty("BaseProperty"));
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmComplexType baseComplexType = model.AssertHasComplexType(baseType);
            Assert.DoesNotContain("BaseProperty", baseComplexType.Properties().Select(p => p.Name));

            IEdmComplexType derivedComplexType = model.AssertHasComplexType(derivedType, baseType);
            Assert.DoesNotContain("BaseProperty", derivedComplexType.Properties().Select(p => p.Name));
        }

        [Fact]
        public void ODataConventionModelBuilder_Sets_IsAddedExplicitly_Appropriately()
        {
            // Arrange
            MockType relatedEntity =
                new MockType("RelatedEntity")
                .Property<int>("ID");
            MockType relatedComplexType =
                new MockType("RelatedComplexType");
            MockType type =
                new MockType()
                .Property<int>("ID")
                .Property<int>("ExplicitlyAddedPrimitive")
                .Property<int>("InferredPrimitive")
                .Property<int[]>("ExplicitlyAddedPrimitiveCollection")
                .Property<int[]>("InferredAddedPrimitiveCollection")
                .Property(relatedComplexType, "ExplicitlyAddedComplex")
                .Property(relatedComplexType, "InferredComplex")
                .Property(relatedComplexType.AsCollection(), "ExplicitlyAddedComplexCollection")
                .Property(relatedComplexType.AsCollection(), "InferredComplexCollection")
                .Property(relatedEntity, "ExplicitlyAddedNavigation")
                .Property(relatedEntity, "InferredNavigation")
                .Property(relatedEntity.AsCollection(), "ExplicitlyAddedNavigationCollection")
                .Property(relatedEntity.AsCollection(), "InferredNavigationCollection");

            var builder = ODataConventionModelBuilderFactory.Create();
            var entity = builder.AddEntityType(type);
            entity.AddProperty(type.GetProperty("ExplicitlyAddedPrimitive"));
            entity.AddCollectionProperty(type.GetProperty("ExplicitlyAddedPrimitiveCollection"));
            entity.AddComplexProperty(type.GetProperty("ExplicitlyAddedComplex"));
            entity.AddCollectionProperty(type.GetProperty("ExplicitlyAddedComplexCollection"));
            entity.AddNavigationProperty(type.GetProperty("ExplicitlyAddedNavigation"), EdmMultiplicity.ZeroOrOne);
            entity.AddNavigationProperty(type.GetProperty("ExplicitlyAddedNavigationCollection"), EdmMultiplicity.Many);

            builder.OnModelCreating = (b) =>
                {
                    var explicitlyAddedProperties = entity.Properties.Where(p => p.Name.Contains("ExplicitlyAdded"));
                    var inferredProperties = entity.Properties.Where(p => p.Name.Contains("Inferred"));

                    Assert.Equal(13, entity.Properties.Count());
                    Assert.Equal(6, explicitlyAddedProperties.Count());
                    Assert.Equal(6, inferredProperties.Count());
                    foreach (var explicitlyAddedProperty in explicitlyAddedProperties)
                    {
                        Assert.True(explicitlyAddedProperty.AddedExplicitly);
                    }
                    foreach (var inferredProperty in inferredProperties)
                    {
                        Assert.False(inferredProperty.AddedExplicitly);
                    }
                };

            ExceptionAssert.DoesNotThrow(() => builder.GetEdmModel());
        }

        [Fact]
        public void ODataConventionModelBuilder_GetEdmModel_HasContainment()
        {
            // Arrange
            var builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<MyOrder>("MyOrders");

            // Act & Assert
            IEdmModel model = builder.GetEdmModel();
            Assert.NotNull(model);

            var container = Assert.Single(model.SchemaElements.OfType<IEdmEntityContainer>());

            var myOrders = container.FindEntitySet("MyOrders");
            Assert.NotNull(myOrders);
            var myOrder = myOrders.EntityType();
            Assert.Equal("MyOrder", myOrder.Name);
            ODataModelBuilderTest.AssertHasContainment(myOrder, model);
        }

        [Fact]
        public void ODataConventionModelBuilder_GetEdmModel_DerivedTypeHasContainment()
        {
            // Arrange
            var builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<MySpecialOrder>("MySpecialOrders");

            // Act & Assert
            IEdmModel model = builder.GetEdmModel();
            Assert.NotNull(model);

            var container = Assert.Single(model.SchemaElements.OfType<IEdmEntityContainer>());

            var myOrders = container.FindEntitySet("MySpecialOrders");
            Assert.NotNull(myOrders);
            var myOrder = myOrders.EntityType();
            Assert.Equal("MySpecialOrder", myOrder.Name);
            ODataModelBuilderTest.AssertHasContainment(myOrder, model);
            ODataModelBuilderTest.AssertHasAdditionalContainment(myOrder, model);
        }

        [Fact]
        public void ODataConventionModelBuilder_SetIdWithTypeNamePrefixAsKey_IfNoKeyAttribute()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntityType<EntityKeyConventionTests_Album1>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmEntityType entityType = model.AssertHasEntityType(typeof(EntityKeyConventionTests_Album1));
            IEdmStructuralProperty keyProperty = Assert.Single(entityType.Key());
            Assert.Equal("EntityKeyConventionTests_Album1Id", keyProperty.Name);
            Assert.True(keyProperty.Type.IsInt64());
        }

        [Fact]
        public void ODataConventionModelBuilder_SetIdAsKey_IfNoKeyAttribute()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntityType<EntityKeyConventionTests_Album2>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmEntityType entityType = model.AssertHasEntityType(typeof(EntityKeyConventionTests_Album2));
            IEdmStructuralProperty keyProperty = Assert.Single(entityType.Key());
            Assert.Equal("Id", keyProperty.Name);
            Assert.True(keyProperty.Type.IsInt32());
        }

        [Fact]
        public void ODataConventionModelBuilder_EntityKeyConvention_DoesNothing_IfKeyAttribute()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntityType<EntityKeyConventionTests_AlbumWithKey>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmEntityType entityType = model.AssertHasEntityType(typeof(EntityKeyConventionTests_AlbumWithKey));
            IEdmStructuralProperty keyProperty = Assert.Single(entityType.Key());
            Assert.Equal("Path", keyProperty.Name);
            Assert.True(keyProperty.Type.IsString());
        }

        [Fact]
        public void ODataConventionModelBuilder_MappedDerivedTypeHasNoAliasedBaseProperties()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            EntitySetConfiguration<BaseEmployee> employees = builder.EntitySet<BaseEmployee>("Employees");
            EntityTypeConfiguration<BaseEmployee> employee = employees.EntityType;
            employee.EnumProperty<Gender>(e => e.Sex).Name = "gender";
            employee.Ignore(e => e.FullName);

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmEntityType derivedEntityType = model.AssertHasEntityType(typeof(DerivedManager));
            IEdmProperty property = Assert.Single(derivedEntityType.DeclaredProperties);
            Assert.Equal("Heads", property.Name);
        }

        [Fact]
        public void ModelBuilder_MediaTypeAttribute()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntityType<Vehicle>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.True(model.AssertHasEntityType(typeof(Vehicle)).HasStream);
        }

        [Fact]
        public void ODataConventionModelBuilder_RequiredAttribute_WorksOnComplexTypeProperty()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<RequiredEmployee>("Employees");

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmEntityType entityType = model.AssertHasEntityType(typeof(RequiredEmployee));
            IEdmProperty property = Assert.Single(entityType.DeclaredProperties.Where(e => e.Name == "Address"));
            Assert.False(property.Type.IsNullable);
        }

        [Fact]
        public void ODataConventionModelBuilder_QueryLimitAttributes_WorksOnComplexTypeProperty()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<QueryLimitEmployee>("Employees");

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmEntityType entityType = model.AssertHasEntityType(typeof(QueryLimitEmployee));
            IEdmProperty property = Assert.Single(entityType.DeclaredProperties.Where(e => e.Name == "Address"));

            Assert.True(EdmLibHelpers.IsNotFilterable(property, null, null, model, true));
            Assert.True(EdmLibHelpers.IsNotSortable(property, null, null, model, true));
        }

        [Fact]
        public void ODataConventionModelBuilder_ForeignKeyAttribute_WorksOnNavigationProperty()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<ForeignKeyCustomer>("Customers");

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.NotNull(model);
            IEdmEntityType orderType = model.AssertHasEntityType(typeof(ForeignKeyOrder));

            IEdmNavigationProperty navigation = orderType.AssertHasNavigationProperty(model, "Customer",
                typeof(ForeignKeyCustomer), true, EdmMultiplicity.ZeroOrOne);

            IEdmStructuralProperty dependentProperty = Assert.Single(navigation.DependentProperties());
            Assert.Equal("CustomerId", dependentProperty.Name);

            IEdmStructuralProperty principalProperty = Assert.Single(navigation.PrincipalProperties());
            Assert.Equal("Id", principalProperty.Name);
        }

        [Fact]
        public void ODataConventionModelBuilder_ForeignKeyDiscovery_WorksOnNavigationProperty()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<ForeignKeyCustomer>("Customers");

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.NotNull(model);
            IEdmEntityType categoryType = model.AssertHasEntityType(typeof(ForeignKeyCategory));

            IEdmNavigationProperty navigation = categoryType.AssertHasNavigationProperty(model, "Customer",
                typeof(ForeignKeyCustomer), true, EdmMultiplicity.ZeroOrOne);

            IEdmStructuralProperty dependentProperty = Assert.Single(navigation.DependentProperties());
            Assert.Equal("ForeignKeyCustomerId", dependentProperty.Name);

            IEdmStructuralProperty principalProperty = Assert.Single(navigation.PrincipalProperties());
            Assert.Equal("Id", principalProperty.Name);
        }

        [Theory]
        [InlineData(typeof(ForeignKeyCustomer))]
        [InlineData(typeof(ForeignKeyVipCustomer))]
        public void ODataConventionModelBuilder_ForeignKeyAttribute_WorksOnNavigationProperty_PrincipalOnBaseType(Type entityType)
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.AddEntityType(entityType);

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.NotNull(model);
            IEdmEntityType orderType = model.AssertHasEntityType(typeof(ForeignKeyVipOrder));

            IEdmNavigationProperty navigation = orderType.AssertHasNavigationProperty(model, "VipCustomer",
                typeof(ForeignKeyVipCustomer), true, EdmMultiplicity.ZeroOrOne);

            IEdmStructuralProperty dependentProperty = Assert.Single(navigation.DependentProperties());
            Assert.Equal("CustomerId", dependentProperty.Name);

            IEdmStructuralProperty principalProperty = Assert.Single(navigation.PrincipalProperties());
            Assert.Equal("Id", principalProperty.Name);
        }

        [Theory]
        [InlineData(typeof(ForeignKeyCustomer))]
        [InlineData(typeof(ForeignKeyVipCustomer))]
        public void ODataConventionModelBuilder_ForeignKeyDiscovery_WorksOnNavigationProperty_PrincipalOnBaseType(Type entityType)
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.AddEntityType(entityType);

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.NotNull(model);
            IEdmEntityType categoryType = model.AssertHasEntityType(typeof(ForeignKeyVipCatogory));

            IEdmNavigationProperty navigation = categoryType.AssertHasNavigationProperty(model, "VipCustomer",
                typeof(ForeignKeyVipCustomer), true, EdmMultiplicity.ZeroOrOne);

            IEdmStructuralProperty dependentProperty = Assert.Single(navigation.DependentProperties());
            Assert.Equal("ForeignKeyVipCustomerId", dependentProperty.Name);

            IEdmStructuralProperty principalProperty = Assert.Single(navigation.PrincipalProperties());
            Assert.Equal("Id", principalProperty.Name);
        }

        [Fact]
        public void AddDynamicDictionary_ThrowsException_IfMoreThanOneDynamicPropertyInOpenEntityType()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntityType<BadOpenEntityType>();

            // Act & Assert
#if NETCOREAPP3_1
            ExceptionAssert.ThrowsArgument(() => builder.GetEdmModel(),
                "propertyInfo",
                "Found more than one dynamic property container in type 'BadOpenEntityType'. " +
                "Each open type must have at most one dynamic property container. (Parameter 'propertyInfo')");
#else
            ExceptionAssert.ThrowsArgument(() => builder.GetEdmModel(),
                "propertyInfo",
                "Found more than one dynamic property container in type 'BadOpenEntityType'. " +
                "Each open type must have at most one dynamic property container.\r\n" +
                "Parameter name: propertyInfo");
#endif
        }

        [Fact]
        public void AddDynamicDictionary_ThrowsException_IfBaseAndDerivedHasDynamicPropertyDictionary()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntityType<BadBaseOpenEntityType>();

            // Act & Assert
#if NETCOREAPP3_1
            ExceptionAssert.ThrowsArgument(() => builder.GetEdmModel(),
                "propertyInfo",
                "Found more than one dynamic property container in type 'BadDerivedOpenEntityType'. " +
                "Each open type must have at most one dynamic property container. (Parameter 'propertyInfo')");
#else
            ExceptionAssert.ThrowsArgument(() => builder.GetEdmModel(),
                "propertyInfo",
                "Found more than one dynamic property container in type 'BadDerivedOpenEntityType'. " +
                "Each open type must have at most one dynamic property container.\r\n" +
                "Parameter name: propertyInfo");
#endif
        }

        [Fact]
        public void GetEdmModel_Works_ForDateTime()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntityType<DateTimeModel>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.NotNull(model);
            IEdmEntityType entityType = Assert.Single(model.SchemaElements.OfType<IEdmEntityType>());
            Assert.Equal("DateTimeModel", entityType.Name);

            IEdmProperty edmProperty = Assert.Single(entityType.Properties().Where(e => e.Name == "BirthdayA"));
            Assert.Equal("Edm.DateTimeOffset", edmProperty.Type.FullName());
            Assert.False(edmProperty.Type.IsNullable);

            edmProperty = Assert.Single(entityType.Properties().Where(e => e.Name == "BirthdayB"));
            Assert.Equal("Edm.DateTimeOffset", edmProperty.Type.FullName());
            Assert.True(edmProperty.Type.IsNullable);

            edmProperty = Assert.Single(entityType.Properties().Where(e => e.Name == "BirthdayC"));
            Assert.Equal("Collection(Edm.DateTimeOffset)", edmProperty.Type.FullName());
            Assert.False(edmProperty.Type.IsNullable);

            edmProperty = Assert.Single(entityType.Properties().Where(e => e.Name == "BirthdayD"));
            Assert.Equal("Collection(Edm.DateTimeOffset)", edmProperty.Type.FullName());
            Assert.True(edmProperty.Type.IsNullable);
        }

        [Fact]
        public void GetEdmModel_WorksOnConventionModelBuilder_ForOpenEntityType()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntityType<EntityTypeTest.SimpleOpenEntityType>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.NotNull(model);
            IEdmEntityType entityType = Assert.Single(model.SchemaElements.OfType<IEdmEntityType>());
            Assert.True(entityType.IsOpen);
            Assert.Equal(2, entityType.Properties().Count());

            Assert.True(entityType.Properties().Where(c => c.Name == "Id").Any());
            Assert.True(entityType.Properties().Where(c => c.Name == "Name").Any());
        }

        [Fact]
        public void GetEdmModel_WorksOnConventionModelBuilder_ForDerivedOpenEntityType()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntityType<BaseOpenEntityType>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.NotNull(model);
            IEdmEntityType baseEntityType =
                Assert.Single(model.SchemaElements.OfType<IEdmEntityType>().Where(c => c.Name == "BaseOpenEntityType"));
            Assert.True(baseEntityType.IsOpen);
            Assert.Single(baseEntityType.Properties());

            IEdmEntityType derivedEntityType =
                Assert.Single(model.SchemaElements.OfType<IEdmEntityType>().Where(c => c.Name == "DerivedOpenEntityType"));
            Assert.True(derivedEntityType.IsOpen);
            Assert.Equal(2, derivedEntityType.Properties().Count());

            DynamicPropertyDictionaryAnnotation baseDynamicPropertyAnnotation =
                model.GetAnnotationValue<DynamicPropertyDictionaryAnnotation>(baseEntityType);

            DynamicPropertyDictionaryAnnotation derivedDynamicPropertyAnnotation =
                model.GetAnnotationValue<DynamicPropertyDictionaryAnnotation>(derivedEntityType);

            Assert.Equal(baseDynamicPropertyAnnotation.PropertyInfo.Name, derivedDynamicPropertyAnnotation.PropertyInfo.Name);
        }

        [Fact]
        public void GetEdmModel_WorksOnConventionModelBuilder_ForBaseEntityType_DerivedOpenEntityType()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntityType<BaseEntityType>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.NotNull(model);
            IEdmEntityType baseEntityType =
                Assert.Single(model.SchemaElements.OfType<IEdmEntityType>().Where(c => c.Name == "BaseEntityType"));
            Assert.False(baseEntityType.IsOpen);
            Assert.Single(baseEntityType.Properties());

            IEdmEntityType derivedEntityType =
                Assert.Single(model.SchemaElements.OfType<IEdmEntityType>().Where(c => c.Name == "DerivedEntityType"));
            Assert.True(derivedEntityType.IsOpen);
            Assert.Equal(2, derivedEntityType.Properties().Count());
        }

        [Fact]
        public void GetEdmModel_Works_ForOpenEntityTypeWithDerivedDynamicProperty()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntityType<OpenEntityTypeWithDerivedDynamicProperty>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.NotNull(model);
            IEdmEntityType entityType = Assert.Single(model.SchemaElements.OfType<IEdmEntityType>());
            Assert.True(entityType.IsOpen);
            IEdmProperty edmProperty = Assert.Single(entityType.Properties());
            Assert.Equal("Id", edmProperty.Name);
        }

        [Fact]
        public void GetEdmModel_Works_ForRecursiveLoopOfComplexType()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.ComplexType<RecursiveEmployee>();

            // Act & Assert
            IEdmModel model = builder.GetEdmModel();
            var employeeType = model.SchemaElements.OfType<IEdmComplexType>().SingleOrDefault(se => se.Name == "RecursiveEmployee");
            Assert.NotNull(employeeType);
            var managerProperty = employeeType.FindProperty("Manager");
            Assert.NotNull(managerProperty);
            Assert.Equal(employeeType.FullName(), managerProperty.Type.AsComplex().ComplexDefinition().FullName());
        }

        [Fact]
        public void GetEdmModel_CreatesNavigationPropertyBindings_ForRecursiveLoopOfComplexTypes()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<CountryDetails>("countryDetails");
            builder.EntitySet<Organization>("orgs");

            // Act & Assert
            IEdmModel model = builder.GetEdmModel();

            model.AssertHasEntitySet("orgs", typeof(Organization));
            IEdmEntitySet companies = model.FindDeclaredEntitySet("orgs");

            Assert.Collection(
                companies.NavigationPropertyBindings,
                nav => Assert.Equal("HeadquartersAddress/CountryDetails", nav.Path.Path));
        }

        [Fact]
        public void GetEdmModel_CreatesNavigationPropertyBindings_ForMutuallyRecursiveLoopOfComplexTypes()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<Car>("cars");
            builder.EntitySet<Person>("people");

            // Act & Assert
            IEdmModel model = builder.GetEdmModel();

            model.AssertHasEntitySet("people", typeof(Person));
            IEdmEntitySet people = model.FindDeclaredEntitySet("people");

            Assert.Collection(
                people.NavigationPropertyBindings,
                nav => Assert.Equal("ResponsibleGuardian/Microsoft.AspNet.OData.Test.Builder.TestModels.Child/Car", nav.Path.Path),
                nav => Assert.Equal("ResponsibleGuardian/OnlyChild/Car", nav.Path.Path));
        }

        [Fact]
        public void ConventionModelBuilder_Work_With_ExpilitPropertyDeclare()
        {
            // Arrange
            var builder = ODataConventionModelBuilderFactory.Create();

            // Act 
            var user = builder.EntitySet<IdentityUser>("IdentityUsers");
            user.EntityType.HasKey(p => new { p.Provider, p.UserId });
            user.EntityType.Property(p => p.Name).IsOptional();
            user.EntityType.ComplexProperty<ProductVersion>(p => p.ProductVersion).IsRequired();
            user.EntityType.EnumProperty<UserType>(p => p.UserType).IsRequired();
            user.EntityType.CollectionProperty(p => p.Contacts).IsRequired();
            var edmModel = builder.GetEdmModel();

            // Assert
            Assert.NotNull(edmModel);
            IEdmEntityType entityType = edmModel.SchemaElements.OfType<IEdmEntityType>().First();
            Assert.Equal(6, entityType.Properties().Count());
            Assert.Equal(2, entityType.Key().Count());
            Assert.True(entityType.Properties().First(p => p.Name.Equals("Name")).Type.IsNullable);
            Assert.False(entityType.Properties().First(p => p.Name.Equals("ProductVersion")).Type.IsNullable);
            Assert.False(entityType.Properties().First(p => p.Name.Equals("UserType")).Type.IsNullable);
            Assert.False(entityType.Properties().First(p => p.Name.Equals("Contacts")).Type.IsNullable);
        }

        [Fact]
        public void ConventionModelBuild_Work_With_AutoExpandEdmTypeAttribute()
        {
            // Arrange
            var modelBuilder = ODataConventionModelBuilderFactory.Create();
            var entityTypeConf = modelBuilder.EntityType<Product>();

            // Act 
            modelBuilder.EntitySet<Product>("Products");
            var model = modelBuilder.GetEdmModel();

            // Assert
            Assert.NotNull(model);
            NavigationPropertyConfiguration category = entityTypeConf.NavigationProperties.FirstOrDefault(p => p.Name == "Category");
            Assert.NotNull(category);
            Assert.True(category.AutoExpand);
        }

        [Fact]
        public void CanConfig_SystemCultureInfo_AsEntityType()
        {
            // Arrange
            ODataModelBuilder modelBuilder = ODataConventionModelBuilderFactory.Create();
            modelBuilder.EntityType<CultureInfo>().HasKey(c => c.LCID);

            // Act
            IEdmModel model = modelBuilder.GetEdmModel();

            // Assert
            Assert.NotNull(model);

            Assert.Equal(24, model.SchemaElements.Count());

            // only one entity type.
            var entityTypes = model.SchemaElements.OfType<IEdmEntityType>();
            IEdmEntityType cultureInfo = Assert.Single(entityTypes);
            Assert.NotNull(cultureInfo);
            Assert.Equal(EdmTypeKind.Entity, cultureInfo.TypeKind);
            Assert.Equal("System.Globalization.CultureInfo", cultureInfo.FullTypeName());

            IEdmComplexType calendar = model.SchemaElements.OfType<IEdmComplexType>()
                .FirstOrDefault(e => e.FullTypeName() == "System.Globalization.Calendar");
            Assert.NotNull(calendar);
            Assert.Equal(EdmTypeKind.Complex, calendar.TypeKind);
            Assert.True(calendar.IsAbstract);

            IEdmComplexType gregorianCalendar  = model.SchemaElements.OfType<IEdmComplexType>()
                .FirstOrDefault(e => e.FullTypeName() == "System.Globalization.GregorianCalendar");
            Assert.NotNull(gregorianCalendar);
            Assert.Equal(EdmTypeKind.Complex, gregorianCalendar.TypeKind);
            Assert.False(gregorianCalendar.IsAbstract);
            Assert.Equal(calendar, gregorianCalendar.BaseType);
        }

        [Fact]
        public void CanConfig_SystemCultureInfo_AsComplexType()
        {
            // Arrange
            ODataModelBuilder modelBuilder = ODataConventionModelBuilderFactory.Create();
            modelBuilder.ComplexType<CultureInfo>();

            // Act
            IEdmModel model = modelBuilder.GetEdmModel();

            // Assert
            Assert.NotNull(model);

            Assert.Equal(24, model.SchemaElements.Count());
            Assert.Empty(model.SchemaElements.OfType<IEdmEntityType>());

            IEdmComplexType cultureInfo = model.SchemaElements.OfType<IEdmComplexType>()
                .FirstOrDefault(e => e.FullTypeName() == "System.Globalization.CultureInfo");
            Assert.NotNull(cultureInfo);
            Assert.Equal(EdmTypeKind.Complex, cultureInfo.TypeKind);
            Assert.False(cultureInfo.IsAbstract);

            IEdmComplexType calendar = model.SchemaElements.OfType<IEdmComplexType>()
                .FirstOrDefault(e => e.FullTypeName() == "System.Globalization.Calendar");
            Assert.NotNull(calendar);
            Assert.Equal(EdmTypeKind.Complex, calendar.TypeKind);
            Assert.True(calendar.IsAbstract);

            IEdmComplexType gregorianCalendar = model.SchemaElements.OfType<IEdmComplexType>()
                .FirstOrDefault(e => e.FullTypeName() == "System.Globalization.GregorianCalendar");
            Assert.NotNull(gregorianCalendar);
            Assert.Equal(EdmTypeKind.Complex, gregorianCalendar.TypeKind);
            Assert.False(gregorianCalendar.IsAbstract);
            Assert.Equal(calendar, gregorianCalendar.BaseType);
        }

        public class DateTimeRelatedModel
        {
            public int Id { get; set; }

            public DateTime NationalDay { get; set; } // by default to Edm.DateTimeOffset
            public TimeSpan ResumeTime { get; set; }  // by default to Edm.Duration

            [Column(TypeName = "date")]
            public DateTime Birthday { get; set; }

            [Column(TypeName = "Date")]
            public DateTime? PublishDay { get; set; }

            [Column(TypeName = "time")]
            public TimeSpan CreatedTime { get; set; }

            [Column(TypeName = "Time")]
            public TimeSpan? EndTime { get; set; }
        }

        [Fact]
        public void CanConfig_DateTimeRelatedProperties_Correctly()
        {
            // Arrange
            ODataModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntityType<DateTimeRelatedModel>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.NotNull(model);
            IEdmEntityType entityType =
                model.SchemaElements.OfType<IEdmEntityType>().FirstOrDefault(e => e.Name == "DateTimeRelatedModel");
            Assert.NotNull(entityType);

            Assert.Equal(7, entityType.Properties().Count());
            Assert.Single(entityType.Key());

            // by default is Edm.DateTimeOffset
            IEdmProperty property = entityType.Properties().Single(p => p.Name == "NationalDay");
            Assert.False(property.Type.IsNullable);
            Assert.Equal("Edm.DateTimeOffset", property.Type.FullName());

            // by default is Edm.Duration
            property = entityType.Properties().Single(p => p.Name == "ResumeTime");
            Assert.False(property.Type.IsNullable);
            Assert.Equal("Edm.Duration", property.Type.FullName());

            // mapped to Edm.Date
            property = entityType.Properties().Single(p => p.Name == "Birthday");
            Assert.False(property.Type.IsNullable);
            Assert.Equal("Edm.Date", property.Type.FullName());

            property = entityType.Properties().Single(p => p.Name == "PublishDay");
            Assert.True(property.Type.IsNullable);
            Assert.Equal("Edm.Date", property.Type.FullName());

            // mapped to Edm.TimeOfDay
            property = entityType.Properties().Single(p => p.Name == "CreatedTime");
            Assert.False(property.Type.IsNullable);
            Assert.Equal("Edm.TimeOfDay", property.Type.FullName());

            property = entityType.Properties().Single(p => p.Name == "EndTime");
            Assert.True(property.Type.IsNullable);
            Assert.Equal("Edm.TimeOfDay", property.Type.FullName());
        }

        [Fact]
        public void CanConfig_MaxLengthOfStringAndBinaryType()
        {
            // Arrange
            ODataModelBuilder modelBuidler = ODataConventionModelBuilderFactory.Create();
            modelBuidler.EntitySet<MaxLengthEntity>("MaxLengthEntity");
            var entityType = modelBuidler.EntityType<MaxLengthEntity>();

            // Act
            IEdmModel model = modelBuidler.GetEdmModel();
            IEdmEntityType edmEntityType = model.SchemaElements.OfType<IEdmEntityType>().First(p => p.Name == "MaxLengthEntity");
            IEdmStringTypeReference nameType =
                (IEdmStringTypeReference)edmEntityType.DeclaredProperties.First(p => p.Name.Equals("Name")).Type;
            IEdmStringTypeReference nonLengthType =
                (IEdmStringTypeReference)edmEntityType.DeclaredProperties.First(p => p.Name.Equals("NonLength")).Type;

            // Assert
            Assert.NotNull(model);
            var nameProp = entityType.Properties.Where(p => p.Name.Equals("Name")).First();
            Assert.NotNull(nameProp);
            var byteProp = entityType.Properties.Where(p => p.Name.Equals("Byte")).First();
            Assert.NotNull(byteProp);
            var nonLengthProp = entityType.Properties.Where(p => p.Name.Equals("NonLength")).First();
            Assert.NotNull(nonLengthProp);
            Assert.Equal(3, ((LengthPropertyConfiguration)nameProp).MaxLength);
            Assert.Equal(5, ((LengthPropertyConfiguration)byteProp).MaxLength);
            Assert.Null(((LengthPropertyConfiguration)nonLengthProp).MaxLength);
            Assert.Equal(3, nameType.MaxLength);
            Assert.Null(nonLengthType.MaxLength);
        }
    }

    public enum UserType
    {
        Normal = 1,
        Vip = 2
    }

    public class UserBase
    {
        public string Provider { get; set; }

        public string UserId { get; set; }

        public string Name { get; set; }

        public ProductVersion ProductVersion { get; set; }

        public IList<ProductVersion> Contacts { get; set; }

        public UserType UserType { get; set; }
    }

    public class IdentityUser : UserBase
    {
    }

    [AutoExpand]
    public class Product
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public DateTimeOffset? ReleaseDate { get; set; }

        public Date PublishDate { get; set; }

        public TimeOfDay? ShowTime { get; set; }

        public ProductVersion Version { get; set; }

        public Category Category { get; set; }
    }

    public class Category
    {
        public string ID { get; set; }

        public string Name { get; set; }

        public ICollection<Product> Products { get; set; }
    }

    public class ProductVersion
    {
        public int Major { get; set; }

        public int Minor { get; set; }

        [NotMapped]
        public int BuildNumber { get; set; }
    }

    public class ProductWithKeyAttribute
    {
        [Key]
        public int IdOfProduct { get; set; }

        public string Name { get; set; }

        public DateTimeOffset? ReleaseDate { get; set; }

        public ProductVersion Version { get; set; }

        public CategoryWithKeyAttribute Category { get; set; }
    }

    public class ProductWithEnumKeyAttribute
    {
        [Key]
        public Color ProductColor { get; set; }

        public string Name { get; set; }
    }

    public class ProductWithFilterSortable
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public IEnumerable<DateTimeOffset> CountableProperty { get; set; }

        [NotCountable]
        public IEnumerable<DateTimeOffset> NotCountableProperty { get; set; }

        [NotFilterable]
        public string NotFilterableProperty { get; set; }

        [NonFilterable]
        public string NonFilterableProperty { get; set; }

        [NotSortable]
        public string NotSortableProperty { get; set; }

        [Unsortable]
        public string UnsortableProperty { get; set; }

        [NotNavigable]
        [NotExpandable]
        public CategoryWithKeyAttribute Category { get; set; }

        [AutoExpand]
        public CategoryWithKeyAttribute Category2 { get; set; }
    }

    public class CategoryWithKeyAttribute
    {
        [Key]
        public Guid IdOfCategory { get; set; }

        public string Name { get; set; }

        public ICollection<ProductWithKeyAttribute> Products { get; set; }
    }

    [ComplexType]
    public class CategoryWithComplexTypeAttribute
    {
        public int Id { get; set; }
        public string Value { get; set; }
    }

    public class ProductWithCategoryComplexTypeAttribute
    {
        public int Id { get; set; }
        public CategoryWithComplexTypeAttribute Category { get; set; }
    }

    public class ProductWithComplexCollection
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public DateTime? ReleaseDate { get; set; }

        public IEnumerable<Version> Versions { get; set; }
    }

    public class ProductWithPrimitiveCollection
    {
        public int ID { get; set; }

        public string[] Aliases { get; set; }
    }

    public class ProductWithConcurrencyCheckAttribute
    {
        public int ID { get; set; }

        [ConcurrencyCheck]
        public string Name { get; set; }
    }

    public class ProductWithTimestampAttribute
    {
        public int ID { get; set; }

        [Timestamp]
        public string Name { get; set; }
    }

    class EntityKeyConventionTests_Album1
    {
        public string Path { get; set; }
        public long EntityKeyConventionTests_Album1Id { get; set; }
    }

    class EntityKeyConventionTests_Album2
    {
        public string Path { get; set; }
        public int Id { get; set; }
    }

    class EntityKeyConventionTests_AlbumWithKey
    {
        [Key]
        public string Path { get; set; }
        public int Id { get; set; }
    }

    [DataContract]
    public class BaseEmployee
    {
        [DataMember]
        public int ID { get; set; }

        [DataMember(Name = "name")]
        public string FullName { get; set; }

        [DataMember]
        public Gender Sex { get; set; }
    }

    public class DerivedManager : BaseEmployee
    {
        public int Heads { get; set; }
    }

    public class RequiredEmployee
    {
        public int Id { get; set; }

        [Required]
        public EmployeeAddress Address { get; set; }
    }

    public class QueryLimitEmployee
    {
        public int Id { get; set; }

        [NonFilterable]
        [Unsortable]
        public EmployeeAddress Address { get; set; }
    }

    public class EmployeeAddress
    {
        public string City { get; set; }
    }

    public enum Gender
    {
        Male = 1,
        Female = 2
    }

    public class EntityTypeWithDateTime
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public DateTime ReleaseDate { get; set; }
    }

    public class BadOpenEntityType
    {
        public int Id { get; set; }
        public int IntProperty { get; set; }
        public IDictionary<string, object> DynamicProperties1 { get; set; }
        public IDictionary<string, object> DynamicProperties2 { get; set; }
    }

    public class BadBaseOpenEntityType
    {
        public int Id { get; set; }
        public IDictionary<string, object> BaseDynamicProperties { get; set; }
    }

    public class BadDerivedOpenEntityType : BadBaseOpenEntityType
    {
        public int IntProperty { get; set; }
        public IDictionary<string, object> DerivedDynamicProperties { get; set; }
    }

    public class DerivedDynamicPropertyDictionary : Dictionary<string, object>
    { }

    public class OpenEntityTypeWithDerivedDynamicProperty
    {
        public int Id { get; set; }
        public DerivedDynamicPropertyDictionary MyProperties { get; set; }
    }

    public class BaseEntityType
    {
        public int Id { get; set; }
    }

    public class DerivedEntityType : BaseEntityType
    {
        public string Name { get; set; }
        public IDictionary<string, object> DynamicProperties { get; set; }
    }

    public class BaseOpenEntityType
    {
        public int Id { get; set; }
        public IDictionary<string, object> DynamicProperties { get; set; }
    }

    public class DerivedOpenEntityType : BaseOpenEntityType
    {
        public string DerivedProperty { get; set; }
    }

    public class RecursiveEmployee
    {
        public RecursiveEmployee Manager { get; set; }
    }

    public class ForeignKeyCustomer
    {
        public int Id { get; set; }

        public IList<ForeignKeyOrder> Orders { get; set; }

        public IList<ForeignKeyCategory> Categories { get; set; }
    }

    public class ForeignKeyOrder
    {
        public int ForeignKeyOrderId { get; set; }

        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public ForeignKeyCustomer Customer { get; set; }
    }

    public class ForeignKeyCategory
    {
        public int Id { get; set; }

        public int ForeignKeyCustomerId { get; set; }

        public ForeignKeyCustomer Customer { get; set; }
    }

    public class ForeignKeyVipCustomer : ForeignKeyCustomer
    {
        public IList<ForeignKeyVipOrder> VipOrders { get; set; }

        public IList<ForeignKeyVipCatogory> VipCategories { get; set; }
    }

    public class ForeignKeyVipOrder
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public ForeignKeyVipCustomer VipCustomer { get; set; }
    }

    public class ForeignKeyVipCatogory
    {
        public int Id { get; set; }

        public int ForeignKeyVipCustomerId { get; set; }

        public ForeignKeyVipCustomer VipCustomer { get; set; }
    }

    public abstract class AbstractEntityType
    {
    }

    public abstract class BaseAbstractEntityType
    {
    }

    public class DerivedEntityTypeWithOwnKey : BaseAbstractEntityType
    {
        public int DerivedEntityTypeWithOwnKeyId { get; set; }
    }

    public abstract class BaseAbstractEntityType2
    {
    }

    public class SubEntityTypeWithOwnKey : BaseAbstractEntityType2
    {
        public int SubEntityTypeWithOwnKeyId { get; set; }
    }

    public class SubSubEntityTypeWithOwnKey : SubEntityTypeWithOwnKey
    {
        public int SubSubEntityTypeWithOwnKeyId { get; set; }
    }

    public class MaxLengthEntity
    {
        public int Id { get; set; }

        [MaxLength(3)]
        public string Name { get; set; }

        [MaxLength(5)]
        public byte[] Byte { get; set; }

        public string NonLength { get; set; }
    }
}
