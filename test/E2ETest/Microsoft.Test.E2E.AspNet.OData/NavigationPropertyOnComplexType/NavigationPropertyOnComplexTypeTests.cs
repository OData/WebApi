// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.NavigationPropertyOnComplexType
{
    public class NavigationPropertyOnComplexType : WebHostTestBase
    {
        private const string PeopleBaseUrl = "{0}/odata/people";

        public NavigationPropertyOnComplexType(WebHostTestFixture fixture)
            : base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.AddControllers(typeof(PeopleController));
            configuration.JsonReferenceLoopHandling =
                Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.MaxTop(2).Expand().Select().OrderBy().Filter();
            configuration.MapODataServiceRoute("odata", "odata", ModelGenerator.GetConventionalEdmModel());
        }

        private const string expandedLocation = "\"Location\":{\"ZipCode\":{\"Zip\":98030,\"City\":\"Kent\",\"State\":\"Washington\"}}";
        private const string expandedHome = "\"Home\":{\"ZipCode\":{\"Zip\":98052,\"City\":\"Redmond\",\"State\":\"Washington\"}}";
        private const string expandedOrder = "\"Order\":{\"Zip\":{\"ZipCode\":{\"Zip\":98052,\"City\":\"Redmond\",\"State\":\"Washington\"}}}";

        [Theory]
        [InlineData("/Location/ZipCode", "\"Zip\":98030,\"City\":\"Kent\",\"State\":\"Washington\"}")]
        [InlineData("/Location?$select=Street&$expand=ZipCode", "\"Street\":\"110th\",\"ZipCode\":{\"Zip\":98030,\"City\":\"Kent\",\"State\":\"Washington\"}}")]
        [InlineData("/Location?$expand=ZipCode", "\"Street\":\"110th\",\"ZipCode\":{\"Zip\":98030,\"City\":\"Kent\",\"State\":\"Washington\"}}")]
        [InlineData("?$expand=Location/ZipCode", "\"Id\":1,\"FirstName\":\"Kate\",\"LastName\":\"Jones\",\"Age\":5,\"Home\":{\"Street\":\"110th\"},\"PreciseLocation\":null,\"Order\":{\"Zip\":{\"Street\":\"110th\"},\"Order\":null}," + expandedLocation + "}")]
        [InlineData("?$expand=Location/ZipCode,Home/ZipCode", "\"Id\":1,\"FirstName\":\"Kate\",\"LastName\":\"Jones\",\"Age\":5,\"PreciseLocation\":null,\"Order\":{\"Zip\":{\"Street\":\"110th\"},\"Order\":null}," + expandedLocation + "," + expandedHome + "}")]
        [InlineData("?$expand=Location/ZipCode,Order/Zip/ZipCode", "\"Id\":1,\"FirstName\":\"Kate\",\"LastName\":\"Jones\",\"Age\":5,\"Home\":{\"Street\":\"110th\"},\"PreciseLocation\":null," + expandedLocation + "," + expandedOrder + "}")]
        public async Task SerializingNavigationPropertyOnComplexType(string queryOption, string expected)
        {
            string resourcePath = PeopleBaseUrl + "(1)";
            string queryUrl =
                string.Format(
                    resourcePath + queryOption,
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(expected, result);
        }

        [Theory]
        [InlineData("?$expand=Location/ZipCode", "{\"Zip\":98030,\"City\":\"Kent\",\"State\":\"Washington\"}")]
        [InlineData("?$expand=Location/ZipCode,Home/ZipCode", expandedLocation + "," + expandedHome)]
        [InlineData("?$expand=Location/ZipCode,Order/Zip/ZipCode", expandedLocation + "," + expandedOrder)]
        public async Task NavigationPropertyWithTopLevelResource(string queryOption, string expected)
        {
            string queryUrl =
                string.Format(
                    PeopleBaseUrl + queryOption,
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(expected, result);
        }

        [Fact]
        public async Task DeserializingNavigationPropertyOnComplexType()
        {
            string url =  PeopleBaseUrl + "(1)/Location/ZipCode/$ref";
            string payload = "{\"Zip\":98038,\"City\":\"Redmond\",\"State\":\"Washington\"}";
            HttpContent content = new StringContent(payload, Encoding.UTF8, mediaType: "application/json");
            string queryUrl =
                string.Format(
                    url,
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, queryUrl);
            request.Content = content;
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(payload.TrimStart('{'), result);
        }

        [Theory]
        [InlineData("Location?$expand=Microsoft.Test.E2E.AspNet.OData.NavigationPropertyOnComplexType.Address/ZipCode", "\"Street\":\"110th\",\"Latitude\":\"12.211\",\"Longitude\":\"231.131\",\"ZipCode\":{\"Zip\":98030,\"City\":\"Kent\",\"State\":\"Washington\"}}")]
        [InlineData("Location/Microsoft.Test.E2E.AspNet.OData.NavigationPropertyOnComplexType.Address?$expand=ZipCode", "\"Street\":\"110th\",\"Latitude\":\"12.211\",\"Longitude\":\"231.131\",\"ZipCode\":{\"Zip\":98030,\"City\":\"Kent\",\"State\":\"Washington\"}}")]
        [InlineData("?$expand=Location/Microsoft.Test.E2E.AspNet.OData.NavigationPropertyOnComplexType.Address/ZipCode", "\"@odata.type\":\"#Microsoft.Test.E2E.AspNet.OData.NavigationPropertyOnComplexType.GeoLocation\",\"ZipCode\":{\"Zip\":98030,\"City\":\"Kent\",\"State\":\"Washington\"}}")]
        [InlineData("?$expand=Location/ZipCode", "\"@odata.type\":\"#Microsoft.Test.E2E.AspNet.OData.NavigationPropertyOnComplexType.GeoLocation\",\"ZipCode\":{\"Zip\":98030,\"City\":\"Kent\",\"State\":\"Washington\"}}")]
        public async Task ExpandOnDerivedType(string query, string expected)
        {
            string url = PeopleBaseUrl + "(2)/" + query;
            string queryUrl =
                string.Format(
                    url,
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(expected, result);
        }

        [Theory]
        [InlineData("?$select=Age&$expand=Location/ZipCode,Home/ZipCode", "\"Age\":5," + expandedLocation + "," + expandedHome)]
        [InlineData("?$select=Location&$expand=Location/ZipCode,Order/Zip/ZipCode", expandedLocation + "," + expandedOrder)]
        [InlineData("?$select=Location&$expand=Location/ZipCode,Order/Zip/ZipCode($select=State)", expandedLocation + "," + "\"Order\":{\"Zip\":{\"ZipCode\":{\"State\":\"Washington\"}}}")]
        public async Task SelectAndExpandCombos(string queryOption, string expected)
        {
            string resourcePath = PeopleBaseUrl + "(1)";
            string queryUrl =
                string.Format(
                    resourcePath + queryOption,
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(expected, result);
        }

        [Theory]
        [InlineData("?$expand=PreciseLocation/Area", "\"Id\":3,\"FirstName\":\"Carlos\",\"LastName\":\"Park\",\"Age\":7,\"Location\":{\"Street\":\"110th\"},\"Home\":{\"Street\":\"110th\"},\"Order\":{\"Zip\":{\"Street\":\"110th\"},\"Order\":null},\"PreciseLocation\":{\"Area\":{\"Zip\":98004,\"City\":\"Bellevue\",\"State\":\"Washington\"}}}")]
        [InlineData("?$expand=PreciseLocation/ZipCode", "\"Id\":3,\"FirstName\":\"Carlos\",\"LastName\":\"Park\",\"Age\":7,\"Location\":{\"Street\":\"110th\"},\"Home\":{\"Street\":\"110th\"},\"Order\":{\"Zip\":{\"Street\":\"110th\"},\"Order\":null},\"PreciseLocation\":{\"ZipCode\":{\"Zip\":98030,\"City\":\"Kent\",\"State\":\"Washington\"}}}")]
        public async Task ExpandOnDeclaredAndInheritedProperties(string queryOption, string expected)
        {
            string resourcePath = PeopleBaseUrl + "(3)";
            string queryUrl =
                string.Format(
                    resourcePath + queryOption,
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(expected, result);
        }

        [Theory]
        //[InlineData("?$expand=Order/Zip/ZipCode", "\"Order\":{\"Zip\":{\"ZipCode\":{\"Zip\":98052,\"City\":\"Redmond\",\"State\":\"Washington\"}}}}")]
        [InlineData("?$expand=Order/Order/Zip/ZipCode", "\"Order\":{\"Order\":{\"Zip\":{\"ZipCode\":{\"Zip\":98030,\"City\":\"Kent\",\"State\":\"Washington\"}}}}}")]
        public async Task RecursiveExpandOnOrders(string queryOption, string expected)
        {
            string resourcePath = PeopleBaseUrl + "(4)";
            string queryUrl =
                string.Format(
                    resourcePath + queryOption,
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(expected, result);
        }
    }
}

