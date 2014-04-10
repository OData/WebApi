// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.OData.Extensions;
using System.Web.OData.Formatter;
using System.Web.OData.Formatter.Serialization;
using System.Web.OData.Formatter.Serialization.Models;
using System.Web.OData.TestCommon;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;

namespace System.Web.OData.Builder
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
                    ODataMetadataLevel.Default,
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
            Assert.ThrowsArgumentNull(() => new NavigationSourceLinkBuilderAnnotation(navigationSource: null), "navigationSource");
        }

        [Theory]
        [PropertyData("AllODataMetadataLevels")]
        public void BuildIdLink_Throws_IfIdLinkBuilderIsNull(ODataMetadataLevel metadataLevel)
        {
            // Arrange
            NavigationSourceLinkBuilderAnnotation linkBuilder = new NavigationSourceLinkBuilderAnnotation(_entitySet);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => linkBuilder.BuildIdLink(new EntityInstanceContext(), (ODataMetadataLevel)metadataLevel),
                "No IdLink factory was found. Try calling HasIdLink on the NavigationSourceConfiguration for 'Customers'.");
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
        public void BuildIdLink(bool followsConventions, ODataMetadataLevel metadataLevel, bool linkEmitted)
        {
            // Arrange
            _entitySet.HasIdLink(new SelfLinkBuilder<string>((context) => "http://selflink", followsConventions));
            NavigationSourceLinkBuilderAnnotation linkBuilder = new NavigationSourceLinkBuilderAnnotation(_entitySet);

            // Act
            string generatedIdLink = linkBuilder.BuildIdLink(new EntityInstanceContext(), (ODataMetadataLevel)metadataLevel);

            // Assert
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
        public void BuildEditLink_WhenEditLinkIsSameAsIdLink_And_IsNotSet(bool followsConventions, ODataMetadataLevel metadataLevel, bool linkEmitted)
        {
            // Arrange
            _entitySet.HasIdLink(new SelfLinkBuilder<string>((context) => "http://selflink/", followsConventions));
            NavigationSourceLinkBuilderAnnotation linkBuilder = new NavigationSourceLinkBuilderAnnotation(_entitySet);

            // Act
            Uri generatedEditLink = linkBuilder.BuildEditLink(new EntityInstanceContext(), (ODataMetadataLevel)metadataLevel, "http://selflink");

            // Assert
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
        public void BuildEditLink_WhenEditLinkIsNotSameAsIdLink(bool followsConventions, ODataMetadataLevel metadataLevel, bool linkEmitted)
        {
            // Arrange
            _entitySet.HasEditLink(new SelfLinkBuilder<Uri>((context) => new Uri("http://editlink/"), followsConventions));
            NavigationSourceLinkBuilderAnnotation linkBuilder = new NavigationSourceLinkBuilderAnnotation(_entitySet);

            // Act
            Uri generatedEditLink = linkBuilder.BuildEditLink(new EntityInstanceContext(), (ODataMetadataLevel)metadataLevel, "http://selflink");

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
        [InlineData(true, ODataMetadataLevel.Default, true)]
        [InlineData(true, ODataMetadataLevel.FullMetadata, false)]
        [InlineData(true, ODataMetadataLevel.MinimalMetadata, false)]
        [InlineData(true, ODataMetadataLevel.NoMetadata, false)]
        [InlineData(false, ODataMetadataLevel.Default, true)]
        [InlineData(false, ODataMetadataLevel.FullMetadata, false)]
        [InlineData(false, ODataMetadataLevel.MinimalMetadata, false)]
        [InlineData(false, ODataMetadataLevel.NoMetadata, false)]
        public void BuildReadLink_WhenReadLinkIsSameAsEditLink_And_IsNotSet(bool followsConventions, ODataMetadataLevel metadataLevel, bool linkEmitted)
        {
            // Arrange
            _entitySet.HasEditLink(new SelfLinkBuilder<Uri>((context) => new Uri("http://editlink/"), followsConventions));
            NavigationSourceLinkBuilderAnnotation linkBuilder = new NavigationSourceLinkBuilderAnnotation(_entitySet);

            // Act
            Uri generatedReadLink = linkBuilder.BuildReadLink(new EntityInstanceContext(), (ODataMetadataLevel)metadataLevel, new Uri("http://editLink"));

            // Assert
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
        public void BuildReadLink_WhenReadLinkIsNotSameAsEditLink(bool followsConventions, ODataMetadataLevel metadataLevel, bool linkEmitted)
        {
            // Arrange
            _entitySet.HasReadLink(new SelfLinkBuilder<Uri>((context) => new Uri("http://readlink/"), followsConventions));
            NavigationSourceLinkBuilderAnnotation linkBuilder = new NavigationSourceLinkBuilderAnnotation(_entitySet);

            // Act
            Uri generatedReadLink = linkBuilder.BuildReadLink(new EntityInstanceContext(), (ODataMetadataLevel)metadataLevel, new Uri("http://editLink"));

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
        [InlineData(true, ODataMetadataLevel.Default)]
        [InlineData(true, ODataMetadataLevel.FullMetadata)]
        [InlineData(false, ODataMetadataLevel.Default)]
        [InlineData(false, ODataMetadataLevel.FullMetadata)]
        [InlineData(false, ODataMetadataLevel.MinimalMetadata)]
        public void BuildNavigationLink_ReturnsTheNavigationLinkUri(bool followsConventions, ODataMetadataLevel metadataLevel)
        {
            // Arrange
            var navigationProperty = _entitySet.EntityType.AddNavigationProperty(typeof(Customer).GetProperty("Orders"), EdmMultiplicity.Many);
            IEdmModel model = _modelBuilder.GetEdmModel();
            IEdmNavigationProperty edmNavProperty = model.GetEdmTypeReference(typeof(Customer)).AsEntity().DeclaredNavigationProperties().Single(p => p.Name == "Orders");

            NavigationSourceLinkBuilderAnnotation linkBuilder = new NavigationSourceLinkBuilderAnnotation(_entitySet);
            linkBuilder.AddNavigationPropertyLinkBuilder(edmNavProperty, new NavigationLinkBuilder((context, property) => new Uri("http://navigationlink"), followsConventions));

            // Act
            Uri generatedNavigationLink = linkBuilder.BuildNavigationLink(new EntityInstanceContext(), edmNavProperty, (ODataMetadataLevel)metadataLevel);

            // Assert
            Assert.Equal("http://navigationlink/", generatedNavigationLink.AbsoluteUri);
        }

        [Theory]
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
            Uri generatedNavigationLink = linkBuilder.BuildNavigationLink(new EntityInstanceContext(), edmNavProperty, (ODataMetadataLevel)metadataLevel);

            // Assert
            Assert.Null(generatedNavigationLink);
        }

        [Fact]
        public void CanConfigureIdLinkToNotFollowConventions()
        {
            // Arrange
            string idLink = "http://id_link";

            _entitySet.HasIdLink(new SelfLinkBuilder<string>((ctxt) => idLink, followsConventions: false));

            // Act
            var selfLinks = new NavigationSourceLinkBuilderAnnotation(_entitySet).BuildEntitySelfLinks(new EntityInstanceContext(), ODataMetadataLevel.MinimalMetadata);

            // Assert
            Assert.Equal(idLink, selfLinks.IdLink);
            Assert.Null(selfLinks.EditLink);
            Assert.Null(selfLinks.ReadLink);
        }

        [Fact]
        public void CanConfigureEditLinkToNotFollowConventions()
        {
            // Arrange
            string idLink = "http://id_link";
            Uri editLink = new Uri("http://edit_link");

            _entitySet.HasIdLink(new SelfLinkBuilder<string>((ctxt) => idLink, followsConventions: true));
            _entitySet.HasEditLink(new SelfLinkBuilder<Uri>((ctxt) => editLink, followsConventions: false));

            // Act
            var selfLinks = new NavigationSourceLinkBuilderAnnotation(_entitySet).BuildEntitySelfLinks(new EntityInstanceContext(), ODataMetadataLevel.MinimalMetadata);

            // Assert
            Assert.Null(selfLinks.IdLink);
            Assert.Equal(editLink, selfLinks.EditLink);
            Assert.Null(selfLinks.ReadLink);
        }

        [Fact]
        public void CanConfigureReadLinkToNotFollowConventions()
        {
            // Arrange
            string idLink = "http://id_link";
            Uri readLink = new Uri("http://read_link");

            _entitySet.HasIdLink(new SelfLinkBuilder<string>((ctxt) => idLink, followsConventions: true));
            _entitySet.HasReadLink(new SelfLinkBuilder<Uri>((ctxt) => readLink, followsConventions: false));

            // Act
            var selfLinks = new NavigationSourceLinkBuilderAnnotation(_entitySet).BuildEntitySelfLinks(new EntityInstanceContext(), ODataMetadataLevel.MinimalMetadata);

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
            HttpRequestMessage request = GetODataRequest(model.Model);
            ODataSerializerContext serializerContext = new ODataSerializerContext { Model = model.Model, NavigationSource = model.Customers, Url = request.GetUrlHelper() };
            EntityInstanceContext instanceContext = new EntityInstanceContext(serializerContext, model.Customer.AsReference(), new { ID = 42 });

            // Act
            NavigationSourceLinkBuilderAnnotation linkBuilder = new NavigationSourceLinkBuilderAnnotation(model.Customers, model.Model);
            string result = linkBuilder.BuildIdLink(instanceContext, ODataMetadataLevel.Default);

            // Assert
            Assert.Equal("http://localhost/Customers(42)/NS.Customer", result);
        }

        [Fact]
        public void Ctor_FollowingConventions_GeneratesSelfLinkWithoutCast_IfDerivedTypesHaveNoNavigationProperty()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            IEdmEntitySet specialCustomers = new EdmEntitySet(model.Container, "SpecialCustomers", model.SpecialCustomer);
            HttpRequestMessage request = GetODataRequest(model.Model);
            ODataSerializerContext serializerContext = new ODataSerializerContext { Model = model.Model, NavigationSource = specialCustomers, Url = request.GetUrlHelper() };
            EntityInstanceContext instanceContext = new EntityInstanceContext(serializerContext, model.Customer.AsReference(), new { ID = 42 });

            // Act
            NavigationSourceLinkBuilderAnnotation linkBuilder = new NavigationSourceLinkBuilderAnnotation(specialCustomers, model.Model);
            string result = linkBuilder.BuildIdLink(instanceContext, ODataMetadataLevel.Default);

            // Assert
            Assert.Equal("http://localhost/SpecialCustomers(42)", result);
        }

        [Fact]
        public void Ctor_FollowingConventions_GeneratesNavigationLinkWithoutCast_ForNavigationPropertyOnBaseType()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            HttpRequestMessage request = GetODataRequest(model.Model);
            ODataSerializerContext serializerContext = new ODataSerializerContext { Model = model.Model, NavigationSource = model.Customers, Url = request.GetUrlHelper() };
            EntityInstanceContext instanceContext = new EntityInstanceContext(serializerContext, model.Customer.AsReference(), new { ID = 42 });
            IEdmNavigationProperty ordersProperty = model.Customer.NavigationProperties().First(p => p.Name == "Orders");

            // Act
            NavigationSourceLinkBuilderAnnotation linkBuilder = new NavigationSourceLinkBuilderAnnotation(model.Customers, model.Model);
            Uri result = linkBuilder.BuildNavigationLink(instanceContext, ordersProperty, ODataMetadataLevel.Default);

            // Assert
            Assert.Equal("http://localhost/Customers(42)/Orders", result.AbsoluteUri);
        }

        [Fact]
        public void Ctor_FollowingConventions_GeneratesNavigationLinkWithCast_ForDerivedNavigationProperty()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            HttpRequestMessage request = GetODataRequest(model.Model);
            ODataSerializerContext serializerContext = new ODataSerializerContext { Model = model.Model, NavigationSource = model.Customers, Url = request.GetUrlHelper() };
            EntityInstanceContext instanceContext = new EntityInstanceContext(serializerContext, model.SpecialCustomer.AsReference(), new { ID = 42 });
            IEdmNavigationProperty ordersProperty = model.SpecialCustomer.NavigationProperties().First(p => p.Name == "SpecialOrders");

            // Act
            NavigationSourceLinkBuilderAnnotation linkBuilder = new NavigationSourceLinkBuilderAnnotation(model.Customers, model.Model);
            Uri result = linkBuilder.BuildNavigationLink(instanceContext, ordersProperty, ODataMetadataLevel.Default);

            // Assert
            Assert.Equal("http://localhost/Customers(42)/NS.SpecialCustomer/SpecialOrders", result.AbsoluteUri);
        }

        [Fact]
        public void Ctor_FollowingConventions_GeneratesFeedSelfLinkFollowingConventions()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            HttpRequestMessage request = GetODataRequest(model.Model);

            // Act
            NavigationSourceLinkBuilderAnnotation linkBuilder = new NavigationSourceLinkBuilderAnnotation(model.Customers, model.Model);
            Uri result = linkBuilder.BuildFeedSelfLink(new FeedContext { EntitySet = model.Customers, Url = request.GetUrlHelper() });

            // Assert
            Assert.Equal("http://localhost/Customers", result.AbsoluteUri);
        }

        private static HttpRequestMessage GetODataRequest(IEdmModel model)
        {
            HttpConfiguration configuration = new HttpConfiguration();
            string routeName = "Route";
            configuration.Routes.MapODataServiceRoute(routeName, null, model);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost");
            request.SetConfiguration(configuration);
            request.ODataProperties().RouteName = routeName;
            return request;
        }
    }
}
