// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Formatter.Serialization.Models;
using Microsoft.Data.Edm;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Builder
{
    public class EntitySetLinkBuilderAnnotationTest
    {
        private ODataModelBuilder _modelBuilder;
        private EntitySetConfiguration _entitySet;

        public static TheoryDataSet<object> AllODataMetadataLevels
        {
            get
            {
                return new TheoryDataSet<object>
                {
                    ODataMetadataLevel.Default,
                    ODataMetadataLevel.FullMetadata,
                    ODataMetadataLevel.MinimalMetadata,
                    ODataMetadataLevel.NoMetadata,
                };
            }
        }

        public EntitySetLinkBuilderAnnotationTest()
        {
            _modelBuilder = new ODataModelBuilder();
            _entitySet = _modelBuilder.AddEntitySet("Customers", _modelBuilder.AddEntity(typeof(Customer)));
        }

        [Fact]
        public void Ctor_Throws_ArgumentNull_EntitySet()
        {
            Assert.ThrowsArgumentNull(() => new EntitySetLinkBuilderAnnotation(entitySet: null), "entitySet");
        }

        [Theory]
        [PropertyData("AllODataMetadataLevels")]
        public void BuildIdLink_Throws_IfIdLinkBuilderIsNull(object metadataLevel)
        {
            EntitySetLinkBuilderAnnotation linkBuilder = new EntitySetLinkBuilderAnnotation(_entitySet);
            Assert.Throws<InvalidOperationException>(
                () => linkBuilder.BuildIdLink(new EntityInstanceContext(), (ODataMetadataLevel)metadataLevel),
                "No IdLink factory was found. Try calling HasIdLink on the EntitySetConfiguration for 'Customers'.");
        }

        [Theory]
        [InlineData(true, ODataMetadataLevel.Default, true)]
        [InlineData(true, ODataMetadataLevel.FullMetadata, true)]
        [InlineData(true, ODataMetadataLevel.MinimalMetadata, false)]
        [InlineData(true, ODataMetadataLevel.NoMetadata, false)]
        [InlineData(false, ODataMetadataLevel.Default, true)]
        [InlineData(false, ODataMetadataLevel.FullMetadata, true)]
        [InlineData(false, ODataMetadataLevel.MinimalMetadata, true)]
        [InlineData(false, ODataMetadataLevel.NoMetadata, false)]
        public void BuildIdLink(bool followsConventions, object metadataLevel, bool linkEmitted)
        {
            _entitySet.HasIdLink(new SelfLinkBuilder<string>((context) => "http://selflink", followsConventions));
            EntitySetLinkBuilderAnnotation linkBuilder = new EntitySetLinkBuilderAnnotation(_entitySet);

            string generatedIdLink = linkBuilder.BuildIdLink(new EntityInstanceContext(), (ODataMetadataLevel)metadataLevel);

            if (linkEmitted)
            {
                Assert.Equal("http://selflink", generatedIdLink);
            }
            else
            {
                Assert.Null(generatedIdLink);
            }
        }

        [Theory]
        [InlineData(true, ODataMetadataLevel.Default, true)]
        [InlineData(true, ODataMetadataLevel.FullMetadata, false)]
        [InlineData(true, ODataMetadataLevel.MinimalMetadata, false)]
        [InlineData(true, ODataMetadataLevel.NoMetadata, false)]
        [InlineData(false, ODataMetadataLevel.Default, true)]
        [InlineData(false, ODataMetadataLevel.FullMetadata, false)]
        [InlineData(false, ODataMetadataLevel.MinimalMetadata, false)]
        [InlineData(false, ODataMetadataLevel.NoMetadata, false)]
        public void BuildEditLink_WhenEditLinkIsSameAsIdLink_And_IsNotSet(bool followsConventions, object metadataLevel, bool linkEmitted)
        {
            _entitySet.HasIdLink(new SelfLinkBuilder<string>((context) => "http://selflink/", followsConventions));
            EntitySetLinkBuilderAnnotation linkBuilder = new EntitySetLinkBuilderAnnotation(_entitySet);

            Uri generatedEditLink = linkBuilder.BuildEditLink(new EntityInstanceContext(), (ODataMetadataLevel)metadataLevel, "http://selflink");

            if (linkEmitted)
            {
                Assert.Equal("http://selflink/", generatedEditLink.AbsoluteUri);
            }
            else
            {
                Assert.Null(generatedEditLink);
            }
        }

        [Theory]
        [InlineData(true, ODataMetadataLevel.Default, true)]
        [InlineData(true, ODataMetadataLevel.FullMetadata, true)]
        [InlineData(true, ODataMetadataLevel.MinimalMetadata, false)]
        [InlineData(true, ODataMetadataLevel.NoMetadata, false)]
        [InlineData(false, ODataMetadataLevel.Default, true)]
        [InlineData(false, ODataMetadataLevel.FullMetadata, true)]
        [InlineData(false, ODataMetadataLevel.MinimalMetadata, true)]
        [InlineData(false, ODataMetadataLevel.NoMetadata, false)]
        public void BuildEditLink_WhenEditLinkIsNotSameAsIdLink(bool followsConventions, object metadataLevel, bool linkEmitted)
        {
            _entitySet.HasEditLink(new SelfLinkBuilder<Uri>((context) => new Uri("http://editlink/"), followsConventions));
            EntitySetLinkBuilderAnnotation linkBuilder = new EntitySetLinkBuilderAnnotation(_entitySet);

            Uri generatedEditLink = linkBuilder.BuildEditLink(new EntityInstanceContext(), (ODataMetadataLevel)metadataLevel, "http://selflink");

            if (linkEmitted)
            {
                Assert.Equal("http://editlink/", generatedEditLink.AbsoluteUri);
            }
            else
            {
                Assert.Null(generatedEditLink);
            }
        }

        [Theory]
        [InlineData(true, ODataMetadataLevel.Default, true)]
        [InlineData(true, ODataMetadataLevel.FullMetadata, false)]
        [InlineData(true, ODataMetadataLevel.MinimalMetadata, false)]
        [InlineData(true, ODataMetadataLevel.NoMetadata, false)]
        [InlineData(false, ODataMetadataLevel.Default, true)]
        [InlineData(false, ODataMetadataLevel.FullMetadata, false)]
        [InlineData(false, ODataMetadataLevel.MinimalMetadata, false)]
        [InlineData(false, ODataMetadataLevel.NoMetadata, false)]
        public void BuildReadLink_WhenReadLinkIsSameAsEditLink_And_IsNotSet(bool followsConventions, object metadataLevel, bool linkEmitted)
        {
            _entitySet.HasEditLink(new SelfLinkBuilder<Uri>((context) => new Uri("http://editlink/"), followsConventions));
            EntitySetLinkBuilderAnnotation linkBuilder = new EntitySetLinkBuilderAnnotation(_entitySet);

            Uri generatedReadLink = linkBuilder.BuildReadLink(new EntityInstanceContext(), (ODataMetadataLevel)metadataLevel, new Uri("http://editLink"));

            if (linkEmitted)
            {
                Assert.Equal("http://editlink/", generatedReadLink.AbsoluteUri);
            }
            else
            {
                Assert.Null(generatedReadLink);
            }
        }

        [Theory]
        [InlineData(true, ODataMetadataLevel.Default, true)]
        [InlineData(true, ODataMetadataLevel.FullMetadata, true)]
        [InlineData(true, ODataMetadataLevel.MinimalMetadata, false)]
        [InlineData(true, ODataMetadataLevel.NoMetadata, false)]
        [InlineData(false, ODataMetadataLevel.Default, true)]
        [InlineData(false, ODataMetadataLevel.FullMetadata, true)]
        [InlineData(false, ODataMetadataLevel.MinimalMetadata, true)]
        [InlineData(false, ODataMetadataLevel.NoMetadata, false)]
        public void BuildReadLink_WhenReadLinkIsNotSameAsEditLink(bool followsConventions, object metadataLevel, bool linkEmitted)
        {
            _entitySet.HasReadLink(new SelfLinkBuilder<Uri>((context) => new Uri("http://readlink/"), followsConventions));
            EntitySetLinkBuilderAnnotation linkBuilder = new EntitySetLinkBuilderAnnotation(_entitySet);

            Uri generatedReadLink = linkBuilder.BuildReadLink(new EntityInstanceContext(), (ODataMetadataLevel)metadataLevel, new Uri("http://editLink"));

            if (linkEmitted)
            {
                Assert.Equal("http://readlink/", generatedReadLink.AbsoluteUri);
            }
            else
            {
                Assert.Null(generatedReadLink);
            }
        }

        [Theory]
        [InlineData(true, ODataMetadataLevel.Default, true)]
        [InlineData(true, ODataMetadataLevel.FullMetadata, true)]
        [InlineData(true, ODataMetadataLevel.MinimalMetadata, false)]
        [InlineData(true, ODataMetadataLevel.NoMetadata, false)]
        [InlineData(false, ODataMetadataLevel.Default, true)]
        [InlineData(false, ODataMetadataLevel.FullMetadata, true)]
        [InlineData(false, ODataMetadataLevel.MinimalMetadata, true)]
        [InlineData(false, ODataMetadataLevel.NoMetadata, false)]
        public void BuildNavigationLink(bool followsConventions, object metadataLevel, bool linkEmitted)
        {
            var navigationProperty = _entitySet.EntityType.AddNavigationProperty(typeof(Customer).GetProperty("Orders"), EdmMultiplicity.Many);
            IEdmModel model = _modelBuilder.GetEdmModel();
            IEdmNavigationProperty edmNavProperty = model.GetEdmTypeReference(typeof(Customer)).AsEntity().DeclaredNavigationProperties().Single(p => p.Name == "Orders");

            EntitySetLinkBuilderAnnotation linkBuilder = new EntitySetLinkBuilderAnnotation(_entitySet);
            linkBuilder.AddNavigationPropertyLinkBuilder(edmNavProperty, new NavigationLinkBuilder((context, property) => new Uri("http://navigationlink"), followsConventions));

            Uri generatedNavigationLink = linkBuilder.BuildNavigationLink(new EntityInstanceContext(), edmNavProperty, (ODataMetadataLevel)metadataLevel);

            if (linkEmitted)
            {
                Assert.Equal("http://navigationlink/", generatedNavigationLink.AbsoluteUri);
            }
            else
            {
                Assert.Null(generatedNavigationLink);
            }
        }

        [Fact]
        public void CanConfigureIdLinkToNotFollowConventions()
        {
            string idLink = "http://id_link";

            _entitySet.HasIdLink(new SelfLinkBuilder<string>((ctxt) => idLink, followsConventions: false));

            var selfLinks = new EntitySetLinkBuilderAnnotation(_entitySet).BuildEntitySelfLinks(new EntityInstanceContext(), ODataMetadataLevel.MinimalMetadata);

            Assert.Equal(idLink, selfLinks.IdLink);
            Assert.Null(selfLinks.EditLink);
            Assert.Null(selfLinks.ReadLink);
        }

        [Fact]
        public void CanConfigureEditLinkToNotFollowConventions()
        {
            string idLink = "http://id_link";
            Uri editLink = new Uri("http://edit_link");

            _entitySet.HasIdLink(new SelfLinkBuilder<string>((ctxt) => idLink, followsConventions: true));
            _entitySet.HasEditLink(new SelfLinkBuilder<Uri>((ctxt) => editLink, followsConventions: false));

            var selfLinks = new EntitySetLinkBuilderAnnotation(_entitySet).BuildEntitySelfLinks(new EntityInstanceContext(), ODataMetadataLevel.MinimalMetadata);

            Assert.Null(selfLinks.IdLink);
            Assert.Equal(editLink, selfLinks.EditLink);
            Assert.Null(selfLinks.ReadLink);
        }

        [Fact]
        public void CanConfigureReadLinkToNotFollowConventions()
        {
            string idLink = "http://id_link";
            Uri readLink = new Uri("http://read_link");

            _entitySet.HasIdLink(new SelfLinkBuilder<string>((ctxt) => idLink, followsConventions: true));
            _entitySet.HasReadLink(new SelfLinkBuilder<Uri>((ctxt) => readLink, followsConventions: false));

            var selfLinks = new EntitySetLinkBuilderAnnotation(_entitySet).BuildEntitySelfLinks(new EntityInstanceContext(), ODataMetadataLevel.MinimalMetadata);

            Assert.Null(selfLinks.IdLink);
            Assert.Null(selfLinks.EditLink);
            Assert.Equal(readLink, selfLinks.ReadLink);
        }
    }
}
