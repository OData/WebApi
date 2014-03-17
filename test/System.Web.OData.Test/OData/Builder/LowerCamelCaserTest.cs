// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web.OData.Builder.TestModels;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;

namespace System.Web.OData.Builder
{
    public class LowerCamelCaserTest
    {
        [Theory]
        [InlineData("", "")]
        [InlineData("a", "a")]
        [InlineData("B", "b")]
        [InlineData("ab", "ab")]
        [InlineData("Ab", "ab")]
        [InlineData("aBcd", "aBcd")]
        [InlineData("AbCD", "abCD")]
        [InlineData("AbcDE", "abcDE")]
        [InlineData("AbcD", "abcD")]
        [InlineData("ABCdef", "abCdef")]
        [InlineData("A1B2", "a1B2")]
        [InlineData("A_BC", "a_BC")]
        [InlineData("_123", "_123")]
        [InlineData("_abc", "_abc")]
        [InlineData("_ABC", "_ABC")]
        [InlineData("aB_Cd", "aB_Cd")]
        [InlineData("AB_CD", "ab_CD")]
        [InlineData("AB12", "ab12")]
        [InlineData("ABCD", "abcd")]
        [InlineData("ID", "id")]
        [InlineData("Id", "id")]
        [InlineData("id", "id")]
        [InlineData("IOStream", "ioStream")]
        [InlineData("ID1", "id1")]
        [InlineData("MyId", "myId")]
        [InlineData("YourId", "yourId")]
        public void ToLowerCamelCase_LowerCamelCaser(string propertyName, string expectName)
        {
            // Arrange
            var lowerCamelCaser = new LowerCamelCaser();

            // Act
            string actualName = lowerCamelCaser.ToLowerCamelCase(propertyName);

            // Assert
            Assert.Equal(expectName, actualName);
        }

        [Fact]
        public void LowerCamelCaser_RespectModelAliasing()
        {
            // Arrange
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<LowerCamelCaserModelAliasEntity>("Entities");
            builder.OnModelCreating += new LowerCamelCaser(NameResolverOptions.RespectModelAliasing).Apply;
            
            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmEntityType lowerCamelCaserEntity = 
                Assert.Single(model.SchemaElements.OfType<IEdmEntityType>().Where(e => e.Name == "LowerCamelCaserModelAliasEntity"));
            Assert.Equal(4, lowerCamelCaserEntity.Properties().Count());
            Assert.Single(lowerCamelCaserEntity.Properties().Where(p => p.Name == "ID"));
            Assert.Single(lowerCamelCaserEntity.Properties().Where(p => p.Name == "name"));
            Assert.Single(lowerCamelCaserEntity.Properties().Where(p => p.Name == "Something"));
            Assert.Single(lowerCamelCaserEntity.Properties().Where(p => p.Name == "color"));
        }

        [Fact]
        public void LowerCamelCaser_RespectExplicitProperties()
        {
            // Arrange
            var builder = new ODataConventionModelBuilder();
            EntityTypeConfiguration<LowerCamelCaserEntity> entityTypeConfiguration = builder.EntitySet<LowerCamelCaserEntity>("Entities").EntityType;
            entityTypeConfiguration.Property(b => b.ID).Name = "iD";
            entityTypeConfiguration.Property(d => d.Name).Name = "Name";
            entityTypeConfiguration.EnumProperty(d => d.Color).Name = "Something";
            ComplexTypeConfiguration<LowerCamelCaserComplex> complexTypeConfiguration = builder.ComplexType<LowerCamelCaserComplex>();
            complexTypeConfiguration.CollectionProperty(c => c.Notes).Name = "MyNotes";
            builder.OnModelCreating += new LowerCamelCaser(NameResolverOptions.RespectExplicitProperties).Apply;

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmEntityType lowerCamelCaserEntity = 
                Assert.Single(model.SchemaElements.OfType<IEdmEntityType>().Where(e => e.Name == "LowerCamelCaserEntity"));
            IEdmComplexType lowerCamelCaserComplex = 
                Assert.Single(model.SchemaElements.OfType<IEdmComplexType>().Where(e => e.Name == "LowerCamelCaserComplex"));
            Assert.Equal(5, lowerCamelCaserEntity.Properties().Count());
            Assert.Single(lowerCamelCaserEntity.Properties().Where(p => p.Name == "iD"));
            Assert.Single(lowerCamelCaserEntity.Properties().Where(p => p.Name == "Name"));
            Assert.Single(lowerCamelCaserEntity.Properties().Where(p => p.Name == "details"));
            Assert.Single(lowerCamelCaserEntity.Properties().Where(p => p.Name == "Something"));
            Assert.Single(lowerCamelCaserEntity.Properties().Where(p => p.Name == "complexProperty"));
            Assert.Equal(2, lowerCamelCaserComplex.Properties().Count());
            Assert.Single(lowerCamelCaserComplex.Properties().Where(p => p.Name == "price"));
            Assert.Single(lowerCamelCaserComplex.Properties().Where(p => p.Name == "MyNotes"));
        }

        [Fact]
        public void LowerCamelCaser_RespectModelAliasingAndExplicitProperties()
        {
            // Arrange
            var builder = new ODataConventionModelBuilder();
            EntityTypeConfiguration<LowerCamelCaserModelAliasEntity> entity = builder.EntitySet<LowerCamelCaserModelAliasEntity>("Entities").EntityType;
            entity.HasKey(e => e.ID).Property(e => e.ID).Name = "IDExplicitly"; 
            entity.Property(d => d.Price).Name = "Price";
            builder.OnModelCreating += new LowerCamelCaser(NameResolverOptions.RespectModelAliasing | NameResolverOptions.RespectExplicitProperties).Apply;

            // Act
            IEdmModel model = builder.GetEdmModel();
            
            // Assert
            IEdmEntityType lowerCamelCaserModelAliasEntity =
                Assert.Single(model.SchemaElements.OfType<IEdmEntityType>().Where(e => e.Name == "LowerCamelCaserModelAliasEntity"));
            Assert.Equal(5, lowerCamelCaserModelAliasEntity.Properties().Count());
            Assert.Single(lowerCamelCaserModelAliasEntity.Properties().Where(p => p.Name == "IDExplicitly"));
            Assert.Single(lowerCamelCaserModelAliasEntity.Properties().Where(p => p.Name == "name"));
            Assert.Single(lowerCamelCaserModelAliasEntity.Properties().Where(p => p.Name == "Something"));
            Assert.Single(lowerCamelCaserModelAliasEntity.Properties().Where(p => p.Name == "color"));
            Assert.Single(lowerCamelCaserModelAliasEntity.Properties().Where(p => p.Name == "Price"));
        }
    }

    public class LowerCamelCaserEntity
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public List<string> Details { get; set; }
        public Color Color { get; set; }
        public LowerCamelCaserComplex ComplexProperty { get; set; }
    }

    public class LowerCamelCaserComplex
    {
        public double? Price { get; set; }
        public IList<string> Notes { get; set; }
    }

    [DataContract]
    public class LowerCamelCaserModelAliasEntity
    {
        [Key]
        [DataMember(Name = "ID")]
        public int ID { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "Something")]
        public List<String> Details { get; set; }

        [DataMember]
        public Color Color { get; set; }

        public double Price { get; set; }
    }
}