// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData.Builder.TestModels;
using System.Web.OData.Formatter;
using System.Web.OData.Query;
using System.Web.OData.TestCommon;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Builder.Conventions
{
    public class ODataConventionModelBuilderTests
    {
        private const int _totalExpectedSchemaTypesForVehiclesModel = 8;

        [Fact]
        public void Ctor_ThrowsForNullConfiguration()
        {
            Assert.ThrowsArgumentNull(
                () => new ODataConventionModelBuilder(configuration: null),
                "configuration");
        }

        [Fact]
        public void Ignore_Should_AddToListOfIgnoredTypes()
        {
            var builder = new ODataConventionModelBuilder();
            builder.Ignore(typeof(object));

            Assert.True(builder.IsIgnoredType(typeof(object)));
        }

        [Fact]
        public void IgnoreOfT_Should_AddToListOfIgnoredTypes()
        {
            var builder = new ODataConventionModelBuilder();
            builder.Ignore<object>();

            Assert.True(builder.IsIgnoredType(typeof(object)));
        }

        [Fact]
        public void CanCallIgnore_MultipleTimes_WithDuplicates()
        {
            var builder = new ODataConventionModelBuilder();
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
            var mockAssembly = new MockAssembly(mockType1, mockType2);

            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Services.Replace(typeof(IAssembliesResolver), new TestAssemblyResolver(mockAssembly));
            var builder = new ODataConventionModelBuilder(configuration);

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
            var mockType3 = new MockType("FooBar").BaseType(mockType2);

            var mockAssembly = new MockAssembly(mockType1, mockType2, mockType3);

            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Services.Replace(typeof(IAssembliesResolver), new TestAssemblyResolver(mockAssembly));
            var builder = new ODataConventionModelBuilder(configuration);

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
            var mockType3 = new MockType("FooBar").BaseType(mockType2).Property<int>("P1").Property<int>("P2");

            var mockAssembly = new MockAssembly(mockType1, mockType2, mockType3);

            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Services.Replace(typeof(IAssembliesResolver), new TestAssemblyResolver(mockAssembly));
            var builder = new ODataConventionModelBuilder(configuration);

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
            var mockType1 = new MockType("FooBar");
            var mockType2 = new MockType("Foo").BaseType(mockType1);
            var mockType3 = new MockType("Fo").BaseType(mockType2);
            var mockType4 = new MockType("Bar").BaseType(mockType1);

            var mockAssembly = new MockAssembly(mockType1, mockType2, mockType3, mockType4);

            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Services.Replace(typeof(IAssembliesResolver), new TestAssemblyResolver(mockAssembly));
            var builder = new ODataConventionModelBuilder(configuration);

            var entity1 = builder.AddEntityType(mockType1);
            builder.MapDerivedTypes(entity1);

            Assert.Equal(
                new[] { "FooBar", "Foo", "Fo", "Bar" }.OrderBy(name => name),
                builder.StructuralTypes.Select(t => t.Name).OrderBy(name => name));
        }

        [Fact]
        public void ModelBuilder_Products()
        {
            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<Product>("Products");
            modelBuilder.Singleton<Product>("Book"); // singleton

            var model = modelBuilder.GetEdmModel();
            Assert.Equal(model.SchemaElements.OfType<IEdmSchemaType>().Count(), 3);

            var product = model.AssertHasEntitySet(entitySetName: "Products", mappedEntityClrType: typeof(Product));
            Assert.Equal(4, product.StructuralProperties().Count());
            Assert.Equal(1, product.NavigationProperties().Count());
            product.AssertHasKey(model, "ID", EdmPrimitiveTypeKind.Int32);
            product.AssertHasPrimitiveProperty(model, "ID", EdmPrimitiveTypeKind.Int32, isNullable: false);
            product.AssertHasPrimitiveProperty(model, "Name", EdmPrimitiveTypeKind.String, isNullable: true);
            product.AssertHasPrimitiveProperty(model, "ReleaseDate", EdmPrimitiveTypeKind.DateTimeOffset, isNullable: true);
            product.AssertHasComplexProperty(model, "Version", typeof(ProductVersion), isNullable: true);
            product.AssertHasNavigationProperty(model, "Category", typeof(Category), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne);

            var singletonProduct = model.AssertHasSingleton(singletonName: "Book", mappedEntityClrType: typeof(Product));
            Assert.Same(singletonProduct, product);

            var category = model.AssertHasEntityType(mappedEntityClrType: typeof(Category));
            Assert.Equal(2, category.StructuralProperties().Count());
            Assert.Equal(1, category.NavigationProperties().Count());
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
            ODataModelBuilder modelBuilder = new ODataConventionModelBuilder();
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
            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<ProductWithKeyAttribute>("Products");

            var model = modelBuilder.GetEdmModel();
            Assert.Equal(model.SchemaElements.OfType<IEdmSchemaType>().Count(), 3);

            var product = model.AssertHasEntitySet(entitySetName: "Products", mappedEntityClrType: typeof(ProductWithKeyAttribute));
            Assert.Equal(4, product.StructuralProperties().Count());
            Assert.Equal(1, product.NavigationProperties().Count());
            product.AssertHasKey(model, "IdOfProduct", EdmPrimitiveTypeKind.Int32);
            product.AssertHasPrimitiveProperty(model, "IdOfProduct", EdmPrimitiveTypeKind.Int32, isNullable: false);
            product.AssertHasPrimitiveProperty(model, "Name", EdmPrimitiveTypeKind.String, isNullable: true);
            product.AssertHasPrimitiveProperty(model, "ReleaseDate", EdmPrimitiveTypeKind.DateTimeOffset, isNullable: true);
            product.AssertHasComplexProperty(model, "Version", typeof(ProductVersion), isNullable: true);
            product.AssertHasNavigationProperty(model, "Category", typeof(CategoryWithKeyAttribute), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne);


            var category = model.AssertHasEntityType(mappedEntityClrType: typeof(CategoryWithKeyAttribute));
            Assert.Equal(2, category.StructuralProperties().Count());
            Assert.Equal(1, category.NavigationProperties().Count());
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
        public void ModelBuilder_ProductsWithFilterSortable()
        {
            var modelBuilder = new ODataConventionModelBuilder();
            var entityTypeConf = modelBuilder.EntityType<ProductWithFilterSortable>();
            modelBuilder.EntitySet<ProductWithFilterSortable>("Products");
            var model = modelBuilder.GetEdmModel();

            var prop = entityTypeConf.Properties.FirstOrDefault(p => p.Name == "Name");
            Assert.False(prop.NonFilterable);

            prop = entityTypeConf.Properties.FirstOrDefault(p => p.Name == "NonFilterableProperty");
            Assert.True(prop.NonFilterable);

            prop = entityTypeConf.Properties.FirstOrDefault(p => p.Name == "UnsortableProperty");
            Assert.True(prop.Unsortable);

            prop = entityTypeConf.Properties.FirstOrDefault(p => p.Name == "Category");
            Assert.True(prop.NotNavigable);
            Assert.True(prop.NotExpandable);
        }

        [Fact]
        public void ModelBuilder_ProductsWithFilterSortableExplicitly()
        {
            var modelBuilder = new ODataConventionModelBuilder();
            var entityTypeConf = modelBuilder.AddEntityType(typeof(ProductWithFilterSortable));
            entityTypeConf.AddProperty(typeof(ProductWithFilterSortable).GetProperty("NonFilterableProperty"));
            entityTypeConf.AddProperty(typeof(ProductWithFilterSortable).GetProperty("UnsortableProperty"));
            entityTypeConf.AddNavigationProperty(typeof(ProductWithFilterSortable).GetProperty("Category"), EdmMultiplicity.One);

            var model = modelBuilder.GetEdmModel();

            var prop = entityTypeConf.Properties.FirstOrDefault(p => p.Name == "NonFilterableProperty");
            Assert.False(prop.NonFilterable);

            prop = entityTypeConf.Properties.FirstOrDefault(p => p.Name == "UnsortableProperty");
            Assert.False(prop.Unsortable);

            prop = entityTypeConf.Properties.FirstOrDefault(p => p.Name == "Category");
            Assert.False(prop.NotNavigable);
            Assert.False(prop.NotExpandable);
        }

        [Fact]
        public void ModelBuilder_ProductsWithConcurrentcyCheckAttribute()
        {
            // Arrange
            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<ProductWithETagAttribute>("Products");

            // Act
            var model = modelBuilder.GetEdmModel();

            // Assert
            Assert.Equal(model.SchemaElements.OfType<IEdmSchemaType>().Count(), 1);

            var product = model.AssertHasEntitySet(entitySetName: "Products", mappedEntityClrType: typeof(ProductWithETagAttribute));
            Assert.Equal(2, product.StructuralProperties().Count());
            Assert.Equal(0, product.NavigationProperties().Count());
            product.AssertHasKey(model, "ID", EdmPrimitiveTypeKind.Int32);
            IEdmStructuralProperty idProperty =
                product.AssertHasPrimitiveProperty(model, "ID", EdmPrimitiveTypeKind.Int32, isNullable: false);
            Assert.Equal(EdmConcurrencyMode.None, idProperty.ConcurrencyMode);
            IEdmStructuralProperty nameProperty =
                product.AssertHasPrimitiveProperty(model, "Name", EdmPrimitiveTypeKind.String, isNullable: true);
            Assert.Equal(EdmConcurrencyMode.Fixed, nameProperty.ConcurrencyMode);
        }

        [Fact]
        public void ModelBuilder_ProductWithTimestampAttribute()
        {
            // Arrange
            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<ProductWithTimestampAttribute>("Products");

            // Act
            var model = modelBuilder.GetEdmModel();

            // Assert
            Assert.Equal(model.SchemaElements.OfType<IEdmSchemaType>().Count(), 1);
            var product = model.AssertHasEntitySet(entitySetName: "Products", mappedEntityClrType: typeof(ProductWithTimestampAttribute));
            IEdmStructuralProperty nameProperty =
                product.AssertHasPrimitiveProperty(model, "Name", EdmPrimitiveTypeKind.String, isNullable: true);
            Assert.Equal(EdmConcurrencyMode.Fixed, nameProperty.ConcurrencyMode);
        }

        [Theory]
        [InlineData(typeof(Version[]))]
        [InlineData(typeof(IEnumerable<Version>))]
        [InlineData(typeof(List<Version>))]
        public void ModelBuilder_SupportsComplexCollectionWhenNotToldElementTypeIsComplex(Type complexCollectionPropertyType)
        {
            var modelBuilder = new ODataConventionModelBuilder();
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
            var modelBuilder = new ODataConventionModelBuilder();
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
            var modelBuilder = new ODataConventionModelBuilder();
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
            var modelBuilder = new ODataConventionModelBuilder();
            Type entityType =
                new MockType("SampleType")
                .Property<int>("ID")
                .Property(collectionType, "Products");

            modelBuilder.AddEntityType(entityType);

            Assert.DoesNotThrow(
               () => modelBuilder.GetEdmModel());
        }

        [Fact]
        public void ModelBuilder_CanBuild_ModelWithInheritance()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Vehicle>("Vehicles");

            IEdmModel model = builder.GetEdmModel();

            Assert.Equal(_totalExpectedSchemaTypesForVehiclesModel, model.SchemaElements.Count());
            Assert.Equal(1, model.EntityContainer.EntitySets().Count());
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
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntityType<SportBike>();
            builder.EntityType<Car>();
            builder.EntityType<Vehicle>();

            IEdmModel model = builder.GetEdmModel();

            Assert.Equal(_totalExpectedSchemaTypesForVehiclesModel, model.SchemaElements.Count());
        }

        [Fact]
        public void ModelBuilder_Ignores_IgnoredTypeAndTheirDerivedTypes()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Vehicle>("Vehicles");
            builder.Ignore<Motorcycle>();

            IEdmModel model = builder.GetEdmModel();

            // ignore motorcycle, sportbike and MotorcycleManufacturer
            Assert.Equal(_totalExpectedSchemaTypesForVehiclesModel - 3, model.SchemaElements.Count());
            Assert.Equal(1, model.EntityContainer.EntitySets().Count());
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
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Vehicle>("Vehicles");
            builder.Ignore<Motorcycle>();
            builder.EntityType<SportBike>();

            IEdmModel model = builder.GetEdmModel();

            Assert.Equal(_totalExpectedSchemaTypesForVehiclesModel - 1, model.SchemaElements.Count());
            Assert.Equal(1, model.EntityContainer.EntitySets().Count());
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
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntityType<Vehicle>();
            builder.EntityType<Car>();
            builder.EntityType<Motorcycle>();
            builder.EntityType<SportBike>();

            IEdmModel model = builder.GetEdmModel();

            Assert.Equal(_totalExpectedSchemaTypesForVehiclesModel, model.SchemaElements.Count());

            var vehicle = model.AssertHasEntityType(typeof(Vehicle));
            Assert.Equal(null, vehicle.BaseEntityType());

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
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntityType<Vehicle>();
            builder.EntityType<Car>().DerivesFromNothing();
            builder.EntityType<Motorcycle>().DerivesFromNothing();
            builder.EntityType<SportBike>();

            IEdmModel model = builder.GetEdmModel();

            Assert.Equal(_totalExpectedSchemaTypesForVehiclesModel, model.SchemaElements.Count());

            var vehicle = model.AssertHasEntityType(typeof(Vehicle));
            Assert.Equal(null, vehicle.BaseEntityType());
            Assert.Equal(2, vehicle.Key().Count());

            var motorcycle = model.AssertHasEntityType(typeof(Motorcycle));
            Assert.Equal(null, motorcycle.BaseEntityType());
            Assert.Equal(2, motorcycle.Key().Count());
            Assert.Equal(5, motorcycle.Properties().Count());

            var car = model.AssertHasEntityType(typeof(Car));
            Assert.Equal(null, car.BaseEntityType());
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
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
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
        public void ModelBuilder_Doesnot_Override_AbstractnessOfEntityTypes_IfSet()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntityType<Vehicle>();
            builder.EntityType<Motorcycle>().Abstract();

            IEdmModel model = builder.GetEdmModel();

            Assert.Equal(_totalExpectedSchemaTypesForVehiclesModel, model.SchemaElements.Count());
            Assert.True(model.AssertHasEntityType(typeof(Motorcycle)).IsAbstract);
            Assert.False(model.AssertHasEntityType(typeof(SportBike)).IsAbstract);
        }

        [Fact]
        public void ModelBuilder_CanHaveAnAbstractDerivedTypeOfConcreteBaseType()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
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
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Vehicle>("vehicles");

            IEdmModel model = builder.GetEdmModel();

            Assert.Equal(_totalExpectedSchemaTypesForVehiclesModel, model.SchemaElements.Count());
            model.AssertHasComplexType(typeof(ManufacturerAddress));
        }

        [Fact]
        public void ModelBuilder_ModelAliased_IfModelAliasingEnabled()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder { ModelAliasingEnabled = true };
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
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder { ModelAliasingEnabled = true };
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
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder { ModelAliasingEnabled = true };
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
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder { ModelAliasingEnabled = true };
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
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder { ModelAliasingEnabled = true };
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
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Vehicle>("vehicles");
            builder.Singleton<Vehicle>("MyVehicle");
            builder.EntitySet<Manufacturer>("manufacturers");

            IEdmModel model = builder.GetEdmModel();

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
                "manufacturers");
            vehicles.AssertHasNavigationTarget(
                motorcycle.AssertHasNavigationProperty(model, "Manufacturer", typeof(MotorcycleManufacturer), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne),
                "manufacturers");
            vehicles.AssertHasNavigationTarget(
                sportbike.AssertHasNavigationProperty(model, "Manufacturer", typeof(MotorcycleManufacturer), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne),
                "manufacturers");

            // for singleton
            Assert.Equal(2, singleton.NavigationPropertyBindings.Count());
            singleton.AssertHasNavigationTarget(
                car.AssertHasNavigationProperty(model, "Manufacturer", typeof(CarManufacturer), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne),
                "manufacturers");
            singleton.AssertHasNavigationTarget(
                motorcycle.AssertHasNavigationProperty(model, "Manufacturer", typeof(MotorcycleManufacturer), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne),
                "manufacturers");
            singleton.AssertHasNavigationTarget(
                sportbike.AssertHasNavigationProperty(model, "Manufacturer", typeof(MotorcycleManufacturer), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne),
                "manufacturers");
        }

        [Fact]
        public void ModelBuilder_BindsToTheClosestEntitySet_ForNavigationProperties()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Vehicle>("vehicles");
            builder.Singleton<Vehicle>("MyVehicle");
            builder.EntitySet<CarManufacturer>("car_manufacturers");
            builder.EntitySet<MotorcycleManufacturer>("motorcycle_manufacturers");

            IEdmModel model = builder.GetEdmModel();

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
                "car_manufacturers");
            vehicles.AssertHasNavigationTarget(
                motorcycle.AssertHasNavigationProperty(model, "Manufacturer", typeof(MotorcycleManufacturer), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne),
                "motorcycle_manufacturers");
            vehicles.AssertHasNavigationTarget(
                sportbike.AssertHasNavigationProperty(model, "Manufacturer", typeof(MotorcycleManufacturer), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne),
                "motorcycle_manufacturers");

            // for singleton
            Assert.Equal(2, singleton.NavigationPropertyBindings.Count());
            singleton.AssertHasNavigationTarget(
                car.AssertHasNavigationProperty(model, "Manufacturer", typeof(CarManufacturer), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne),
                "car_manufacturers");
            singleton.AssertHasNavigationTarget(
                motorcycle.AssertHasNavigationProperty(model, "Manufacturer", typeof(MotorcycleManufacturer), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne),
                "motorcycle_manufacturers");
            singleton.AssertHasNavigationTarget(
                sportbike.AssertHasNavigationProperty(model, "Manufacturer", typeof(MotorcycleManufacturer), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne),
                "motorcycle_manufacturers");
        }

        [Fact]
        public void ModelBuilder_BindsToAllEntitySets()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();

            builder.EntitySet<Vehicle>("vehicles");
            builder.EntitySet<Car>("cars");
            builder.EntitySet<Motorcycle>("motorcycles");
            builder.EntitySet<SportBike>("sportbikes");
            builder.EntitySet<CarManufacturer>("car_manufacturers");
            builder.EntitySet<MotorcycleManufacturer>("motorcycle_manufacturers");

            IEdmModel model = builder.GetEdmModel();

            // one for motorcycle manufacturer and one for car manufacturer
            IEdmEntitySet vehicles = model.EntityContainer.FindEntitySet("vehicles");
            Assert.Equal(2, vehicles.NavigationPropertyBindings.Count());

            // one for car manufacturer
            IEdmEntitySet cars = model.EntityContainer.FindEntitySet("cars");
            Assert.Equal(1, cars.NavigationPropertyBindings.Count());

            // one for motorcycle manufacturer
            IEdmEntitySet motorcycles = model.EntityContainer.FindEntitySet("motorcycles");
            Assert.Equal(1, motorcycles.NavigationPropertyBindings.Count());

            // one for motorcycle manufacturer
            IEdmEntitySet sportbikes = model.EntityContainer.FindEntitySet("sportbikes");
            Assert.Equal(1, sportbikes.NavigationPropertyBindings.Count());

            // no navigations
            IEdmEntitySet carManufacturers = model.EntityContainer.FindEntitySet("car_manufacturers");
            Assert.Equal(0, carManufacturers.NavigationPropertyBindings.Count());

            //  no navigations
            IEdmEntitySet motorcycleManufacturers = model.EntityContainer.FindEntitySet("motorcycle_manufacturers");
            Assert.Equal(0, motorcycleManufacturers.NavigationPropertyBindings.Count());
        }

        [Fact]
        public void ModelBuilder_OnSingleton_BindsToAllEntitySets()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
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

            // one for car manufacturer
            IEdmSingleton car = model.EntityContainer.FindSingleton("Contoso");
            Assert.Equal(1, car.NavigationPropertyBindings.Count());

            // one for motorcycle manufacturer
            IEdmSingleton motorcycle = model.EntityContainer.FindSingleton("MyMotorcycle");
            Assert.Equal(1, motorcycle.NavigationPropertyBindings.Count());

            // one for motorcycle manufacturer
            IEdmSingleton sportbike = model.EntityContainer.FindSingleton("Gianta");
            Assert.Equal(1, sportbike.NavigationPropertyBindings.Count());

            // no navigation
            IEdmSingleton carManufacturer = model.EntityContainer.FindSingleton("Fordo");
            Assert.Equal(0, carManufacturer.NavigationPropertyBindings.Count());

            //  no navigation
            IEdmSingleton motorcycleManufacturer = model.EntityContainer.FindSingleton("Yayaham");
            Assert.Equal(0, motorcycleManufacturer.NavigationPropertyBindings.Count());
        }

        [Fact]
        public void ModelBuilder_OnSingleton_OnlyHasOneBinding_WithoutAnyEntitySets()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.Singleton<Employee>("Gibs");

            // Act
            IEdmModel model = builder.GetEdmModel();
            Assert.NotNull(model); // Guard
            IEdmSingleton singleton = model.EntityContainer.FindSingleton("Gibs");

            // Assert
            Assert.NotNull(singleton);
            Assert.Single(singleton.NavigationPropertyBindings);
            Assert.Equal("Boss", singleton.NavigationPropertyBindings.Single().NavigationProperty.Name);
        }

        [Fact]
        public void ModelBuilder_OnSingleton_HasBindings_WithEntitySet()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
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
            var customerType = model.AssertHasEntityType(typeof(Customer));
            var bossProperty = employeeType.AssertHasNavigationProperty(model, "Boss", typeof(Employee), true, EdmMultiplicity.ZeroOrOne);
            var customerProperty = salePersonType.AssertHasNavigationProperty(model, "Customers", typeof(Customer), true, EdmMultiplicity.Many);

            Assert.Equal(EdmNavigationSourceKind.Singleton, singleton.FindNavigationTarget(bossProperty).NavigationSourceKind());
            Assert.Equal(EdmNavigationSourceKind.EntitySet, singleton.FindNavigationTarget(customerProperty).NavigationSourceKind());
        }

        [Fact]
        public void ModelBuilder_DerivedTypeDeclaringKeyThrows()
        {
            MockType baseType =
                  new MockType("BaseType")
                  .Property(typeof(int), "BaseTypeID");

            MockType derivedType =
                new MockType("DerivedType")
                .Property(typeof(int), "DerivedTypeId")
                .BaseType(baseType);

            MockAssembly assembly = new MockAssembly(baseType, derivedType);

            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Services.Replace(typeof(IAssembliesResolver), new TestAssemblyResolver(assembly));
            var builder = new ODataConventionModelBuilder(configuration);

            builder.AddEntitySet("bases", builder.AddEntityType(baseType));

            Assert.Throws<InvalidOperationException>(
                () => builder.GetEdmModel(),
            "Cannot define keys on type 'DefaultNamespace.DerivedType' deriving from 'DefaultNamespace.BaseType'. Only the root type in the entity inheritance hierarchy can contain keys.");
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

            MockAssembly assembly = new MockAssembly(baseType, derivedType);

            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Services.Replace(typeof(IAssembliesResolver), new TestAssemblyResolver(assembly));
            var builder = new ODataConventionModelBuilder(configuration, isQueryCompositionMode: true);

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
        public void ModelBuilder_DerivedComplexTypeHavingKeys_Throws()
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

            MockAssembly assembly = new MockAssembly(baseComplexType, derivedComplexType, entityType);

            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Services.Replace(typeof(IAssembliesResolver), new TestAssemblyResolver(assembly));
            var builder = new ODataConventionModelBuilder(configuration);

            builder.AddEntitySet("entities", builder.AddEntityType(entityType));

            Assert.Throws<InvalidOperationException>(
                () => builder.GetEdmModel(),
                "Cannot define keys on type 'DefaultNamespace.DerivedComplexType' deriving from 'DefaultNamespace.BaseComplexType'. Only the root type in the entity inheritance hierarchy can contain keys.");
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

            MockAssembly assembly = new MockAssembly(baseComplexType, derivedComplexType, entityType);

            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Services.Replace(typeof(IAssembliesResolver), new TestAssemblyResolver(assembly));
            var builder = new ODataConventionModelBuilder(configuration);

            builder.AddEntitySet("entities", builder.AddEntityType(entityType));
            builder.AddComplexType(baseComplexType);

            IEdmModel model = builder.GetEdmModel();
            Assert.Equal(3, model.SchemaElements.Count());
            Assert.NotNull(model.FindType("DefaultNamespace.EntityType"));
            Assert.NotNull(model.FindType("DefaultNamespace.BaseComplexType"));
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
        [PropertyData("ModelBuilder_PrunesUnReachableTypes_Data")]
        public void ModelBuilder_PrunesUnReachableTypes(MockType type)
        {
            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.AddEntityType(type);

            var model = modelBuilder.GetEdmModel();
            Assert.True(model.FindType("DefaultNamespace.IgnoredType") == null);
        }

        [Fact]
        public void ModelBuilder_DeepChainOfComplexTypes()
        {
            var modelBuilder = new ODataConventionModelBuilder();

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

        [Fact]
        public void ComplexType_Containing_EntityCollection_Throws()
        {
            MockType entityType = new MockType("EntityType");

            MockType complexType =
                new MockType("ComplexTypeWithEntityCollection")
                .Property(entityType.AsCollection(), "CollectionProperty");

            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.AddEntityType(entityType);
            modelBuilder.AddComplexType(complexType);

            Assert.Throws<InvalidOperationException>(
                () => modelBuilder.GetEdmModel(),
                "The complex type 'DefaultNamespace.ComplexTypeWithEntityCollection' refers to the entity type 'DefaultNamespace.EntityType' through the property 'CollectionProperty'.");
        }

        [Fact]
        public void ComplexType_Containing_ComplexCollection_works()
        {
            Type complexType =
                new MockType("ComplexTypeWithComplexCollection")
                .Property<Version[]>("CollectionProperty");

            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.AddComplexType(complexType);

            var model = modelBuilder.GetEdmModel();

            IEdmComplexType complexEdmType = model.AssertHasComplexType(complexType);
            model.AssertHasComplexType(typeof(Version));
            var collectionProperty = complexEdmType.DeclaredProperties.Where(p => p.Name == "CollectionProperty").SingleOrDefault();
            Assert.NotNull(collectionProperty);
            Assert.True(collectionProperty.Type.IsCollection());
            Assert.Equal(collectionProperty.Type.AsCollection().ElementType().FullName(), "System.Version");
        }

        [Fact]
        public void EntityType_Containing_ComplexCollection_Works()
        {
            Type entityType =
                new MockType("EntityTypeWithComplexCollection")
                .Property<int>("ID")
                .Property<Version[]>("CollectionProperty");

            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.AddEntityType(entityType);

            var model = modelBuilder.GetEdmModel();

            IEdmEntityType entityEdmType = model.AssertHasEntityType(entityType);
            model.AssertHasComplexType(typeof(Version));
            var collectionProperty = entityEdmType.DeclaredProperties.Where(p => p.Name == "CollectionProperty").SingleOrDefault();
            Assert.NotNull(collectionProperty);
            Assert.True(collectionProperty.Type.IsCollection());
            Assert.Equal(collectionProperty.Type.AsCollection().ElementType().FullName(), "System.Version");
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

            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.AddEntityType(entityType);

            var model = modelBuilder.GetEdmModel();

            IEdmEntityType entityEdmType = model.AssertHasEntityType(entityType);
            model.AssertHasComplexType(typeof(Version));
            IEdmComplexType edmComplexType = model.AssertHasComplexType(complexTypeWithComplexCollection);

            var collectionProperty = edmComplexType.DeclaredProperties.Where(p => p.Name == "ComplexCollectionProperty").SingleOrDefault();
            Assert.NotNull(collectionProperty);
            Assert.True(collectionProperty.Type.IsCollection());
            Assert.Equal(collectionProperty.Type.AsCollection().ElementType().FullName(), "System.Version");
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

            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
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

            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
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

            ODataConventionModelBuilder builder = new ODataConventionModelBuilder(new HttpConfiguration(), isQueryCompositionMode: true);
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

            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
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

            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
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
            MockType complexBase =
                new MockType("ComplexBase")
                .Property<int>("BaseProperty");

            MockType complexDerived =
                new MockType("ComplexBase")
                .BaseType(complexBase)
                .Property<int>("DerivedProperty");

            MockType entity =
                new MockType("entity")
                .Property<int>("ID")
                .Property(complexBase, "ComplexBase")
                .Property(complexDerived, "ComplexDerived");

            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            var entityType = builder.AddEntityType(entity);

            IEdmModel model = builder.GetEdmModel();
            Assert.Equal(4, model.SchemaElements.Count());
            var entityEdmType = model.AssertHasEntityType(entity);

            var complexBaseEdmType = model.AssertHasComplexType(complexBase);
            complexBaseEdmType.AssertHasPrimitiveProperty(model, "BaseProperty", EdmPrimitiveTypeKind.Int32, isNullable: false);

            var complexDerivedEdmType = model.AssertHasComplexType(complexDerived);
            complexDerivedEdmType.AssertHasPrimitiveProperty(model, "BaseProperty", EdmPrimitiveTypeKind.Int32, isNullable: false);
            complexDerivedEdmType.AssertHasPrimitiveProperty(model, "DerivedProperty", EdmPrimitiveTypeKind.Int32, isNullable: false);
        }

        [Fact]
        public void OnModelCreating_IsInvoked_AfterConventionsAreRun()
        {
            // Arrange
            MockType entity =
                new MockType("entity")
                .Property<int>("ID");

            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.AddEntitySet("entities", builder.AddEntityType(entity));
            builder.OnModelCreating = (modelBuilder) =>
                {
                    var entityConfiguration = modelBuilder.StructuralTypes.OfType<EntityTypeConfiguration>().Single();
                    Assert.Equal(1, entityConfiguration.Keys.Count());
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

            var mockAssembly = new MockAssembly(baseType, derivedType);

            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Services.Replace(typeof(IAssembliesResolver), new TestAssemblyResolver(mockAssembly));
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

            var builder = new ODataConventionModelBuilder();
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

            Assert.DoesNotThrow(() => builder.GetEdmModel());
        }

        [Fact]
        public void ODataConventionModelBuilder_GetEdmModel_HasContainment()
        {
            // Arrange
            var builder = new ODataConventionModelBuilder();
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
            var builder = new ODataConventionModelBuilder();
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
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
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
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
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
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntityType<EntityKeyConventionTests_AlbumWithKey>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmEntityType entityType = model.AssertHasEntityType(typeof(EntityKeyConventionTests_AlbumWithKey));
            IEdmStructuralProperty keyProperty = Assert.Single(entityType.Key());
            Assert.Equal("Path", keyProperty.Name);
            Assert.True(keyProperty.Type.IsString());
        }
    }

    public class Product
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public DateTimeOffset? ReleaseDate { get; set; }

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

    public class ProductWithFilterSortable
    {
        public int ID { get; set; }

        public string Name { get; set; }

        [NonFilterable]
        public string NonFilterableProperty { get; set; }

        [Unsortable]
        public string UnsortableProperty { get; set; }

        [NotNavigable]
        [NotExpandable]
        public CategoryWithKeyAttribute Category { get; set; }
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

    public class ProductWithETagAttribute
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
}
