//-----------------------------------------------------------------------------
// <copyright file="ComplexTypeTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Newtonsoft.Json.Linq;
using Xunit;
using JsonLightModel = Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight.Metadata.Model;

namespace Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight.Metadata
{
    public class ComplexTypeTests : WebHostTestBase
    {
        public ComplexTypeTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.MapODataServiceRoute("Complex", "Complex", GetEdmModel(configuration), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
            configuration.AddODataQueryFilter();
        }

        protected static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            ODataModelBuilder builder = configuration.CreateConventionModelBuilder();
            var entitySet = builder.EntitySet<JsonLightModel.EntityWithComplexProperties>("EntityWithComplexProperties");
            return builder.GetEdmModel();
        }

        [Theory]
        [InlineData("application/json;odata.metadata=full")]
        [InlineData("application/json;odata.metadata=full;odata.streaming=true")]
        [InlineData("application/json;odata.metadata=full;odata.streaming=false")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=minimal;odata.streaming=true")]
        [InlineData("application/json;odata.metadata=minimal;odata.streaming=false")]
        [InlineData("application/json;odata.metadata=none")]
        [InlineData("application/json;odata.metadata=none;odata.streaming=true")]
        [InlineData("application/json;odata.metadata=none;odata.streaming=false")]
        [InlineData("application/json")]
        [InlineData("application/json;odata.streaming=true")]
        [InlineData("application/json;odata.streaming=false")]
        public async Task ODataTypeAnnotationShouldAppearForComplexTypesLikeCollectionAndUserDefinedTypes(string acceptHeader)
        {
            //Arrange
            string requestUrl = BaseAddress.ToLowerInvariant() + "/Complex/EntityWithComplexProperties/";
            JObject complexProperty;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.SetAcceptHeader(acceptHeader);

            //Act
            HttpResponseMessage response = await Client.SendAsync(request);
            JObject result = await response.Content.ReadAsObject<JObject>();

            //Assert
            JsonAssert.ContainsProperty("value", result);
            JArray returnedEntities = (JArray)result["value"];
            for (int i = 0; i < returnedEntities.Count; i++)
            {
                JObject returnedEntity = (JObject)returnedEntities[i];
                if (!acceptHeader.Contains("odata.metadata=full"))
                {
                    JsonAssert.DoesNotContainProperty("@odata.*", returnedEntity);
                }
                else
                {
                    JsonAssert.PropertyEquals("#Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight.Metadata.Model.EntityWithComplexProperties",
                        "@odata.type", returnedEntity);
                    JsonAssert.PropertyEquals("#Collection(String)", "StringListProperty@odata.type", returnedEntity);
                    JsonAssert.ContainsProperty("ComplexProperty", returnedEntity);
                    complexProperty = (JObject)returnedEntity["ComplexProperty"];
                    JsonAssert.PropertyEquals("#Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight.Metadata.Model.ComplexType",
                        "@odata.type", complexProperty);
                    JsonAssert.PropertyEquals("#Collection(String)", "StringListProperty@odata.type", complexProperty);
                }
            }
        }
    }
}
