//-----------------------------------------------------------------------------
// <copyright file="NavigationSourceLinkBuilderAnnotationTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Builder
{
    public class NavigationSourceLinkBuilderAnnotationTest
    {
        private ODataModelBuilder _modelBuilder;
        private EntitySetConfiguration _entitySet;

        public static TheoryDataSet<ODataMetadataLevel> AllODataMetadataLevels
        {
            get
            {
                return new TheoryDataSet<ODataMetadataLevel>
                {
                    ODataMetadataLevel.FullMetadata,
                    ODataMetadataLevel.MinimalMetadata,
                    ODataMetadataLevel.NoMetadata,
                };
            }
        }

        public NavigationSourceLinkBuilderAnnotationTest()
        {
            _modelBuilder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();
            _entitySet = _modelBuilder.AddEntitySet("Customers", _modelBuilder.AddEntityType(typeof(Customer)));
        }

        [Fact]
        public void Ctor_Throws_ArgumentNull_NavigationSource()
        {
            ExceptionAssert.ThrowsArgumentNull(() => new NavigationSourceLinkBuilderAnnotation(navigationSource: null), "navigationSource");
        }

        [Theory]
        [InlineData(ODataMetadataLevel.FullMetadata)]
        [InlineData(ODataMetadataLevel.MinimalMetadata)]
        [InlineData(ODataMetadataLevel.NoMetadata)]
        public void BuildIdLink_DoesNotThrow_IfJsonAndIdLinkBuilderIsNull(object metadataLevel)
        {
            NavigationSourceLinkBuilderAnnotation linkBuilder = new NavigationSourceLinkBuilderAnnotation(_entitySet);
            ExceptionAssert.DoesNotThrow(
                () => linkBuilder.BuildIdLink(new ResourceContext(), (ODataMetadataLevel)metadataLevel));
        }

        [Theory]
        [InlineData(true, ODataMetadataLevel.FullMetadata, true)]
        [InlineData(true, ODataMetadataLevel.MinimalMetadata, false)]
        [InlineData(true, ODataMetadataLevel.NoMetadata, false)]
        [InlineData(false, ODataMetadataLevel.FullMetadata, true)]
        [InlineData(false, ODataMetadataLevel.MinimalMetadata, true)]
        [InlineData(false, ODataMetadataLevel.NoMetadata, false)]
        public void BuildIdLink(bool followsConventions, ODataMetadataLevel metadataLevel, bool linkEmitted)
        {
            // Arrange
            _entitySet.HasIdLink(new SelfLinkBuilder<Uri>((context) => new Uri("http://selflink"), followsConventions));
            NavigationSourceLinkBuilderAnnotation linkBuilder = new NavigationSourceLinkBuilderAnnotation(_entitySet);

            // Act
            Uri generatedIdLink = linkBuilder.BuildIdLink(new ResourceContext(), (ODataMetadataLevel)metadataLevel);

            // Assert
            if (linkEmitted)
            {
                Assert.Equal(new Uri("http://selflink"), generatedIdLink);
            }
            else
            {
                Assert.Null(generatedIdLink);
            }
        }

        [Theory]
        [InlineData(true, ODataMetadataLevel.FullMetadata)]
        [InlineData(true, ODataMetadataLevel.MinimalMetadata)]
        [InlineData(true, ODataMetadataLevel.NoMetadata)]
        [InlineData(false, ODataMetadataLevel.FullMetadata)]
        [InlineData(false, ODataMetadataLevel.MinimalMetadata)]
        [InlineData(false, ODataMetadataLevel.NoMetadata)]
        public void BuildEditLink_WhenEditLinkIsSameAsIdLink_And_IsNotSet(bool followsConventions, ODataMetadataLevel metadataLevel)
        {
            // Arrange
            _entitySet.HasIdLink(new SelfLinkBuilder<Uri>((context) => new Uri("http://selflink/"), followsConventions));
            NavigationSourceLinkBuilderAnnotation linkBuilder = new NavigationSourceLinkBuilderAnnotation(_entitySet);

            // Act
            Uri generatedEditLink = linkBuilder.BuildEditLink(new ResourceContext(), (ODataMetadataLevel)metadataLevel, new Uri("http://selflink"));

            // Assert
            Assert.Null(generatedEditLink);
        }

        [Theory]
        [InlineData(true, ODataMetadataLevel.FullMetadata, true)]
        [InlineData(true, ODataMetadataLevel.MinimalMetadata, false)]
        [InlineData(true, ODataMetadataLevel.NoMetadata, false)]
        [InlineData(false, ODataMetadataLevel.FullMetadata, true)]
        [InlineData(false, ODataMetadataLevel.MinimalMetadata, true)]
        [InlineData(false, ODataMetadataLevel.NoMetadata, false)]
        public void BuildEditLink_WhenEditLinkIsNotSameAsIdLink(bool followsConventions, ODataMetadataLevel metadataLevel, bool linkEmitted)
        {
            // Arrange
            _entitySet.HasEditLink(new SelfLinkBuilder<Uri>((context) => new Uri("http://editlink/"), followsConventions));
            NavigationSourceLinkBuilderAnnotation linkBuilder = new NavigationSourceLinkBuilderAnnotation(_entitySet);

            // Act
            Uri generatedEditLink = linkBuilder.BuildEditLink(new ResourceContext(), metadataLevel, new Uri("http://selflink"));

            // Assert
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
        [InlineData(true, ODataMetadataLevel.FullMetadata)]
        [InlineData(true, ODataMetadataLevel.MinimalMetadata)]
        [InlineData(true, ODataMetadataLevel.NoMetadata)]
        [InlineData(false, ODataMetadataLevel.FullMetadata)]
        [InlineData(false, ODataMetadataLevel.MinimalMetadata)]
        [InlineData(false, ODataMetadataLevel.NoMetadata)]
        public void BuildReadLink_WhenReadLinkIsSameAsEditLink_And_IsNotSet(bool followsConventions, ODataMetadataLevel metadataLevel)
        {
            // Arrange
            _entitySet.HasEditLink(new SelfLinkBuilder<Uri>((context) => new Uri("http://editlink/"), followsConventions));
            NavigationSourceLinkBuilderAnnotation linkBuilder = new NavigationSourceLinkBuilderAnnotation(_entitySet);

            // Act
            Uri generatedReadLink = linkBuilder.BuildReadLink(new ResourceContext(), (ODataMetadataLevel)metadataLevel, new Uri("http://editLink"));

            // Assert
            Assert.Null(generatedReadLink);
        }

        [Theory]
        [InlineData(true, ODataMetadataLevel.FullMetadata, true)]
        [InlineData(true, ODataMetadataLevel.MinimalMetadata, false)]
        [InlineData(true, ODataMetadataLevel.NoMetadata, false)]
        [InlineData(false, ODataMetadataLevel.FullMetadata, true)]
        [InlineData(false, ODataMetadataLevel.MinimalMetadata, true)]
        [InlineData(false, ODataMetadataLevel.NoMetadata, false)]
        public void BuildReadLink_WhenReadLinkIsNotSameAsEditLink(bool followsConventions, ODataMetadataLevel metadataLevel, bool linkEmitted)
        {
            // Arrange
            _entitySet.HasReadLink(new SelfLinkBuilder<Uri>((context) => new Uri("http://readlink/"), followsConventions));
            NavigationSourceLinkBuilderAnnotation linkBuilder = new NavigationSourceLinkBuilderAnnotation(_entitySet);

            // Act
            Uri generatedReadLink = linkBuilder.BuildReadLink(new ResourceContext(), metadataLevel, new Uri("http://editLink"));

            // Assert
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
        [InlineData(false, ODataMetadataLevel.MinimalMetadata)]
        [InlineData(false, ODataMetadataLevel.FullMetadata)]
        public void BuildNavigationLink_ReturnsTheNavigationLinkUri(bool followsConventions, ODataMetadataLevel metadataLevel)
        {
            // Arrange
            var navigationProperty = _entitySet.EntityType.AddNavigationProperty(typeof(Customer).GetProperty("Orders"), EdmMultiplicity.Many);
            IEdmModel model = _modelBuilder.GetEdmModel();
            IEdmNavigationProperty edmNavProperty = model.GetEdmTypeReference(typeof(Customer)).AsEntity().DeclaredNavigationProperties().Single(p => p.Name == "Orders");

            NavigationSourceLinkBuilderAnnotation linkBuilder = new NavigationSourceLinkBuilderAnnotation(_entitySet);
            linkBuilder.AddNavigationPropertyLinkBuilder(edmNavProperty, new NavigationLinkBuilder((context, property) => new Uri("http://navigationlink"), followsConventions));

            // Act
            Uri generatedNavigationLink = linkBuilder.BuildNavigationLink(new ResourceContext(), edmNavProperty, (ODataMetadataLevel)metadataLevel);

            // Assert
            Assert.Equal("http://navigationlink/", generatedNavigationLink.AbsoluteUri);
        }

        [Theory]
        [InlineData(true, ODataMetadataLevel.FullMetadata)]
        [InlineData(true, ODataMetadataLevel.MinimalMetadata)]
        [InlineData(true, ODataMetadataLevel.NoMetadata)]
        [InlineData(false, ODataMetadataLevel.NoMetadata)]
        public void BuildNavigationLink_ReturnsNull(bool followsConventions, ODataMetadataLevel metadataLevel)
        {
            // Arrange
            var navigationProperty = _entitySet.EntityType.AddNavigationProperty(typeof(Customer).GetProperty("Orders"), EdmMultiplicity.Many);
            IEdmModel model = _modelBuilder.GetEdmModel();
            IEdmNavigationProperty edmNavProperty = model.GetEdmTypeReference(typeof(Customer)).AsEntity().DeclaredNavigationProperties().Single(p => p.Name == "Orders");

            NavigationSourceLinkBuilderAnnotation linkBuilder = new NavigationSourceLinkBuilderAnnotation(_entitySet);
            linkBuilder.AddNavigationPropertyLinkBuilder(edmNavProperty, new NavigationLinkBuilder((context, property) => new Uri("http://navigationlink"), followsConventions));

            // Act
            Uri generatedNavigationLink = linkBuilder.BuildNavigationLink(new ResourceContext(), edmNavProperty, (ODataMetadataLevel)metadataLevel);

            // Assert
            Assert.Null(generatedNavigationLink);
        }

        [Fact]
        public void CanConfigureIdLinkToNotFollowConventions()
        {
            // Arrange
            Uri idLink = new Uri("http://id_link");
            _entitySet.HasIdLink(new SelfLinkBuilder<Uri>((ctxt) => idLink, followsConventions: false));

            // Act
            var selfLinks = new NavigationSourceLinkBuilderAnnotation(_entitySet).BuildEntitySelfLinks(new ResourceContext(), ODataMetadataLevel.MinimalMetadata);

            // Assert
            Assert.Equal(idLink, selfLinks.IdLink);
            Assert.Null(selfLinks.EditLink);
            Assert.Null(selfLinks.ReadLink);
        }

        [Fact]
        public void CanConfigureEditLinkToNotFollowConventions()
        {
            // Arrange
            Uri idLink = new Uri("http://id_link");
            Uri editLink = new Uri("http://edit_link");

            _entitySet.HasIdLink(new SelfLinkBuilder<Uri>((ctxt) => idLink, followsConventions: true));
            _entitySet.HasEditLink(new SelfLinkBuilder<Uri>((ctxt) => editLink, followsConventions: false));

            // Act
            var selfLinks = new NavigationSourceLinkBuilderAnnotation(_entitySet).BuildEntitySelfLinks(new ResourceContext(), ODataMetadataLevel.MinimalMetadata);

            // Assert
            Assert.Null(selfLinks.IdLink);
            Assert.Equal(editLink, selfLinks.EditLink);
            Assert.Null(selfLinks.ReadLink);
        }

        [Fact]
        public void CanConfigureReadLinkToNotFollowConventions()
        {
            // Arrange
            Uri idLink = new Uri("http://id_link");
            Uri readLink = new Uri("http://read_link");

            _entitySet.HasIdLink(new SelfLinkBuilder<Uri>((ctxt) => idLink, followsConventions: true));
            _entitySet.HasReadLink(new SelfLinkBuilder<Uri>((ctxt) => readLink, followsConventions: false));

            // Act
            var selfLinks = new NavigationSourceLinkBuilderAnnotation(_entitySet).BuildEntitySelfLinks(new ResourceContext(), ODataMetadataLevel.MinimalMetadata);

            // Assert
            Assert.Null(selfLinks.IdLink);
            Assert.Null(selfLinks.EditLink);
            Assert.Equal(readLink, selfLinks.ReadLink);
        }

        [Fact]
        public void Ctor_FollowingConventions_GeneratesSelfLinkWithCast_IfDerivedTypeHasNavigationProperty()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            var request = RequestFactory.CreateFromModel(model.Model);
            ODataSerializerContext serializerContext = ODataSerializerContextFactory.Create(model.Model, model.Customers, request);
            ResourceContext instanceContext = new ResourceContext(serializerContext, model.SpecialCustomer.AsReference(), new { ID = 42 });

            // Act
            NavigationSourceLinkBuilderAnnotation linkBuilder = new NavigationSourceLinkBuilderAnnotation(model.Customers, model.Model);
            var result = linkBuilder.BuildIdLink(instanceContext, ODataMetadataLevel.FullMetadata);

            // Assert
            Assert.Equal("http://localhost/Customers(42)/NS.SpecialCustomer", result.ToString());
        }

        [Fact]
        public void Ctor_FollowingConventions_GeneratesSelfLinkWithoutCast_IfDerivedTypesHaveNoNavigationProperty()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            IEdmEntitySet specialCustomers = new EdmEntitySet(model.Container, "SpecialCustomers", model.SpecialCustomer);
            var request = RequestFactory.CreateFromModel(model.Model);
            ODataSerializerContext serializerContext = ODataSerializerContextFactory.Create(model.Model, specialCustomers, request);
            ResourceContext instanceContext = new ResourceContext(serializerContext, model.Customer.AsReference(), new { ID = 42 });

            // Act
            NavigationSourceLinkBuilderAnnotation linkBuilder = new NavigationSourceLinkBuilderAnnotation(specialCustomers, model.Model);
            var result = linkBuilder.BuildIdLink(instanceContext, ODataMetadataLevel.FullMetadata);

            // Assert
            Assert.Equal("http://localhost/SpecialCustomers(42)", result.ToString());
        }

        [Fact]
        public void Ctor_FollowingConventions_GeneratesNavigationLinkWithoutCast_ForNavigationPropertyOnBaseType()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            var request = RequestFactory.CreateFromModel(model.Model);
            ODataSerializerContext serializerContext = ODataSerializerContextFactory.Create(model.Model, model.Customers, request);
            ResourceContext instanceContext = new ResourceContext(serializerContext, model.Customer.AsReference(), new { ID = 42 });
            IEdmNavigationProperty ordersProperty = model.Customer.NavigationProperties().First(p => p.Name == "Orders");

            // Act
            NavigationSourceLinkBuilderAnnotation linkBuilder = new NavigationSourceLinkBuilderAnnotation(model.Customers, model.Model);
            linkBuilder.AddNavigationPropertyLinkBuilder(ordersProperty, new NavigationLinkBuilder((context, property) => context.GenerateNavigationPropertyLink(property, includeCast: false), false));
            Uri result = linkBuilder.BuildNavigationLink(instanceContext, ordersProperty, ODataMetadataLevel.MinimalMetadata);

            // Assert
            Assert.Equal("http://localhost/Customers(42)/Orders", result.AbsoluteUri);
        }

        [Fact]
        public void Ctor_FollowingConventions_GeneratesNavigationLinkWithCast_ForDerivedNavigationProperty()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            var request = RequestFactory.CreateFromModel(model.Model);
            ODataSerializerContext serializerContext = ODataSerializerContextFactory.Create(model.Model, model.Customers, request);
            ResourceContext instanceContext = new ResourceContext(serializerContext, model.SpecialCustomer.AsReference(), new { ID = 42 });
            IEdmNavigationProperty ordersProperty = model.SpecialCustomer.NavigationProperties().First(p => p.Name == "SpecialOrders");

            // Act
            NavigationSourceLinkBuilderAnnotation linkBuilder = new NavigationSourceLinkBuilderAnnotation(model.Customers, model.Model);
            linkBuilder.AddNavigationPropertyLinkBuilder(ordersProperty, new NavigationLinkBuilder((context, property) => context.GenerateNavigationPropertyLink(property, includeCast: true), false));
            Uri result = linkBuilder.BuildNavigationLink(instanceContext, ordersProperty, ODataMetadataLevel.MinimalMetadata);

            // Assert
            Assert.Equal("http://localhost/Customers(42)/NS.SpecialCustomer/SpecialOrders", result.AbsoluteUri);
        }
    }
}
