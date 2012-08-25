// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Web.Http.OData.Formatter;
using Microsoft.Data.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Builder.Conventions
{
    public class ODataConventionModelBuilderTests
    {
        [Fact]
        public void ModelBuilder_Products()
        {
            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<Product>("Products");

            var model = modelBuilder.GetEdmModel();
            Assert.Equal(model.SchemaElements.OfType<IEdmSchemaType>().Count(), 3);

            var product = model.AssertHasEntitySet(entitySetName: "Products", mappedEntityClrType: typeof(Product));
            Assert.Equal(4, product.StructuralProperties().Count());
            Assert.Equal(1, product.NavigationProperties().Count());
            product.AssertHasKey(model, "ID", EdmPrimitiveTypeKind.Int32);
            product.AssertHasPrimitiveProperty(model, "ID", EdmPrimitiveTypeKind.Int32, isNullable: false);
            product.AssertHasPrimitiveProperty(model, "Name", EdmPrimitiveTypeKind.String, isNullable: true);
            product.AssertHasPrimitiveProperty(model, "ReleaseDate", EdmPrimitiveTypeKind.DateTime, isNullable: true);
            product.AssertHasComplexProperty(model, "Version", "System.Web.Http.OData.Builder.Conventions.ProductVersion", isNullable: true);
            product.AssertHasNavigationProperty(model, "Category", typeof(Category), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne);


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
            product.AssertHasPrimitiveProperty(model, "ReleaseDate", EdmPrimitiveTypeKind.DateTime, isNullable: true);
            product.AssertHasComplexProperty(model, "Version", "System.Web.Http.OData.Builder.Conventions.ProductVersion", isNullable: true);
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

        [Theory(Skip="ODataConventionModelBuilder always treats unknown element types are Entities")]
        [InlineData(typeof(Version[]))]
        [InlineData(typeof(IEnumerable<Version>))]
        [InlineData(typeof(List<Version>))]
        public void ModelBuilder_SupportsComplexCollectionWhenNotToldElementTypeIsComplex(Type complexCollectionPropertyType)
        {
            var modelBuilder = new ODataConventionModelBuilder();
            Type entityType = CreateDynamicType(
                new DynamicType
                {
                    TypeName = "SampleType",
                    Properties = 
                    {
                        new DynamicProperty { Name = "ID", Type = typeof(int) }, 
                        new DynamicProperty { Name = "Property1", Type = complexCollectionPropertyType }
                    }
                });
            modelBuilder.AddEntity(entityType);
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
            Type entityType = CreateDynamicType(
                new DynamicType
                {
                    TypeName = "SampleType",
                    Properties = 
                    {
                        new DynamicProperty { Name = "ID", Type = typeof(int) }, 
                        new DynamicProperty { Name = "Property1", Type = complexCollectionPropertyType }
                    }
                });
            modelBuilder.AddEntity(entityType);
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
            Type entityType = CreateDynamicType(
                new DynamicType
                {
                    TypeName = "SampleType",
                    Properties = 
                    {
                        new DynamicProperty { Name = "ID", Type = typeof(int) }, 
                        new DynamicProperty { Name = "Property1", Type = primitiveCollectionPropertyType }
                    }
                });
            modelBuilder.AddEntity(entityType);
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
            Type entityType = CreateDynamicType(
                new DynamicType
                    {
                        TypeName = "SampleType",
                        Properties = 
                        {
                            new DynamicProperty { Name = "ID", Type = typeof(int) }, 
                            new DynamicProperty { Name = "Products", Type =collectionType }
                        }
                    });
            modelBuilder.AddEntity(entityType);

            Assert.DoesNotThrow(
               () => modelBuilder.GetEdmModel());
        }

        public static IEnumerable<object[]> AllLoadedTypes
        {
            get
            {
                return AppDomain
                    .CurrentDomain
                    .GetAssemblies()
                    .SelectMany(assembly =>
                    {
                        Type[] types;
                        try
                        {
                            types = assembly.GetTypes();
                        }
                        catch (ReflectionTypeLoadException e)
                        {
                            types = e.Types;
                        }

                        return types ?? Enumerable.Empty<Type>();
                    })
                    .Where(t => t != null)
                    .Where(t => t.IsPublic && t.IsClass && !t.IsGenericTypeDefinition)
                    .Where(t => !(t.IsAbstract && t.IsSealed)) // static class
                    .Select(t => new object[] { t });
            }
        }

        [Theory]
        [PropertyData("AllLoadedTypes")]
        public void ModelBuilder_AnyType_QueryComposition(Type type)
        {
            var modelBuilder = new ODataConventionModelBuilder(isQueryCompositionMode: true);
            modelBuilder.AddEntity(type);

            IEdmModel model = modelBuilder.GetEdmModel();
            Assert.True(model.SchemaElements.Count() > 0);
            IEdmEntityType entityType = Assert.IsAssignableFrom<IEdmEntityType>(model.FindType(type.EdmFullName()));
        }

        public static TheoryDataSet<DynamicType> ModelBuilder_PrunesUnReachableTypes_Data
        {
            get
            {
                DynamicType ignoredType = new DynamicType
                {
                    TypeName = "IgnoredType",
                    Properties = 
                    { 
                        new DynamicProperty { Name = "Property", Type = typeof(int)}
                    }
                };

                return new TheoryDataSet<DynamicType>
                {
                    new DynamicType
                    { 
                        TypeName = "SampleType", 
                        Properties = 
                        { 
                            new DynamicProperty { Name = "ID", Type = typeof(int) }, 
                            new DynamicProperty { Name = "IgnoredProperty", Type = ignoredType, Attributes = new[] { new NotMappedAttribute() } }
                        } 
                    },

                    new DynamicType
                    { 
                        TypeName = "SampleType", 
                        Properties = 
                        { 
                            new DynamicProperty { Name = "ID", Type = typeof(int) }, 
                            new DynamicProperty 
                            { 
                                Name = "IgnoredProperty", 
                                Attributes = new[] { new NotMappedAttribute() },
                                Type = new DynamicType
                                {
                                    TypeName = "AnotherType",
                                    Properties = { new DynamicProperty { Name = "IgnoredProperty", Type = ignoredType } }
                                }
                            }
                        } 
                    },

                    new DynamicType
                    { 
                        TypeName = "SampleType", 
                        Properties = 
                        { 
                            new DynamicProperty { Name = "ID", Type = typeof(int) }, 
                            new DynamicProperty 
                            { 
                                Name = "IgnoredProperty", 
                                Type = new DynamicType
                                {
                                    TypeName = "AnotherType",
                                    Properties = { new DynamicProperty { Name = "IgnoredProperty", Type = ignoredType, Attributes = new[] { new NotMappedAttribute() } } },
                                }
                            }
                        } 
                    }
                };
            }
        }

        [Theory]
        [PropertyData("ModelBuilder_PrunesUnReachableTypes_Data")]
        public void ModelBuilder_PrunesUnReachableTypes(DynamicType type)
        {
            var modelBuilder = new ODataConventionModelBuilder();
            Type entityType = CreateDynamicType(type);
            modelBuilder.AddEntity(entityType);

            var model = modelBuilder.GetEdmModel();
            Assert.True(model.FindType("SampleNamespace.IgnoredType") == null);
        }

        [Fact]
        public void ModelBuilder_DeepChainOfComplexTypes()
        {
            var modelBuilder = new ODataConventionModelBuilder();
            Type entityType = CreateDynamicType(
                new DynamicType
                    {
                        TypeName = "SampleType",
                        Properties = 
                        { 
                            new DynamicProperty { Name = "ID", Type = typeof(int) }, 
                            new DynamicProperty 
                            { 
                                Name = "Property", 
                                Type = new DynamicType 
                                {
                                    TypeName = "ComplexType1",
                                    Properties =
                                    { 
                                        new DynamicProperty
                                        { 
                                            Name ="Property", 
                                            Type = new DynamicType
                                            {
                                                TypeName = "ComplexType2",
                                                Properties = 
                                                {
                                                    new DynamicProperty
                                                    { 
                                                        Name ="Property", 
                                                        Type = new DynamicType
                                                        {
                                                            TypeName = "ComplexType3",
                                                            Properties = { new DynamicProperty { Name = "Property", Type = typeof(int) } }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    });
            modelBuilder.AddEntity(entityType);

            var model = modelBuilder.GetEdmModel();
            Assert.NotNull(model.FindType("SampleNamespace.SampleType"));
            Assert.NotNull(model.FindType("SampleNamespace.ComplexType1"));
            Assert.NotNull(model.FindType("SampleNamespace.ComplexType2"));
            Assert.NotNull(model.FindType("SampleNamespace.ComplexType3"));
        }

        private static Type CreateDynamicType(DynamicType dynamicType)
        {
            Mock<Type> type = new Mock<Type>();
            type.Setup(t => t.Name).Returns(dynamicType.TypeName);
            type.Setup(t => t.Namespace).Returns("SampleNamespace");
            type.Setup(t => t.FullName).Returns("SampleNamespace." + dynamicType.TypeName);
            type
                .Setup(t => t.GetProperties(It.IsAny<BindingFlags>()))
                .Returns(dynamicType.Properties.Select(property => CreateProperty(property, type.Object)).Cast<PropertyInfo>().ToArray());
            type.Setup(t => t.IsAssignableFrom(type.Object)).Returns(true);
            type.Setup(t => t.GetHashCode()).Returns(type.GetHashCode());
            type.Setup(t => t.Equals(It.IsAny<object>())).Returns((Type t) => Object.ReferenceEquals(type.Object, t));
            return type.Object;
        }

        private static PropertyInfo CreateProperty(DynamicProperty property, Type reflectedType)
        {
            Mock<PropertyInfo> pi = new Mock<PropertyInfo>();
            pi.Setup(p => p.Name).Returns(property.Name);

            if (property.Type is DynamicType)
            {
                property.Type = CreateDynamicType(property.Type as DynamicType);
            }

            pi.Setup(p => p.PropertyType).Returns(property.Type as Type);
            pi.Setup(p => p.ReflectedType).Returns(reflectedType);
            pi.Setup(p => p.DeclaringType).Returns(reflectedType);
            pi.Setup(p => p.CanRead).Returns(true);
            pi.Setup(p => p.CanWrite).Returns(true);
            pi.Setup(p => p.GetGetMethod(It.IsAny<bool>())).Returns(typeof(Product).GetProperty("ID").GetGetMethod());
            pi.Setup(p => p.GetCustomAttributes(It.IsAny<bool>())).Returns(property.Attributes);
            pi.Setup(p => p.GetHashCode()).Returns(pi.GetHashCode());
            pi.Setup(p => p.Equals(It.IsAny<object>())).Returns((PropertyInfo p) => Object.ReferenceEquals(pi.Object, p));
            return pi.Object;
        }

        public class DynamicType
        {
            public DynamicType()
            {
                Properties = new List<DynamicProperty>();
            }

            public string TypeName { get; set; }

            public ICollection<DynamicProperty> Properties { get; set; }
        }

        public class DynamicProperty
        {
            public DynamicProperty()
            {
                Attributes = new Attribute[0];
            }

            public string Name { get; set; }

            public object Type { get; set; }

            public Attribute[] Attributes { get; set; }
        }
    }

    public class Product
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public DateTime? ReleaseDate { get; set; }

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

        public DateTime? ReleaseDate { get; set; }

        public ProductVersion Version { get; set; }

        public CategoryWithKeyAttribute Category { get; set; }
    }

    public class CategoryWithKeyAttribute
    {
        [Key]
        public Guid IdOfCategory { get; set; }

        public string Name { get; set; }

        public ICollection<ProductWithKeyAttribute> Products { get; set; }
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
}
