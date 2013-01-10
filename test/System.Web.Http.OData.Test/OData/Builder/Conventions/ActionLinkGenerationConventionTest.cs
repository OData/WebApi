// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http.Hosting;
using System.Web.Http.OData.Builder.TestModels;
using System.Web.Http.OData.Routing;
using System.Web.Http.Routing;
using Microsoft.Data.Edm;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Builder.Conventions
{
    public class ActionLinkGenerationConventionTest
    {
        ActionLinkGenerationConvention _convention = new ActionLinkGenerationConvention();

        [Fact]
        public void GenerateActionLink_GeneratesLinkWithoutCast_IfEntitySetTypeDerivesFromActionEntityType()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            var cars = builder.EntitySet<Car>("cars");
            var paintAction = builder.Entity<Vehicle>().Action("Paint");

            IEdmModel model = builder.GetEdmModel();
            var carsEdmSet = model.EntityContainers().Single().FindEntitySet("cars");

            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Routes.MapODataRoute(model);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost");
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;
            request.Properties[HttpPropertyKeys.HttpRouteDataKey] = new HttpRouteData(new HttpRoute());

            Uri link = ActionLinkGenerationConvention.GenerateActionLink(
                new EntityInstanceContext()
                {
                    EdmModel = model,
                    EntitySet = carsEdmSet,
                    EntityType = carsEdmSet.ElementType,
                    Url = request.GetUrlHelper(),
                    EntityInstance = new Car { Model = 2009, Name = "Accord" }
                },
                paintAction);

            Assert.Equal("http://localhost/cars(Model=2009,Name='Accord')/Paint", link.AbsoluteUri);
        }

        [Fact]
        public void GenerateActionLink_GeneratesLinkWithoutCast_IfEntitySetTypeMatchesActionEntityType()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            var cars = builder.EntitySet<Car>("cars");
            var paintAction = cars.EntityType.Action("Paint");

            IEdmModel model = builder.GetEdmModel();
            var carsEdmSet = model.EntityContainers().Single().FindEntitySet("cars");

            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Routes.MapODataRoute(model);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost");
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;
            request.Properties[HttpPropertyKeys.HttpRouteDataKey] = new HttpRouteData(new HttpRoute());

            Uri link = ActionLinkGenerationConvention.GenerateActionLink(
                new EntityInstanceContext()
                {
                    EdmModel = model,
                    EntitySet = carsEdmSet,
                    EntityType = carsEdmSet.ElementType,
                    Url = request.GetUrlHelper(),
                    EntityInstance = new Car { Model = 2009, Name = "Accord" }
                },
                paintAction);

            Assert.Equal("http://localhost/cars(Model=2009,Name='Accord')/Paint", link.AbsoluteUri);
        }

        [Fact]
        public void GenerateActionLink_GeneratesLinkWithCast_IfEntitySetTypeDoesnotMatchActionEntityType()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            var vehicles = builder.EntitySet<Vehicle>("vehicles");
            var car = builder.Entity<Car>();
            var paintAction = car.Action("Paint");

            IEdmModel model = builder.GetEdmModel();
            var vehiclesEdmSet = model.EntityContainers().Single().FindEntitySet("vehicles");
            var carEdmType = model.FindDeclaredType("System.Web.Http.OData.Builder.TestModels.Car") as IEdmEntityType;

            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Routes.MapODataRoute(model);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost");
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;
            request.Properties[HttpPropertyKeys.HttpRouteDataKey] = new HttpRouteData(new HttpRoute());

            Uri link = ActionLinkGenerationConvention.GenerateActionLink(
                new EntityInstanceContext()
                {
                    EdmModel = model,
                    EntitySet = vehiclesEdmSet,
                    EntityType = carEdmType,
                    Url = request.GetUrlHelper(),
                    EntityInstance = new Car { Model = 2009, Name = "Accord" }
                },
                paintAction);

            Assert.Equal("http://localhost/vehicles(Model=2009,Name='Accord')/System.Web.Http.OData.Builder.TestModels.Car/Paint", link.AbsoluteUri);
        }

        [Fact]
        public void Apply_Doesnot_Override_UserConfiguration()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            var vehicles = builder.EntitySet<Vehicle>("vehicles");
            var car = builder.AddEntity(typeof(Car));
            var paintAction = vehicles.EntityType.Action("Paint");
            paintAction.HasActionLink(ctxt => new Uri("http://localhost/ActionTestWorks"));

            _convention.Apply(paintAction, builder);

            IEdmModel model = builder.GetEdmModel();
            var vehiclesEdmSet = model.EntityContainers().Single().FindEntitySet("vehicles");
            var carEdmType = model.FindDeclaredType("System.Web.Http.OData.Builder.TestModels.Car") as IEdmEntityType;
            var paintEdmAction = model.GetAvailableProcedures(model.FindDeclaredType("System.Web.Http.OData.Builder.TestModels.Car") as IEdmEntityType).Single();

            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Routes.MapODataRoute(model);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost");
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;
            request.Properties[HttpPropertyKeys.HttpRouteDataKey] = new HttpRouteData(new HttpRoute());

            ActionLinkBuilder actionLinkBuilder = model.GetActionLinkBuilder(paintEdmAction);

            Uri link = actionLinkBuilder.BuildActionLink(new EntityInstanceContext()
            {
                EdmModel = model,
                EntitySet = vehiclesEdmSet,
                EntityType = carEdmType,
                Url = request.GetUrlHelper(),
                EntityInstance = new Car { Model = 2009, Name = "Accord" }
            });
            Assert.Equal(
                "http://localhost/ActionTestWorks",
                link.AbsoluteUri);
        }

        [Fact]
        public void Apply_SetsActionLinkBuilder_OnlyIfActionIsBindable()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            var vehicles = builder.EntitySet<Vehicle>("vehicles");
            var paintAction = builder.Action("Paint");

            _convention.Apply(paintAction, builder);

            IEdmModel model = builder.GetEdmModel();
            var paintEdmAction = model.EntityContainers().Single().Elements.OfType<IEdmFunctionImport>().Single();

            ActionLinkBuilder actionLinkBuilder = model.GetActionLinkBuilder(paintEdmAction);

            Assert.Null(actionLinkBuilder);
        }
    }
}
