//-----------------------------------------------------------------------------
// <copyright file="ODataActionParametersTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using System.Runtime.Serialization;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;
using Xunit;

namespace Microsoft.AspNet.OData.Test
{
    public class ODataActionParametersTest
    {
        private const string _serviceRoot = "http://any/";

        [Theory]
        [InlineData("Drive", "Vehicles(Model=6,Name='6')/org.odata.Drive")]
        [InlineData("Drive", "Vehicles(Model=6,Name='6')/org.odata.Car/org.odata.Drive")]
        [InlineData("Drive", "MyVehicle/org.odata.Drive")]
        [InlineData("Drive", "MyVehicle/org.odata.Car/org.odata.Drive")]
        public void Can_Find_Action_QualifiedActionName(string actionName, string url)
        {
            // Arrange
            IEdmModel model = GetModel();

            // Act
            ODataPath path = new DefaultODataPathHandler().Parse(model, _serviceRoot, url);
            Assert.NotNull(path); // Guard
            ODataDeserializerContext context = new ODataDeserializerContext { Path = path, Model = model };
            IEdmAction action = ODataActionPayloadDeserializer.GetAction(context);

            // Assert
            Assert.NotNull(action);
            Assert.Equal(actionName, action.Name);
        }

        [Theory]
        [InlineData("Vehicles(Model=6,Name='6')/Drive")]
        [InlineData("Vehicles(Model=6,Name='6')/org.odata.Car/Drive")]
        [InlineData("MyVehicle/Drive")]
        [InlineData("MyVehicle/org.odata.Car/Drive")]
        public void CanParse_UnqualifiedBoundAction(string url)
        {
            // Arrange
            IEdmModel model = GetModel();

            // Act & Assert
            OperationSegment operationSegment = Assert.IsType<OperationSegment>(
                new DefaultODataPathHandler().Parse(model, _serviceRoot, url).Segments.Last());
            Assert.Single(operationSegment.Operations);
            Assert.Equal("Drive", operationSegment.Operations.First().Name);
        }


        [Theory]
        [InlineData("Vehicles(Model=8,Name='8')/org.odata.Car/org.odata.Wash")]
        [InlineData("MyVehicle/org.odata.Car/org.odata.Wash")]
        public void Can_find_action_overload_using_bindingparameter_type(string url)
        {
            // Arrange
            IEdmModel model = GetModel();
            ODataPath path = new DefaultODataPathHandler().Parse(model, _serviceRoot, url);
            Assert.NotNull(path); // Guard
            ODataDeserializerContext context = new ODataDeserializerContext { Path = path, Model = model };

            // Act
            IEdmAction action = ODataActionPayloadDeserializer.GetAction(context);

            // Assert
            Assert.NotNull(action);
            Assert.Equal("Wash", action.Name);
        }

        [Theory]
        [InlineData("Vehicles(Model=8,Name='8')/org.odata.Car/customize.NSAction")]
        [InlineData("MyVehicle/org.odata.Car/customize.NSAction")]
        public void Can_Find_Customized_Namespace_Action(string url)
        {
            // Arrange
            IEdmModel model = GetModel();
            ODataPath path = new DefaultODataPathHandler().Parse(model, _serviceRoot, url);
            Assert.NotNull(path); // Guard
            ODataDeserializerContext context = new ODataDeserializerContext { Path = path, Model = model };

            // Act
            IEdmAction action = ODataActionPayloadDeserializer.GetAction(context);

            // Assert
            Assert.NotNull(action);
            Assert.Equal("NSAction", action.Name);
        }

        [Fact]
        public void Throws_Serialization_WhenPathNotFound()
        {
            // Arrange
            ODataDeserializerContext context = new ODataDeserializerContext { Path = null };

            // Act & Assert
            ExceptionAssert.Throws<SerializationException>(() =>
            {
                IEdmAction action = ODataActionPayloadDeserializer.GetAction(context);
            }, "The operation cannot be completed because no ODataPath is available for the request.");
        }

        [Fact]
        public void ParserThrows_ODataException_when_multiple_overloads_found()
        {
            // Arrange
            IEdmModel model = GetModel();

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(() =>
                new DefaultODataPathHandler().Parse(model, _serviceRoot, "Vehicles/org.odata.Car(Model=8,Name='8')/org.odata.Park"),
                "Multiple action overloads were found with the same binding parameter for 'org.odata.Park'.");
        }

        private static IEdmModel GetModel()
        {
            ODataModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.ContainerName = "Container";
            builder.Namespace = "org.odata";
            // Action with no overloads
            builder.EntitySet<Vehicle>("Vehicles").EntityType.Action("Drive");
            builder.Singleton<Vehicle>("MyVehicle");
            // Valid overloads of "Wash" bound to different entities
            builder.EntityType<Motorcycle>().Action("Wash");
            builder.EntityType<Car>().Action("Wash");
            builder.EntityType<Car>().Action("NSAction").Namespace = "customize";

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
