// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.OData.Builder;
using System.Web.OData.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;
using Microsoft.TestCommon.Types;

namespace System.Web.OData.Query.Validators
{
    public class CountQueryValidatorTest
    {
        private readonly CountQueryValidator _validator = new CountQueryValidator();

        [Fact]
        public void Validate_Throws_NullOption()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _validator.Validate(null, new ODataValidationSettings()));
        }

        [Fact]
        public void Validate_Throws_NullSettings()
        {
            // Arrange
            ODataQueryContext context = ValidationTestHelper.CreateCustomerContext();
            var option = new CountQueryOption("Name eq 'abc'", context);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _validator.Validate(option, null));
        }

        [Theory]
        [InlineData("LimitedEntities(1)/Integers", "The property 'Integers' cannot be used for $count.")]
        [InlineData("LimitedEntities(1)/ComplexCollectionProperty", "The property 'ComplexCollectionProperty' cannot be used for $count.")]
        [InlineData("LimitedEntities(1)/EntityCollectionProperty", "The property 'EntityCollectionProperty' cannot be used for $count.")]
        [InlineData("LimitedEntities(1)/ComplexProperty/Strings", "The property 'Strings' cannot be used for $count.")]
        [InlineData("LimitedEntities(1)/ComplexProperty/SimpleEnums", "The property 'SimpleEnums' cannot be used for $count.")]
        [InlineData("LimitedEntities(1)/EntityCollectionProperty(1)/ComplexCollectionProperty", "The property 'ComplexCollectionProperty' cannot be used for $count.")]
        [InlineData("LimitedEntities(1)/Integers/$count", "The property 'Integers' cannot be used for $count.")]
        [InlineData("LimitedEntities(1)/ComplexCollectionProperty/$count", "The property 'ComplexCollectionProperty' cannot be used for $count.")]
        [InlineData("LimitedEntities(1)/EntityCollectionProperty/$count", "The property 'EntityCollectionProperty' cannot be used for $count.")]
        [InlineData("LimitedEntities(1)/ComplexProperty/Strings/$count", "The property 'Strings' cannot be used for $count.")]
        [InlineData("LimitedEntities(1)/ComplexProperty/SimpleEnums/$count", "The property 'SimpleEnums' cannot be used for $count.")]
        [InlineData("LimitedEntities(1)/EntityCollectionProperty(1)/ComplexCollectionProperty/$count", "The property 'ComplexCollectionProperty' cannot be used for $count.")]
        public void Validate_Throws_DollarCountAppliedOnNotCountableCollection(string uri, string message)
        {
            // Arrange
            IEdmModel model = GetEdmModel();
            var pathHandler = new DefaultODataPathHandler();
            string serviceRoot = "http://localhost/";
            ODataPath path = pathHandler.Parse(model, serviceRoot, uri);
            var context = new ODataQueryContext(
                model,
                EdmCoreModel.Instance.GetInt32(false).Definition,
                path);
            var option = new CountQueryOption("true", context);
            var settings = new ODataValidationSettings();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _validator.Validate(option, settings), message);
        }

        private static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = new ODataModelBuilder();

            // Configure LimitedEntity
            EntitySetConfiguration<LimitedEntity> limitedEntities = builder.EntitySet<LimitedEntity>("LimitedEntities");
            limitedEntities.EntityType.HasKey(p => p.Id);
            limitedEntities.EntityType.ComplexProperty(c => c.ComplexProperty);
            limitedEntities.EntityType.CollectionProperty(c => c.ComplexCollectionProperty).IsNotCountable();
            limitedEntities.EntityType.HasMany(l => l.EntityCollectionProperty).IsNotCountable();
            limitedEntities.EntityType.CollectionProperty(cp => cp.Integers).IsNotCountable();

            // Configure LimitedRelatedEntity
            EntitySetConfiguration<LimitedRelatedEntity> limitedRelatedEntities =
                builder.EntitySet<LimitedRelatedEntity>("LimitedRelatedEntities");
            limitedRelatedEntities.EntityType.HasKey(p => p.Id);
            limitedRelatedEntities.EntityType.CollectionProperty(p => p.ComplexCollectionProperty).IsNotCountable();

            // Configure Complextype
            ComplexTypeConfiguration<LimitedComplex> complexType = builder.ComplexType<LimitedComplex>();
            complexType.CollectionProperty(p => p.Strings).IsNotCountable();
            complexType.Property(p => p.Value);
            complexType.CollectionProperty(p => p.SimpleEnums).IsNotCountable();

            // Configure EnumType
            EnumTypeConfiguration<SimpleEnum> enumType = builder.EnumType<SimpleEnum>();
            enumType.Member(SimpleEnum.First);
            enumType.Member(SimpleEnum.Second);
            enumType.Member(SimpleEnum.Third);
            enumType.Member(SimpleEnum.Fourth);

            return builder.GetEdmModel();
        }

        private class LimitedEntity
        {
            public int Id { get; set; }
            public LimitedComplex ComplexProperty { get; set; }
            public LimitedComplex[] ComplexCollectionProperty { get; set; }
            public IEnumerable<LimitedRelatedEntity> EntityCollectionProperty { get; set; }
            public ICollection<int> Integers { get; set; }
        }

        private class LimitedComplex
        {
            public int Value { get; set; }
            public string[] Strings { get; set; }
            public IList<SimpleEnum> SimpleEnums { get; set; }
        }

        private class LimitedRelatedEntity
        {
            public int Id { get; set; }
            public LimitedComplex[] ComplexCollectionProperty { get; set; }
        }
    }
}