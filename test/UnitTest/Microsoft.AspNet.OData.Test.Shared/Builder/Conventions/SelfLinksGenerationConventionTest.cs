//-----------------------------------------------------------------------------
// <copyright file="SelfLinksGenerationConventionTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Builder.Conventions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Builder.Conventions
{
    public class SelfLinksGenerationConventionTest
    {
        [Fact]
        public void Apply_AddsFeedSelfLink()
        {
            // Arrange
            var mockEntityType = new Mock<EntityTypeConfiguration>();
            var mockEntitySet = new Mock<EntitySetConfiguration>();
            mockEntitySet.Setup(entitySet => entitySet.GetFeedSelfLink()).Returns((Func<ResourceSetContext, Uri>)null).Verifiable();
            mockEntitySet.Setup(entitySet => entitySet.HasFeedSelfLink(It.IsAny<Func<ResourceSetContext, Uri>>()))
                .Returns(mockEntitySet.Object).Verifiable();
            mockEntitySet.Setup(entitySet => entitySet.EntityType).Returns(mockEntityType.Object);

            var mockModelBuilder = new Mock<ODataModelBuilder>();

            // Act
            new SelfLinksGenerationConvention().Apply(mockEntitySet.Object, mockModelBuilder.Object);

            // Assert
            mockEntitySet.Verify();
        }

        [Fact]
        public void Apply_DoesNotAddFeedSelfLink_IfOneIsPresent()
        {
            // Arrange
            var mockEntitySet = new Mock<EntitySetConfiguration>();
            var mockEntityType = new Mock<EntityTypeConfiguration>();
            mockEntitySet.Setup(entitySet => entitySet.GetFeedSelfLink()).Returns(feedContext => new Uri("http://www.cool.com")).Verifiable();
            mockEntitySet.Setup(entitySet => entitySet.EntityType).Returns(mockEntityType.Object);

            var mockModelBuilder = new Mock<ODataModelBuilder>();

            // Act
            new SelfLinksGenerationConvention().Apply(mockEntitySet.Object, mockModelBuilder.Object);

            // Assert
            mockEntitySet.Verify();
            mockEntitySet.Verify(entitySet => entitySet.HasFeedSelfLink(It.IsAny<Func<ResourceSetContext, Uri>>()), Times.Never());
        }

        [Fact]
        public void SelfLinksGenerationConvention_Uses_GetByIdWithCast_IfDerivedTypeHasNavigationProperty()
        {
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            var vehicles = builder.EntitySet<Vehicle>("vehicles");

            IEdmModel model = builder.GetEdmModel();
            IEdmEntitySet vehiclesEdmEntitySet = model.EntityContainer.EntitySets().Single();
            IEdmEntityType carType = model.AssertHasEntityType(typeof(Car));

            var request = RequestFactory.CreateFromModel(model);
            NavigationSourceLinkBuilderAnnotation linkBuilder = model.GetNavigationSourceLinkBuilder(vehiclesEdmEntitySet);
            var serializerContext = ODataSerializerContextFactory.Create(model, vehiclesEdmEntitySet, request);
            var entityContext = new ResourceContext(serializerContext, carType.AsReference(), new Car { Model = 2009, Name = "Contoso" });

            EntitySelfLinks selfLinks = linkBuilder.BuildEntitySelfLinks(entityContext, ODataMetadataLevel.FullMetadata);

            Assert.Equal("http://localhost/vehicles(Model=2009,Name='Contoso')", selfLinks.IdLink.ToString());
            Assert.Equal("http://localhost/vehicles(Model=2009,Name='Contoso')/Microsoft.AspNet.OData.Test.Builder.TestModels.Car", selfLinks.EditLink.ToString());
        }

        [Fact]
        public void SelfLinksGenerationConvention_Uses_WithCast_IfDerivedTypeHasNavigationProperty_ForSingleton()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            var myVehicle = builder.Singleton<Vehicle>("MyVehicle");

            IEdmModel model = builder.GetEdmModel();
            IEdmSingleton vehicleEdmSingleton = model.EntityContainer.FindSingleton("MyVehicle");
            IEdmEntityType carType = model.AssertHasEntityType(typeof(Car));

            var request = RequestFactory.CreateFromModel(model);
            NavigationSourceLinkBuilderAnnotation linkBuilder = model.GetNavigationSourceLinkBuilder(vehicleEdmSingleton);
            var serializerContext = ODataSerializerContextFactory.Create(model, vehicleEdmSingleton, request);
            var entityContext = new ResourceContext(serializerContext, carType.AsReference(), new Car { Model = 2014, Name = "Contoso" });

            // Act
            EntitySelfLinks selfLinks = linkBuilder.BuildEntitySelfLinks(entityContext, ODataMetadataLevel.FullMetadata);

            // Assert
            Assert.Equal("http://localhost/MyVehicle", selfLinks.IdLink.ToString());
            Assert.Equal("http://localhost/MyVehicle/Microsoft.AspNet.OData.Test.Builder.TestModels.Car", selfLinks.EditLink.ToString());
        }

        [Fact]
        public void SelfLinksGenerationConvention_Uses_GetByIdWithoutCast_IfDerivedTypeDoesnotHaveNavigationProperty()
        {
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            var motorcycles = builder.EntitySet<Motorcycle>("motorcycles");

            IEdmModel model = builder.GetEdmModel();
            IEdmEntitySet vehiclesEdmEntitySet = model.EntityContainer.EntitySets().Single();
            IEdmEntityType sportbikeType = model.AssertHasEntityType(typeof(SportBike));

            var request = RequestFactory.CreateFromModel(model);
            NavigationSourceLinkBuilderAnnotation linkBuilder = model.GetNavigationSourceLinkBuilder(vehiclesEdmEntitySet);
            var serializerContext = ODataSerializerContextFactory.Create(model, vehiclesEdmEntitySet, request);
            var entityContext = new ResourceContext(serializerContext, sportbikeType.AsReference(), new SportBike { Model = 2009, Name = "Ninja" });

            EntitySelfLinks selfLinks = linkBuilder.BuildEntitySelfLinks(entityContext, ODataMetadataLevel.FullMetadata);

            // This test sometimes writes one of these two:
            //Assert.Equal("http://localhost/motorcycles(Model=2009,Name='Ninja')", );
            //Assert.Equal("http://localhost/motorcycles(Name='Ninja',Model=2009)", );
            var link = selfLinks.IdLink.ToString();
            Assert.Contains("http://localhost/motorcycles", link);
            Assert.Contains("Model=2009", link);
            Assert.Contains("Name='Ninja'", link);
        }

        [Fact]
        public void SelfLinksGenerationConvention_Uses_WithoutCast_IfDerivedTypeDoesnotHaveNavigationProperty_ForSingleton()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            var myMotorcycle = builder.Singleton<Motorcycle>("MyMotor");

            IEdmModel model = builder.GetEdmModel();
            IEdmSingleton vehicleEdmSingleton = model.EntityContainer.FindSingleton("MyMotor");
            IEdmEntityType sportbikeType = model.AssertHasEntityType(typeof(SportBike));

            var request = RequestFactory.CreateFromModel(model);
            NavigationSourceLinkBuilderAnnotation linkBuilder = model.GetNavigationSourceLinkBuilder(vehicleEdmSingleton);
            var serializerContext = ODataSerializerContextFactory.Create(model, vehicleEdmSingleton, request);
            var entityContext = new ResourceContext(serializerContext, sportbikeType.AsReference(), new SportBike { Model = 2014, Name = "Ninja" });

            // Act
            EntitySelfLinks selfLinks = linkBuilder.BuildEntitySelfLinks(entityContext, ODataMetadataLevel.FullMetadata);

            // Assert
            Assert.Equal("http://localhost/MyMotor", selfLinks.IdLink.ToString());
        }

        class SelfLinkConventionTests_EntityType
        {
            public string ID { get; set; }
        }
    }
}
