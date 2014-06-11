// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.OData.Builder.TestModels;
using System.Web.OData.Extensions;
using System.Web.OData.Formatter.Serialization;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Builder.Conventions
{
    public class ActionLinkGenerationConventionTest
    {
        [Fact]
        public void Apply_Doesnot_Override_UserConfiguration()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            var vehicles = builder.EntitySet<Vehicle>("vehicles");
            var car = builder.AddEntityType(typeof(Car));
            var paintAction = vehicles.EntityType.Action("Paint");
            paintAction.HasActionLink(ctxt => new Uri("http://localhost/ActionTestWorks"), followsConventions: false);
            ActionLinkGenerationConvention convention = new ActionLinkGenerationConvention();

            convention.Apply(paintAction, builder);

            IEdmModel model = builder.GetEdmModel();
            var vehiclesEdmSet = model.EntityContainer.FindEntitySet("vehicles");
            var carEdmType = model.FindDeclaredType("System.Web.OData.Builder.TestModels.Car") as IEdmEntityType;
            var paintEdmAction =
                model.GetAvailableProcedures(
                    model.FindDeclaredType("System.Web.OData.Builder.TestModels.Car") as IEdmEntityType).Single()
                as IEdmAction;
            Assert.NotNull(paintEdmAction);

            HttpConfiguration configuration = new HttpConfiguration();
            configuration.MapODataServiceRoute(model);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost");
            request.SetConfiguration(configuration);

            ActionLinkBuilder actionLinkBuilder = model.GetActionLinkBuilder(paintEdmAction);

            var serializerContext = new ODataSerializerContext { Model = model, NavigationSource = vehiclesEdmSet, Url = request.GetUrlHelper() };
            var entityContext = new EntityInstanceContext(serializerContext, carEdmType.AsReference(), new Car { Model = 2009, Name = "Contoso" });

            Uri link = actionLinkBuilder.BuildActionLink(entityContext);
            Assert.Equal("http://localhost/ActionTestWorks", link.AbsoluteUri);
        }

        [Fact]
        public void Apply_SetsActionLinkBuilder_OnlyIfActionIsBindable()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            var vehicles = builder.EntitySet<Vehicle>("vehicles");
            var paintAction = builder.Action("Paint");
            ActionLinkGenerationConvention convention = new ActionLinkGenerationConvention();

            // Act
            convention.Apply(paintAction, builder);

            // Assert
            IEdmModel model = builder.GetEdmModel();
            var paintEdmAction = model.EntityContainer.Elements.OfType<IEdmActionImport>().Single();

            ActionLinkBuilder actionLinkBuilder = model.GetAnnotationValue<ActionLinkBuilder>(paintEdmAction);

            Assert.Null(actionLinkBuilder);
        }

        [Fact]
        public void Apply_FollowsConventions()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            ActionConfiguration action = new ActionConfiguration(builder, "IgnoreAction");
            Mock<IEdmTypeConfiguration> mockBindingParameterType = new Mock<IEdmTypeConfiguration>();
            mockBindingParameterType.Setup(o => o.Kind).Returns(EdmTypeKind.Entity);
            action.SetBindingParameter("IgnoreParameter", mockBindingParameterType.Object);
            ActionLinkGenerationConvention convention = new ActionLinkGenerationConvention();

            // Act
            convention.Apply(action, builder);

            // Assert
            Assert.True(action.FollowsConventions);
        }
    }
}
