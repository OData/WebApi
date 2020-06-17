// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ModelBoundQuerySettings.PageAttributeTest.SkipTokenTest
{
    public class SkipTokenTest : WebHostTestBase
    {
        private const string CustomerBaseUrl = "{0}/skiptokentest/Customers";
        private const string OrderBaseUrl = "{0}/skiptokentest/Orders";
        private const string DatesBaseUrl = "{0}/skiptokentest/Dates";

        public SkipTokenTest(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.AddControllers(typeof(CustomersController), typeof(OrdersController), typeof(DatesController));
            configuration.JsonReferenceLoopHandling =
                Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.MaxTop(2).Expand().Filter().OrderBy().SkipToken();
            configuration.MapODataServiceRoute("skiptokentest", "skiptokentest",
                SkipTokenEdmModel.GetEdmModel(configuration));
            configuration.SetTimeZoneInfo(TimeZoneInfo.Utc);
        }

        [Theory]
        [InlineData("full")]
        [InlineData("minimal")]
        [InlineData("none")]
        public async Task GenerateSkiptokenOnEntitySet(string metaDataLevel)
        {
            string queryUrl = string.Format(CustomerBaseUrl, BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=" + metaDataLevel));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(string.Format(CustomerBaseUrl, "") + "?$skiptoken=Id-2", result);
        }

        [Fact]
        public async Task GenerateSkiptokenOnNavigationPropety()
        {
            string queryUrl = string.Format(CustomerBaseUrl + "(1)/Orders", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Orders?$skiptoken=Id-5", result);
        }

        [Theory]
        [InlineData("?$orderby=Name", "$skiptoken=Name-'Customer2',Id-2")]
        [InlineData("?$expand=Orders", "$skiptoken=Id-2")]
        [InlineData("?$filter=Id gt 2", "$skiptoken=Id-4")]
        [InlineData("?$orderby=Name desc", "$skiptoken=Name-'Customer8',Id-8")]
        [InlineData("?$orderby=Name desc&$filter=Id gt 2&$expand=Orders", "$skiptoken=Name-'Customer8',Id-8")]
        public async Task GenerateSkiptokenWithQueryOptions(string queryOption, string expected)
        {
            string queryUrl = string.Format(CustomerBaseUrl + queryOption, BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(expected, result);
        }

        [Theory]
        [InlineData("?$orderby=Name&$skiptoken=Name-'Customer2',Id-2", "$skiptoken=Name-'Customer4',Id-4")]
        [InlineData("?$expand=Orders&$skiptoken=Id-2", "$skiptoken=Id-4")]
        [InlineData("?$filter=Id gt 2&$skiptoken=Id-4", "$skiptoken=Id-6")]
        [InlineData("?$orderby=Name desc&$skiptoken=Name-'Customer8',Id-8", "$skiptoken=Name-'Customer6',Id-6")]
        [InlineData("?$orderby=Name desc&$filter=Id gt 2&$expand=Orders&$skiptoken=Name-'Customer8',Id-8", "$skiptoken=Name-'Customer6',Id-6")]
        [InlineData("?$orderby=Name%20desc&$filter=Id%20gt%202&$expand=Orders&$skiptoken=Name-'Customer6',Id-6", "$skiptoken=Name-'Customer4',Id-4")]
        public async Task ConsumeNextLinkOnEntityType(string nextLink, string expected)
        {
            string queryUrl = string.Format(CustomerBaseUrl + nextLink, BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(expected, result);
        }

        [Theory]
        [InlineData("?$skip=2", "$skiptoken=Id-4")]
        [InlineData("?$top=5", "$top=3&$skiptoken=Id-2")]
        [InlineData("?$skip=2&$top=5", "$top=3&$skiptoken=Id-4")]
        [InlineData("?$TOP=5&$sKiP=2", "$top=3&$skiptoken=Id-4")]
        public async Task SkipAndTopWithSkiptoken(string url, string expected)
        {
            string queryUrl = string.Format(CustomerBaseUrl + url, BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(expected, result);
        }

        [Fact]
        public async Task VerifyLastSetDoesNotHaveNextLink()
        {
            string queryUrl = string.Format(CustomerBaseUrl + "?$orderby=Name&$skiptoken=Name-'Customer8',Id-8", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.DoesNotContain("nextLink", result);
        }

        [Theory]
        [InlineData("Token", "$skiptoken=Token-5af3c516-2d3c-4033-95af-07591f18439c,Id-3")]
        [InlineData("DateTimeOfBirth", "$skiptoken=DateTimeOfBirth-2000-01-02T00:00:00Z,Id-2")]
        [InlineData("Skill", "$skiptoken=Skill-Microsoft.Test.E2E.AspNet.OData.Enums.Skill'CSharp',Id-4")]
        public async Task GenerateSkiptokenWithDifferentPrimitive(string property, string expected)
        {
            string queryUrl = string.Format(CustomerBaseUrl + "?$orderby="+ property, BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(expected, result);
        }

        [Theory]
        [InlineData("?$orderby=Token&$skiptoken=Token-5af3c516-2d3c-4033-95af-07591f18439c,Id-3", "$skiptoken=Token-5af3c516-2d3c-4033-95af-07591f18439c,Id-7")]
        [InlineData("?$orderby=DateTimeOfBirth&$skiptoken=DateTimeOfBirth-2000-01-02T00:00:00Z,Id-2", "$skiptoken=DateTimeOfBirth-2000-01-04T00:00:00Z,Id-4")]
        [InlineData("?$orderby=Skill&$skiptoken=Skill-Microsoft.Test.E2E.AspNet.OData.Enums.Skill'CSharp',Id-4", "$skiptoken=Skill-Microsoft.Test.E2E.AspNet.OData.Enums.Skill'CSharp',Id-8")]
        public async Task ConsumeSkiptokenWithOtherPrimitives(string nextLink, string expected)
        {
            string queryUrl = string.Format(CustomerBaseUrl + nextLink, BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(expected, result);
        }

        [Fact]
        public async Task ComplexCollectionShouldStillUseSkip()
        {
            string queryUrl = string.Format(CustomerBaseUrl + "(1)/Addresses", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.DoesNotContain("$skiptoken", result);
            Assert.Contains("?$skip=2", result);
        }

        [Theory]
        [InlineData("?$expand=Orders($orderby=Name)&$skiptoken=Id-2", "$skiptoken=Id-4")]
        public async Task OrderByInExpandOnNestedProperty(string nextLink, string expected)
        {
            string queryUrl = string.Format(CustomerBaseUrl + nextLink, BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(expected, result);
        }

        [Theory]
        [InlineData("$orderBy=Name", "Details?$orderby=Name&$skiptoken=Name-'2ndOrder',Id-2")]
        [InlineData("$orderBy=Name desc", "$skiptoken=Name-'3rdOrder',Id-3")]
        [InlineData("$orderBy=Name;$skip=1", "Details?$orderby=Name&$skiptoken=Name-'3rdOrder',Id-3")]
        public async Task NestedNestedQueryOptionInNextPageLink(string queryOption, string expected)
        {
            // Arrange
            string queryUrl = string.Format(CustomerBaseUrl +
                    "?$expand=Orders($expand=Details(" + queryOption + "))", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.NotNull(response.Content);
            String result = await response.Content.ReadAsStringAsync();
            Assert.Contains(expected, result);
        }

        [Fact]
        public async Task OrderByUntypedPropertyShouldUseSkip()
        {

            string queryUrl = string.Format(CustomerBaseUrl + "?$orderBy=DynamicProperty1", BaseAddress);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.DoesNotContain("$skiptoken", result);
            Assert.Contains("&$skip=2", result);
        }

        [Theory]
        [InlineData("", "?$skiptoken=DateValue-2019-11-09T00:00:01Z")]
        [InlineData("?$skiptoken=DateValue-2019-11-09T00:00:01Z", "?$skiptoken=DateValue-2019-11-09T00:00:03Z")]
        public async Task GenerateSkiptokenOnEntitySetWithDateTime(string queryOptions, string expected)
        {
            string queryUrl = string.Format(DatesBaseUrl, BaseAddress) + queryOptions;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(string.Format(DatesBaseUrl, "") + expected, result);
        }
    }
}