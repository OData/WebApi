// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight.Metadata.Model;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight.Metadata
{
    public class FeedMetadataTests : WebHostTestBase<FeedMetadataTests>
    {
        public FeedMetadataTests(WebHostTestFixture<FeedMetadataTests> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration configuration)
        {
            configuration.EnableODataSupport(GetEdmModel(configuration));
            configuration.AddODataQueryFilter();
        }

        protected static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            ODataModelBuilder builder = configuration.CreateConventionModelBuilder();
            var entitySet = builder.EntitySet<StubEntity>("StubEntity");
            entitySet.EntityType.Collection.Action("Paged").ReturnsCollectionFromEntitySet<StubEntity>("StubEntity");
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
        public async Task ODataCountAndNextLinkAnnotationsAppearsOnAllMetadataLevelsWhenSpecified(string acceptHeader)
        {
            //Arrange
            StubEntity[] entities = MetadataTestHelpers.CreateInstances<StubEntity[]>();
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, BaseAddress.ToLowerInvariant() + "/StubEntity/Default.Paged");
            message.SetAcceptHeader(acceptHeader);
            string expectedNextLink = new Uri("http://differentServer:5000/StubEntity/Default.Paged?$skip=" + entities.Length).ToString();

            //Act
            HttpResponseMessage response = await Client.SendAsync(message);
            JObject result = await response.Content.ReadAsObject<JObject>();

            //Assert
            JsonAssert.PropertyEquals(entities.Length, "@odata.count", result);
            JsonAssert.PropertyEquals(expectedNextLink, "@odata.nextLink", result);
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
        public async Task MetadataAnnotationAppearsOnlyForFullAndMinimalMetadata(string acceptHeader)
        {
            //Arrange
            StubEntity[] entities = MetadataTestHelpers.CreateInstances<StubEntity[]>();
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, BaseAddress.ToLowerInvariant() + "/StubEntity/");
            message.SetAcceptHeader(acceptHeader);
            string expectedMetadataUrl = BaseAddress.ToLowerInvariant() + "/$metadata#StubEntity";

            //Act
            HttpResponseMessage response = await Client.SendAsync(message);
            JObject result = await response.Content.ReadAsObject<JObject>();

            //Assert
            if (acceptHeader.Contains("odata.metadata=none"))
            {
                JsonAssert.DoesNotContainProperty("@odata.context", result);
            }
            else
            {
                JsonAssert.PropertyEquals(expectedMetadataUrl, "@odata.context", result);
            }
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
        public async Task CanReturnTheWholeResultSetUsingNextLink(string acceptHeader)
        {
            //Arrange
            StubEntity[] entities = MetadataTestHelpers.CreateInstances<StubEntity[]>();
            string nextUrlToQuery = BaseAddress.ToLowerInvariant() + "/StubEntity/";
            JToken token = null;
            JArray returnedEntities = new JArray();
            JObject result = null;

            //Act
            do
            {
                HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, nextUrlToQuery);
                message.SetAcceptHeader(acceptHeader);
                HttpResponseMessage response = await Client.SendAsync(message);
                result = await response.Content.ReadAsObject<JObject>();
                JArray currentResults = (JArray)result["value"];
                for (int i = 0; i < currentResults.Count; i++)
                {
                    returnedEntities.Add(currentResults[i]);
                }
                if (result.TryGetValue("@odata.nextLink", out token))
                {
                    nextUrlToQuery = (string)token;
                }
                else
                {
                    nextUrlToQuery = null;
                }
            }
            while (nextUrlToQuery != null);

            //Assert
            Assert.Equal(entities.Length, returnedEntities.Count);
        }
    }
}
