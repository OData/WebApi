//-----------------------------------------------------------------------------
// <copyright file="ParameterAliasTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ParameterAlias
{
    public class ParameterAliasTest : WebHostTestBase
    {
        public ParameterAliasTest(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            var controllers = new[] { typeof(TradesController) };
            configuration.AddControllers(controllers);

            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.MapODataServiceRoute("OData", "", GetModel(configuration));
            configuration.EnsureInitialized();
        }

        private static IEdmModel GetModel(WebRouteConfiguration configuration)
        {
            ODataConventionModelBuilder builder = configuration.CreateConventionModelBuilder();
            EntitySetConfiguration<Trade> tradesConfiguration = builder.EntitySet<Trade>("Trades");

            //Add bound function
            var boundFunction = tradesConfiguration.EntityType.Collection.Function("GetTradingVolume");
            boundFunction.Parameter<string>("productName");
            boundFunction.Parameter<CountryOrRegion>("PortingCountryOrRegion");
            boundFunction.Returns<long?>();

            //Add bound function
            boundFunction = tradesConfiguration.EntityType.Collection.Function("GetTopTrading");
            boundFunction.Parameter<string>("productName");
            boundFunction.ReturnsFromEntitySet<Trade>("Trades");
            boundFunction.IsComposable = true;

            //Add unbound function
            var unboundFunction = builder.Function("GetTradeByCountry");
            unboundFunction.Parameter<CountryOrRegion>("PortingCountryOrRegion");
            unboundFunction.ReturnsCollectionFromEntitySet<Trade>("Trades");

            builder.Namespace = typeof(CountryOrRegion).Namespace;

           return builder.GetEdmModel();
        }

        #region Test
        [Fact]
        public async Task ParameterAliasInFunctionCall()
        {
            //Unbound function
            string query = "/GetTradeByCountry(PortingCountryOrRegion=@p1)?@p1=Microsoft.Test.E2E.AspNet.OData.ParameterAlias.CountryOrRegion'USA'";

            HttpResponseMessage response = await this.Client.GetAsync(this.BaseAddress + query);
            var json = await response.Content.ReadAsObject<JObject>();
            var result = json["value"] as JArray;
            Assert.Equal(3, result.Count);

            //Bound function
            string requestUri = this.BaseAddress + "/Trades/Microsoft.Test.E2E.AspNet.OData.ParameterAlias.GetTradingVolume(productName=@p1, PortingCountryOrRegion=@p2)?@p1='Rice'&@p2=Microsoft.Test.E2E.AspNet.OData.ParameterAlias.CountryOrRegion'USA'";
            response = await this.Client.GetAsync(requestUri);
            json = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(1000, (long)json["value"]);
        }

        [Theory]
        [InlineData("?$filter=contains(@p1, @p2)&@p1=Description&@p2='Export'", 3)]   //Reference property and primitive type
        [InlineData("?@p1=startswith(Description,'Import')&$filter=@p1", 3)]  //Reference expression
        [InlineData("?$filter=TradingVolume eq @p1", 1)]  //Reference nullable value
        public async Task ParameterAliasInFilter(string queryOption, int expectedResult)
        {
            string requestBaseUri = this.BaseAddress + "/Trades";
            
            HttpResponseMessage response = await this.Client.GetAsync(requestBaseUri + queryOption);

            var json = await response.Content.ReadAsObject<JObject>();
            var result = json["value"] as JArray;
            Assert.Equal(expectedResult, result.Count);
        }

        [Theory]
        [InlineData("?$orderby=@p1&@p1=PortingCountryOrRegion", "Australia")]
        [InlineData("?$orderby=ProductName,@p2 desc,PortingCountryOrRegion desc&@p2=TradingVolume", "USA")]
        public async Task ParameterAliasInOrderby(string queryOption, string expectedPortingCountry)
        {
            string requestBaseUri = this.BaseAddress + "/Trades";

            HttpResponseMessage response = await this.Client.GetAsync(requestBaseUri + queryOption);

            var json = await response.Content.ReadAsObject<JObject>();
            var result = json["value"] as JArray;
            Assert.Equal(expectedPortingCountry, result.First["PortingCountryOrRegion"]);
            Assert.Equal("Corn", result.First["ProductName"]);
            Assert.Equal(8000, result.First["TradingVolume"]);
        }

        [Theory]
        //Use multi times in different place
        [InlineData("/GetTradeByCountry(PortingCountryOrRegion=@p1)?@p1=Microsoft.Test.E2E.AspNet.OData.ParameterAlias.CountryOrRegion'USA'&$filter=PortingCountryOrRegion eq @p1 and @p2 gt 1000&$orderby=@p2&@p2=TradingVolume", 1, 0)]
        //Reference property under complex type
        [InlineData("/Trades?$filter=@p1 gt 0&$orderby=@p1&@p1=TradeLocation/ZipCode", 3, 1)]
        public async Task MiscParameterAlias(string queryUri, int expectedEntryCount, int expectedZipCode)
        {
            string requestBaseUri = this.BaseAddress;

            HttpResponseMessage response = await this.Client.GetAsync(requestBaseUri + queryUri);

            var json = await response.Content.ReadAsObject<JObject>();
            var result = json["value"] as JArray;
            Assert.Equal(expectedEntryCount, result.Count);
            Assert.Equal(expectedZipCode, result.First["TradeLocation"]["ZipCode"]);
        }

        [Fact]
        public async Task ParameterAliasWithUnresolvedPathSegment()
        {
            string requestBaseUri = this.BaseAddress;

            var queryUri = "/Trades/Microsoft.Test.E2E.AspNet.OData.ParameterAlias.GetTopTrading(productName=@p1)/unknown?@p1='Corn'";
            var response = await this.Client.GetAsync(requestBaseUri + queryUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsObject<JObject>();
            Assert.Equal("Corn", (string)json["value"]);
        }
        #endregion
    }
}
