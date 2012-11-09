// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http.Hosting;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Builder.TestModels;
using System.Web.Http.OData.Formatter.Deserialization;
using Microsoft.Data.Edm;
using Microsoft.TestCommon;

namespace System.Web.Http.OData
{
    public class DefaultODataActionResolverTest
    {
        private IEdmModel _model;

        [Theory]
        [InlineData("Drive", "http://server/Vehicles(6)/Drive")]
        [InlineData("Drive", "http://server/Vehicles(6)/Container.Drive")]
        [InlineData("Drive", "http://server/Vehicles(6)/org.odata.Container.Drive")]
        [InlineData("Drive", "http://server/service/Vehicles(6)/Drive")]
        [InlineData("Drive", "http://server/service/Vehicles(6)/Container.Drive")]
        [InlineData("Drive", "http://server/service/Vehicles(6)/org.odata.Container.Drive")]
        [InlineData("Drive", "http://server/Vehicles(6)/Container.Car/Drive")]
        [InlineData("Drive", "http://server/Vehicles(6)/Container.Car/Container.Drive")]
        [InlineData("Drive", "http://server/Vehicles(6)/Container.Car/org.odata.Container.Drive")]
        [InlineData("Drive", "http://server/service/Vehicles/Container.Car(6)/Drive")]
        [InlineData("Drive", "http://server/service/Vehicles/Container.Car(6)/Container.Drive")]
        [InlineData("Drive", "http://server/service/Vehicles/Container.Car(6)/org.odata.Container.Drive")]
        public void Can_find_action(string actionName, string url)
        {
            IODataActionResolver resolver = new DefaultODataActionResolver();
            ODataDeserializerContext context = new ODataDeserializerContext { Request = GetPostRequest(url), Model = GetModel() };
            IEdmFunctionImport action = resolver.Resolve(context);
            Assert.NotNull(action);
            Assert.Equal(actionName, action.Name);
        }

        [Fact]
        public void Can_find_action_overload_using_bindingparameter_type()
        {
            string url = "http://server/service/Vehicles(8)/Container.Car/Wash";
            IODataActionResolver resolver = new DefaultODataActionResolver();
            ODataDeserializerContext context = new ODataDeserializerContext { Request = GetPostRequest(url), Model = GetModel() };

            // TODO: Requires improvements in Uri Parser so it can establish type of path segment prior to ActionName
            // There's currently a bug here. For now, the test checks for the presence of the bug (as a reminder to fix
            // the test once the bug is fixed).
            // The following assert shows the behavior with the bug and should be removed once the bug is fixed.

            Assert.Throws<InvalidOperationException>(() => resolver.Resolve(context));

            // TODO: DateTimeOffsets are not handled well in the uri parser
            // The following calls show the behavior without the bug, and should be enabled once the bug is fixed.
            //IEdmFunctionImport action = resolver.Resolve(context);
            //Assert.NotNull(action);
            //Assert.Equal("Car", action.Parameters.First().Name);
        }

        [Fact]
        public void Throws_InvalidOperation_when_action_not_found()
        {
            IODataActionResolver resolver = new DefaultODataActionResolver();
            ODataDeserializerContext context = new ODataDeserializerContext { Request = GetPostRequest("http://server/service/MissingOperation"), Model = GetModel() };
            Assert.Throws<InvalidOperationException>(() =>
            {
                IEdmFunctionImport action = resolver.Resolve(context);
            },  "Action 'MissingOperation' was not found for RequestUri 'http://server/service/MissingOperation'.");
        }

        [Fact]
        public void Throws_InvalidOperation_when_multiple_overloads_found()
        {
            IODataActionResolver resolver = new DefaultODataActionResolver();
            ODataDeserializerContext context = new ODataDeserializerContext { Request = GetPostRequest("http://server/service/Vehicles/Container.Car(8)/Park"), Model = GetModel() };
            InvalidOperationException ioe = Assert.Throws<InvalidOperationException>(() =>
            {
                IEdmFunctionImport action = resolver.Resolve(context);
            }, "Action resolution failed. Multiple actions matching the action identifier 'Park' were found. The matching actions are: org.odata.Container.Park, org.odata.Container.Park.");
        }

        [Fact]
        public void Is_Auto_Registered()
        {
            HttpConfiguration configuration = new HttpConfiguration();
            DefaultODataActionResolver resolver = configuration.GetODataActionResolver() as DefaultODataActionResolver;
            Assert.NotNull(resolver);
        }

        private IEdmModel GetModel()
        {
            if (_model == null)
            {
                ODataModelBuilder builder = new ODataConventionModelBuilder();
                builder.ContainerName = "Container";
                builder.Namespace = "org.odata";
                // Action with no overloads
                builder.EntitySet<Vehicle>("Vehicles").EntityType.Action("Drive");
                // Valid overloads of "Wash" bound to different entities
                builder.Entity<Motorcycle>().Action("Wash");
                builder.Entity<Car>().Action("Wash");
                // Invalid overloads of "Park"
                builder.Entity<Car>().Action("Park");
                builder.Entity<Car>().Action("Park").Parameter<string>("mood");
                _model = builder.GetEdmModel();
            }
            return _model;
        }

        private static HttpRequestMessage GetPostRequest(string url)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = new HttpConfiguration();
            return request;
        }
    }
}
