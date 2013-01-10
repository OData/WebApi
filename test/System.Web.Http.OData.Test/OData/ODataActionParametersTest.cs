// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Runtime.Serialization;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Builder.TestModels;
using System.Web.Http.OData.Formatter.Deserialization;
using System.Web.Http.OData.Routing;
using Microsoft.Data.Edm;
using Microsoft.TestCommon;

namespace System.Web.Http.OData
{
    public class ODataActionParametersTest
    {
        [Theory]
        [InlineData("Drive", "Vehicles(6)/Drive")]
        [InlineData("Drive", "Vehicles(6)/Container.Drive")]
        [InlineData("Drive", "Vehicles(6)/org.odata.Container.Drive")]
        [InlineData("Drive", "Vehicles(6)/System.Web.Http.OData.Builder.TestModels.Car/Drive")]
        [InlineData("Drive", "Vehicles(6)/System.Web.Http.OData.Builder.TestModels.Car/Container.Drive")]
        [InlineData("Drive", "Vehicles(6)/System.Web.Http.OData.Builder.TestModels.Car/org.odata.Container.Drive")]
        public void Can_find_action(string actionName, string url)
        {
            IEdmModel model = GetModel();
            ODataPath path = new DefaultODataPathHandler().Parse(model, url);
            Assert.NotNull(path); // Guard
            ODataDeserializerContext context = new ODataDeserializerContext { Path = path, Model = model };
            IEdmFunctionImport action = ODataActionPayloadDeserializer.GetFunctionImport(context);
            Assert.NotNull(action);
            Assert.Equal(actionName, action.Name);
        }

        [Fact]
        public void Can_find_action_overload_using_bindingparameter_type()
        {
            IEdmModel model = GetModel();
            string url = "Vehicles(8)/System.Web.Http.OData.Builder.TestModels.Car/Wash";
            ODataPath path = new DefaultODataPathHandler().Parse(model, url);
            Assert.NotNull(path); // Guard
            ODataDeserializerContext context = new ODataDeserializerContext { Path = path, Model = model };

            IEdmFunctionImport action = ODataActionPayloadDeserializer.GetFunctionImport(context);

            Assert.NotNull(action);
            Assert.Equal("Wash", action.Name);

        }

        [Fact]
        public void Throws_Serialization_WhenPathNotFound()
        {
            ODataDeserializerContext context = new ODataDeserializerContext { Path = null };
            Assert.Throws<SerializationException>(() =>
            {
                IEdmFunctionImport action = ODataActionPayloadDeserializer.GetFunctionImport(context);
            }, "The operation cannot be completed because no ODataPath is available for the request.");
        }

        [Fact]
        public void ParserThrows_InvalidArgument_when_multiple_overloads_found()
        {
            IEdmModel model = GetModel();

            Assert.ThrowsArgument(() =>
            {
                new DefaultODataPathHandler().Parse(model, "Vehicles/System.Web.Http.OData.Builder.TestModels.Car(8)/Park");
            }, "actionIdentifier", "Action resolution failed. Multiple actions matching the action identifier 'Park' were found. The matching actions are: org.odata.Container.Park, org.odata.Container.Park.");
        }

        private static IEdmModel GetModel()
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
            return builder.GetEdmModel();
        }
    }
}
