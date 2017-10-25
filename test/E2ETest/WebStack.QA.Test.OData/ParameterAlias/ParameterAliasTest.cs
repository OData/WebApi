﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.ParameterAlias
{
    [NuwaFramework]
    public class ParameterAliasTest
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            var controllers = new[] { typeof(TradesController) };
            TestAssemblyResolver resolver = new TestAssemblyResolver(new TypesInjectionAssembly(controllers));

            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Services.Replace(typeof(IAssembliesResolver), resolver);

            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.MapODataServiceRoute("OData", "", GetModel());
            configuration.EnsureInitialized();
        }

        private static IEdmModel GetModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
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
            string query = "/GetTradeByCountry(PortingCountryOrRegion=@p1)?@p1=WebStack.QA.Test.OData.ParameterAlias.CountryOrRegion'USA'";

            HttpResponseMessage response = this.Client.GetAsync(this.BaseAddress + query).Result;
            var json = await response.Content.ReadAsAsync<JObject>();
            var result = json["value"] as JArray;
            Assert.Equal(3, result.Count);

            //Bound function
            string requestUri = this.BaseAddress + "/Trades/WebStack.QA.Test.OData.ParameterAlias.GetTradingVolume(productName=@p1, PortingCountryOrRegion=@p2)?@p1='Rice'&@p2=WebStack.QA.Test.OData.ParameterAlias.CountryOrRegion'USA'";
            response = this.Client.GetAsync(requestUri).Result;
            json = await response.Content.ReadAsAsync<JObject>();
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

            var json = await response.Content.ReadAsAsync<JObject>();
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

            var json = await response.Content.ReadAsAsync<JObject>();
            var result = json["value"] as JArray;
            Assert.Equal(expectedPortingCountry, result.First["PortingCountryOrRegion"]);
            Assert.Equal("Corn", result.First["ProductName"]);
            Assert.Equal(8000, result.First["TradingVolume"]);
        }

        [Theory]
        //Use multi times in different place
        [InlineData("/GetTradeByCountry(PortingCountryOrRegion=@p1)?@p1=WebStack.QA.Test.OData.ParameterAlias.CountryOrRegion'USA'&$filter=PortingCountryOrRegion eq @p1 and @p2 gt 1000&$orderby=@p2&@p2=TradingVolume", 1, 0)]
        //Reference property under complex type
        [InlineData("/Trades?$filter=@p1 gt 0&$orderby=@p1&@p1=TradeLocation/ZipCode", 3, 1)]
        public async Task MiscParameterAlias(string queryUri, int expectedEntryCount, int expectedZipCode)
        {
            string requestBaseUri = this.BaseAddress;

            HttpResponseMessage response = await this.Client.GetAsync(requestBaseUri + queryUri);

            var json = await response.Content.ReadAsAsync<JObject>();
            var result = json["value"] as JArray;
            Assert.Equal(expectedEntryCount, result.Count);
            Assert.Equal(expectedZipCode, result.First["TradeLocation"]["ZipCode"]);
        }

        [Fact]
        public async Task ParameterAliasWithUnresolvedPathSegment()
        {
            string requestBaseUri = this.BaseAddress;

            var queryUri = "/Trades/WebStack.QA.Test.OData.ParameterAlias.GetTopTrading(productName=@p1)/unknown?@p1='Corn'";
            var response = await this.Client.GetAsync(requestBaseUri + queryUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsAsync<JObject>();
            Assert.Equal("Corn", (string)json["value"]);
        }
        #endregion
    }
}
