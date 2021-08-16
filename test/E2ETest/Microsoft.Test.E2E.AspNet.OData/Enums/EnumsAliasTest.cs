//-----------------------------------------------------------------------------
// <copyright file="EnumsAliasTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Microsoft.Test.E2E.AspNet.OData.ModelBuilder;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Enums
{
    public class EnumsAliasTest : WebHostTestBase
    {
        public EnumsAliasTest(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            var controllers = new[] { typeof(WeatherForecastController), typeof(MetadataController) };
            configuration.AddControllers(controllers);

            configuration.Routes.Clear();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null).Select();
            configuration.MapODataServiceRoute("odata", "odata", EnumsEdmModel.GetEnumAliasModel(configuration));
            configuration.EnsureInitialized();
        }

        [Fact]
        public async Task ModelBuilderTest()
        {
            // Arrange
            string requestUri = string.Format("{0}/odata/$metadata", this.BaseAddress);

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            var stream = await response.Content.ReadAsStreamAsync();

            // Act
            IODataResponseMessage message = new ODataMessageWrapper(stream, response.Content.Headers);
            var reader = new ODataMessageReader(message);
            IEdmModel edmModel = reader.ReadMetadataDocument();

            // Assert
            IEdmEntityType weatherForecast = edmModel.SchemaElements.OfType<IEdmEntityType>().First();
            Assert.Equal("WeatherForecast", weatherForecast.Name);
            Assert.Equal(3, weatherForecast.Properties().Count());

            // Enum Property 1
            var status = weatherForecast.Properties().SingleOrDefault(p => p.Name == "Status");
            Assert.True(status.Type.IsEnum());

            // Enum Property 2
            var skill = weatherForecast.Properties().SingleOrDefault(p => p.Name == "Skill");
            Assert.True(skill.Type.IsEnum());
        }

        [Fact]
        public async Task QueryEntitiesFilterByEnumUsingEnumAlias()
        {
            string uri = this.BaseAddress + "/odata/WeatherForecast?$filter=Status eq 'Sold out'";

            using (var response = await this.Client.GetAsync(uri))
            {
                Assert.True(response.IsSuccessStatusCode);

                var result = await response.Content.ReadAsObject<JObject>();
                var value = result.GetValue("value") as JArray;
                Assert.NotNull(value);
                Assert.Equal(2, value.Count);
                JObject item0 = value.ElementAt(0) as JObject;
                Assert.Equal(2, item0["Id"]);

                JObject item1 = value.ElementAt(1) as JObject;
                Assert.Equal(4, item1["Id"]);
            }
        }

        [Fact]
        public async Task QueryEntitiesFilterByEnumWithoutEnumAlias()
        {
            string uri = this.BaseAddress + "/odata/WeatherForecast?$filter=Skill eq 'Sql'";

            using (var response = await this.Client.GetAsync(uri))
            {
                Assert.True(response.IsSuccessStatusCode);

                var result = await response.Content.ReadAsObject<JObject>();
                var value = result.GetValue("value") as JArray;
                Assert.NotNull(value);
                Assert.Equal(3, value.Count);
                JObject item0 = value.ElementAt(0) as JObject;
                Assert.Equal(1, item0["Id"]);

                JObject item1 = value.ElementAt(1) as JObject;
                Assert.Equal(3, item1["Id"]);

                JObject item2 = value.ElementAt(2) as JObject;
                Assert.Equal(5, item2["Id"]);
            }
        }
    }
}
