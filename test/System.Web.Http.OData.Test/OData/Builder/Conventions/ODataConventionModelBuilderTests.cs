// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Http.OData.Formatter;
using Microsoft.Data.Edm;
using Xunit;

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

            var entitySet = model.EntityContainers().Single().EntitySets().Single();
            Assert.NotNull(entitySet);
            Assert.True(entitySet.Name == "Products");
            Assert.True(entitySet.ElementType.Name == "Product");
            Assert.True(model.GetEdmType(typeof(Product)).IsEquivalentTo(entitySet.ElementType));

            var product = model.SchemaElements.OfType<IEdmEntityType>().Where(t => t.Name == "Product").SingleOrDefault();
            Assert.NotNull(product);
            Assert.True(product.DeclaredKey.Single().Name == "ID");
            Assert.True(product.DeclaredKey.Single().Type.Definition.IsEquivalentTo(model.FindType("Edm.Int32")));
            Assert.False(product.DeclaredKey.Single().Type.IsNullable);
            Assert.Equal(product.StructuralProperties().OrderBy(p => p.Name).Select(p => p.Name), new string[] { "ID", "Name", "ReleaseDate", "Version" });
            Assert.Equal(product.StructuralProperties().OrderBy(p => p.Name).Select(p => (p.Type.Definition as IEdmSchemaType).Name), new string[] { "Int32", "String", "DateTime", "ProductVersion" });
            Assert.Equal(product.StructuralProperties().OrderBy(p => p.Name).Select(p => p.Type.IsNullable), new bool[] { false, true, true, false });
            Assert.Equal(product.NavigationProperties().Single().Name, "Category");
            Assert.Equal(product.NavigationProperties().Single().ToEntityType().Name, "Category");
            Assert.Equal(product.NavigationProperties().Single().Multiplicity(), EdmMultiplicity.ZeroOrOne);

            var category = model.SchemaElements.OfType<IEdmEntityType>().Where(t => t.Name == "Category").SingleOrDefault();
            Assert.NotNull(category);
            Assert.True(category.DeclaredKey.Single().Name == "ID");
            Assert.True(category.DeclaredKey.Single().Type.Definition.IsEquivalentTo(model.FindType("Edm.Int32")));
            Assert.False(category.DeclaredKey.Single().Type.IsNullable);
            Assert.Equal(category.StructuralProperties().OrderBy(p => p.Name).Select(p => p.Name), new string[] { "ID", "Name" });
            Assert.Equal(category.StructuralProperties().OrderBy(p => p.Name).Select(p => (p.Type.Definition as IEdmSchemaType).Name), new string[] { "Int32", "String" });
            Assert.Equal(category.StructuralProperties().OrderBy(p => p.Name).Select(p => p.Type.IsNullable), new bool[] { false, true });
            Assert.Equal(category.NavigationProperties().Single().Name, "Products");
            Assert.Equal(category.NavigationProperties().Single().ToEntityType().Name, "Product");
            // TODO: Bug 468693: [OData] Adding a navigation property with multiplicity Many in the model builder is generating a navigation property with multiplicity ZeroOrOne in the IEdmModel
            // Assert.Equal(category.NavigationProperties().Single().Multiplicity(), EdmMultiplicity.Many);

            var version = model.SchemaElements.OfType<IEdmComplexType>().Where(t => t.Name == "ProductVersion").SingleOrDefault();
            Assert.NotNull(version);
            Assert.Equal(version.StructuralProperties().OrderBy(p => p.Name).Select(p => p.Name), new string[] { "Major", "Minor" });
            Assert.Equal(version.StructuralProperties().OrderBy(p => p.Name).Select(p => (p.Type.Definition as IEdmSchemaType).Name), new string[] { "Int32", "Int32" });
            Assert.Equal(version.StructuralProperties().OrderBy(p => p.Name).Select(p => p.Type.IsNullable), new bool[] { false, false });
        }

        [Fact]
        public void ModelBuilder_ProductsWithKeyAttribute()
        {
            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<ProductWithKeyAttribute>("Products");

            var model = modelBuilder.GetEdmModel();

            Assert.Equal(model.SchemaElements.OfType<IEdmSchemaType>().Count(), 3);

            var entitySet = model.EntityContainers().Single().EntitySets().Single();
            Assert.NotNull(entitySet);
            Assert.Equal(entitySet.Name, "Products");
            Assert.Equal(entitySet.ElementType.Name, "ProductWithKeyAttribute");
            Assert.True(model.GetEdmType(typeof(ProductWithKeyAttribute)).IsEquivalentTo(entitySet.ElementType));

            var product = model.SchemaElements.OfType<IEdmEntityType>().Where(t => t.Name == "ProductWithKeyAttribute").SingleOrDefault();
            Assert.NotNull(product);
            Assert.Equal(product.DeclaredKey.Single().Name, "IdOfProduct");
            Assert.True(product.DeclaredKey.Single().Type.Definition.IsEquivalentTo(model.FindType("Edm.Int32")));
            Assert.False(product.DeclaredKey.Single().Type.IsNullable);
            Assert.Equal(product.StructuralProperties().OrderBy(p => p.Name).Select(p => p.Name), new string[] { "IdOfProduct", "Name", "ReleaseDate", "Version" });
            Assert.Equal(product.StructuralProperties().OrderBy(p => p.Name).Select(p => (p.Type.Definition as IEdmSchemaType).Name), new string[] { "Int32", "String", "DateTime", "ProductVersion" });
            Assert.Equal(product.StructuralProperties().OrderBy(p => p.Name).Select(p => p.Type.IsNullable), new bool[] { false, true, true, false });
            Assert.Equal(product.NavigationProperties().Single().Name, "Category");
            Assert.Equal(product.NavigationProperties().Single().ToEntityType().Name, "CategoryWithKeyAttribute");
            Assert.Equal(product.NavigationProperties().Single().Multiplicity(), EdmMultiplicity.ZeroOrOne);

            var category = model.SchemaElements.OfType<IEdmEntityType>().Where(t => t.Name == "CategoryWithKeyAttribute").SingleOrDefault();
            Assert.NotNull(category);
            Assert.Equal(category.DeclaredKey.Single().Name, "IdOfCategory");
            Assert.True(category.DeclaredKey.Single().Type.Definition.IsEquivalentTo(model.FindType("Edm.Int32")));
            Assert.False(category.DeclaredKey.Single().Type.IsNullable);
            Assert.Equal(category.StructuralProperties().OrderBy(p => p.Name).Select(p => p.Name), new string[] { "IdOfCategory", "Name" });
            Assert.Equal(category.StructuralProperties().OrderBy(p => p.Name).Select(p => (p.Type.Definition as IEdmSchemaType).Name), new string[] { "Int32", "String" });
            Assert.Equal(category.StructuralProperties().OrderBy(p => p.Name).Select(p => p.Type.IsNullable), new bool[] { false, true });
            Assert.Equal(category.NavigationProperties().Single().Name, "Products");
            Assert.Equal(category.NavigationProperties().Single().ToEntityType().Name, "ProductWithKeyAttribute");
            // TODO: Bug 468693: [OData] Adding a navigation property with multiplicity Many in the model builder is generating a navigation property with multiplicity ZeroOrOne in the IEdmModel
            // Assert.Equal(category.NavigationProperties().Single().Multiplicity(), EdmMultiplicity.Many);

            var version = model.SchemaElements.OfType<IEdmComplexType>().Where(t => t.Name == "ProductVersion").SingleOrDefault();
            Assert.NotNull(version);
            Assert.Equal(version.StructuralProperties().OrderBy(p => p.Name).Select(p => p.Name), new string[] { "Major", "Minor" });
            Assert.Equal(version.StructuralProperties().OrderBy(p => p.Name).Select(p => (p.Type.Definition as IEdmSchemaType).Name), new string[] { "Int32", "Int32" });
            Assert.Equal(version.StructuralProperties().OrderBy(p => p.Name).Select(p => p.Type.IsNullable), new bool[] { false, false });
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
        public int ID { get; set; }

        public string Name { get; set; }

        public ICollection<Product> Products { get; set; }
    }

    public class ProductVersion
    {
        public int Major { get; set; }

        public int Minor { get; set; }
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
        public int IdOfCategory { get; set; }

        public string Name { get; set; }

        public ICollection<ProductWithKeyAttribute> Products { get; set; }
    }
}
