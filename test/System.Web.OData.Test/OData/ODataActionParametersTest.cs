// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Runtime.Serialization;
using System.Web.OData.Builder;
using System.Web.OData.Builder.TestModels;
using System.Web.OData.Formatter.Deserialization;
using System.Web.OData.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;

namespace System.Web.OData
{
    public class ODataActionParametersTest
    {
        [Theory]
        [InlineData("Drive", "Vehicles(6)/org.odata.Drive")]
        [InlineData("Drive", "Vehicles(6)/System.Web.OData.Builder.TestModels.Car/org.odata.Drive")]
        public void Can_find_action(string actionName, string url)
        {
            // Arrange
            IEdmModel model = GetModel();

            // Act
            ODataPath path = new DefaultODataPathHandler().Parse(model, url);
            Assert.NotNull(path); // Guard
            ODataDeserializerContext context = new ODataDeserializerContext { Path = path, Model = model };
            IEdmAction action = ODataActionPayloadDeserializer.GetAction(context);

            // Assert
            Assert.NotNull(action);
            Assert.Equal(actionName, action.Name);
        }

        [Theory]
        [InlineData("Vehicles(6)/Drive")]
        [InlineData("Vehicles(6)/System.Web.OData.Builder.TestModels.Car/Drive")]
        public void ParserThrows_SerializationException_UnqualifiedBoundAction(string url)
        {
            // Arrange
            IEdmModel model = GetModel();

            // Act
            ODataPath path = new DefaultODataPathHandler().Parse(model, url);
            Assert.NotNull(path); // Guard
            ODataDeserializerContext context = new ODataDeserializerContext { Path = path, Model = model };

            // Assert
            Assert.Throws<SerializationException>(() => ODataActionPayloadDeserializer.GetAction(context),
            "The last segment of the request URI '" + url + "' was not recognized as an OData action.");
        }

        [Fact]
        public void Can_find_action_overload_using_bindingparameter_type()
        {
            // Arrange
            IEdmModel model = GetModel();
            string url = "Vehicles(8)/System.Web.OData.Builder.TestModels.Car/org.odata.Wash";
            ODataPath path = new DefaultODataPathHandler().Parse(model, url);
            Assert.NotNull(path); // Guard
            ODataDeserializerContext context = new ODataDeserializerContext { Path = path, Model = model };

            // Act
            IEdmAction action = ODataActionPayloadDeserializer.GetAction(context);

            // Assert
            Assert.NotNull(action);
            Assert.Equal("Wash", action.Name);
        }

        [Fact]
        public void Throws_Serialization_WhenPathNotFound()
        {
            // Arrange
            ODataDeserializerContext context = new ODataDeserializerContext { Path = null };

            // Act & Assert
            Assert.Throws<SerializationException>(() =>
            {
                IEdmAction action = ODataActionPayloadDeserializer.GetAction(context);
            }, "The operation cannot be completed because no ODataPath is available for the request.");
        }

        [Fact]
        public void ParserThrows_InvalidArgument_when_multiple_overloads_found()
        {
            // Arrange
            IEdmModel model = GetModel();

            // Act & Assert
            Assert.ThrowsArgument(() =>
            {
                new DefaultODataPathHandler().Parse(model, "Vehicles/System.Web.OData.Builder.TestModels.Car(8)/org.odata.Park");
            }, "actionIdentifier", "Action resolution failed. Multiple actions matching the action identifier 'org.odata.Park' were found. The matching actions are: org.odata.Park, org.odata.Park.");
        }

        private static IEdmModel GetModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            builder.ContainerName = "Container";
            builder.Namespace = "org.odata";
            // Action with no overloads
            builder.EntitySet<Vehicle>("Vehicles").EntityType.Action("Drive");
            // Valid overloads of "Wash" bound to different entities
            builder.EntityType<Motorcycle>().Action("Wash");
            builder.EntityType<Car>().Action("Wash");

            EdmModel model = (EdmModel)builder.GetEdmModel();

            // Invalid overloads of action "Park". These two actions must have different names or binding types
            // but differ only in that the second has a 'mood' parameter.
            IEdmEntityType entityType = model.SchemaElements.OfType<IEdmEntityType>().Single(e => e.Name == "Car");
            var park = new EdmAction(
                "org.odata",
                "Park",
                returnType: null,
                isBound: true,
                entitySetPathExpression: null);
            park.AddParameter("bindingParameter", new EdmEntityTypeReference(entityType, isNullable: false));
            model.AddElement(park);

            IEdmTypeReference stringType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.String, isNullable: true);
            park = new EdmAction(
                "org.odata",
                "Park",
                returnType: null,
                isBound: true,
                entitySetPathExpression: null);
            park.AddParameter("bindingParameter", new EdmEntityTypeReference(entityType, isNullable: false));
            park.AddParameter("mood", stringType);
            model.AddElement(park);

            return model;
        }
    }
}
