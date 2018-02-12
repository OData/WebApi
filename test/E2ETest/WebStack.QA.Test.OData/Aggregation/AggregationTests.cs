// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nuwa;
using Xunit;
using Xunit.Extensions;
using WebStack.QA.Test.OData.Common;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace WebStack.QA.Test.OData.Aggregation
{
    public class AggregationTests : ODataTestBase
    {
        private const string AggregationTestBaseUrl = "{0}/aggregation/Customers";

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.Services.Replace(
                typeof (IAssembliesResolver),
                new TestAssemblyResolver(typeof (CustomersController)));
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling =
                Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.MapODataServiceRoute("aggregation", "aggregation",
                AggregationEdmModel.GetEdmModel(configuration));
        }

        [Fact]
        public void AggregateNavigationPropertyWorks()
        {
            // Arrange
            string queryUrl =
                string.Format(
                    AggregationTestBaseUrl + "?$apply=groupby((Name), aggregate(Order/Price with sum as TotalPrice))",
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            var result = response.Content.ReadAsAsync<JObject>().Result;
            System.Console.WriteLine(result);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var results = result["value"] as JArray;
            Assert.Equal(3, results.Count);
            Assert.Equal("0", results[0]["TotalPrice"].ToString());
            Assert.Equal(null, results[0]["Name"]);
            Assert.Equal("2000", results[1]["TotalPrice"].ToString());
            Assert.Equal("Customer0", results[1]["Name"].ToString());
            Assert.Equal("2500", results[2]["TotalPrice"].ToString());
            Assert.Equal("Customer1", results[2]["Name"].ToString());
        }



        [Fact]
        public void GroupByNavigationPropertyWorks()
        {
            // Arrange
            string queryUrl =
                string.Format(
                    AggregationTestBaseUrl + "?$apply=groupby((Order/Name), aggregate(Id with sum as TotalId))",
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = response.Content.ReadAsAsync<JObject>().Result;
            var results = result["value"] as JArray;
            Assert.Equal(2, results.Count);
            Assert.Equal("30", results[0]["TotalId"].ToString());
            Assert.Equal("25", results[1]["TotalId"].ToString());
            var order0 = results[0]["Order"] as JObject;
            var order1 = results[1]["Order"] as JObject;
            Assert.Equal("Order0", order0["Name"].ToString());
            Assert.Equal("Order1", order1["Name"].ToString());
        }

        [Fact]
        public void GroupByComplexPropertyWorks()
        {
            // Arrange
            string queryUrl =
                string.Format(
                    AggregationTestBaseUrl + "?$apply=groupby((Address/Name), aggregate(Id with sum as TotalId))",
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = response.Content.ReadAsAsync<JObject>().Result;
            var results = result["value"] as JArray;
            Assert.Equal(2, results.Count);
            Assert.Equal("20", results[0]["TotalId"].ToString());
            Assert.Equal("35", results[1]["TotalId"].ToString());
            var address0 = results[0]["Address"] as JObject;
            var address1 = results[1]["Address"] as JObject;
            Assert.Equal("City0", address0["Name"].ToString());
            Assert.Equal("City1", address1["Name"].ToString());
        }

        [Fact]
        public void GroupByMultipleNestedPropertiesWorks()
        {
            // Arrange
            string queryUrl =
                string.Format(
                    AggregationTestBaseUrl + "?$apply=groupby((Order/Name, Address/Street), aggregate(Id with sum as TotalId))",
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            var result = response.Content.ReadAsAsync<JObject>().Result;
            System.Console.WriteLine(result);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var results = result["value"] as JArray;
            Assert.Equal(3, results.Count);
            Assert.Equal("10", results[0]["TotalId"].ToString());
            Assert.Equal("20", results[1]["TotalId"].ToString());
            Assert.Equal("25", results[2]["TotalId"].ToString());
            var order0 = results[0]["Order"] as JObject;
            var order1 = results[1]["Order"] as JObject;
            var order2 = results[2]["Order"] as JObject;
            Assert.Equal("Order0", order0["Name"].ToString());
            Assert.Equal("Order0", order1["Name"].ToString());
            Assert.Equal("Order1", order2["Name"].ToString());
        }


        [Fact]
        public void AggregateWithCastWorks()
        {
            // Arrange
            string queryUrl =
                string.Format(
                    AggregationTestBaseUrl +
                    "?$apply=aggregate(cast(Order/Price, Edm.Decimal) with sum as TotalAmount)",
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert

            var result = response.Content.ReadAsAsync<JObject>().Result;
            System.Console.WriteLine(result);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var results = result["value"] as JArray;
            Assert.Equal(1, results.Count);
            Assert.Equal("4500", results[0]["TotalAmount"].ToString());
        }

        [Fact]
        public void AggregateWithConstantWorks()
        {
            // Arrange
            string queryUrl =
                string.Format(
                    AggregationTestBaseUrl +
                    "?$apply=aggregate(Order/Price div 10 with sum as TotalAmount)",
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert

            var result = response.Content.ReadAsAsync<JObject>().Result;
            System.Console.WriteLine(result);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var results = result["value"] as JArray;
            Assert.Equal(1, results.Count);
            Assert.Equal("450", results[0]["TotalAmount"].ToString());
        }

        [Fact]
        public void AggregateAggregatedPropertyWorks()
        {
            // Arrange
            string queryUrl =
                string.Format(
                    AggregationTestBaseUrl +
                    "?$apply=groupby((Name), aggregate(Order/Price with sum as TotalPrice))/aggregate(TotalPrice with sum as TotalAmount)",
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            
            var result = response.Content.ReadAsAsync<JObject>().Result;
            System.Console.WriteLine(result);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var results = result["value"] as JArray;
            Assert.Equal(1, results.Count);
            Assert.Equal("4500", results[0]["TotalAmount"].ToString());
        }

        [Fact]
        public void AggregateAggregatedWitCastPropertyWorks()
        {
            // Arrange
            string queryUrl =
                string.Format(
                    AggregationTestBaseUrl +
                    "?$apply=groupby((Name), aggregate(cast(Order/Price, Edm.Int64) with sum as TotalPrice))/aggregate(cast(TotalPrice, Edm.Decimal) with sum as TotalAmount)",
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert

            var result = response.Content.ReadAsAsync<JObject>().Result;
            System.Console.WriteLine(result);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var results = result["value"] as JArray;
            Assert.Equal(1, results.Count);
            Assert.Equal("4500", results[0]["TotalAmount"].ToString());
        }

        [Fact]
        public void AggregateVirtualCountWorks()
        {
            // Arrange
            string queryUrl =
                string.Format(
                    AggregationTestBaseUrl + "?$apply=aggregate($count as Count)",
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject json = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            JToken value = json["value"].Children().First();

            var anonymousResponse = new { Count = 0 };
            var responseObj = JsonConvert.DeserializeAnonymousType(value.ToString(), anonymousResponse);

            Assert.Equal(10, responseObj.Count);
        }

        [Fact]
        public void GroupByVirtualCountWorks()
        {
            // Arrange
            string queryUrl =
                string.Format(
                    AggregationTestBaseUrl + "?$apply=filter(Name ne null)/groupby((Name), aggregate($count as Count))",
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject json = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            IList<JToken> value = json["value"].Children().ToList();

            var responseObj = new { Name = "", Count = 0 };
            var dict = value
                .Select(x => JsonConvert.DeserializeAnonymousType(x.ToString(), responseObj))
                .ToDictionary(x => x.Name);

            dict.TryGetValue("Customer1", out responseObj);
            Assert.Equal(responseObj.Count, 5);

            dict.TryGetValue("Customer0", out responseObj);
            Assert.Equal(responseObj.Count, 4);
        }


        [Fact]
        public void AggregateAggregatedWithGRoupByPropertyWorks()
        {
            // Arrange
            string queryUrl =
                string.Format(
                    AggregationTestBaseUrl +
                    "?$apply=groupby((Name), aggregate(Order/Price with sum as TotalPrice))/groupby((Name),aggregate(TotalPrice with sum as TotalAmount))",
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert

            var result = response.Content.ReadAsAsync<JObject>().Result;
            System.Console.WriteLine(result);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var results = result["value"] as JArray;
            Assert.Equal(3, results.Count);
            Assert.Equal("0", results[0]["TotalAmount"].ToString());
            Assert.Equal("2000", results[1]["TotalAmount"].ToString());
            Assert.Equal("2500", results[2]["TotalAmount"].ToString());
            Assert.Equal(null, results[0]["Name"]);
            Assert.Equal("Customer0", results[1]["Name"].ToString());
            Assert.Equal("Customer1", results[2]["Name"].ToString());
        }

        [Theory]
        [InlineData("?$apply=groupby((Name), aggregate(Order/Price with sum as TotalPrice))" +
                    "/filter(TotalPrice ge 2001)")]
        [InlineData("?$apply=groupby((Address/Name), aggregate(Id with sum as TotalId))" +
                    "/filter(Address/Name ne 'City1')")]
        [InlineData("?$apply=groupby((Order/Name), aggregate(Id with sum as TotalId))" +
                    "/filter(Order/Name ne 'Order0')")]
        [InlineData("?$apply=groupby((Order/Name), aggregate(Id with sum as TotalId))" +
                    "/filter(Order/Name ne 'Order0')&$orderby=Order/Name")]
        [InlineData("?$apply=groupby((Order/Name), aggregate(Id with sum as TotalId))" +
                    "&$filter=Order/Name ne 'Order0'&$orderby=Order/Name")]
        public void FilterWorks(string query)
        {
            // Arrange
            string queryUrl =
                string.Format(
                    AggregationTestBaseUrl + query,
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            var result = response.Content.ReadAsAsync<JObject>().Result;
            System.Console.WriteLine(result);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var results = result["value"] as JArray;
            Assert.Equal(1, results.Count);
        }

        [Theory]
        [InlineData("?$apply=groupby((Name), aggregate(Order/Price with Custom.StdDev as PriceStdDev))")]
        [InlineData("?$apply=groupby((Address/Name), aggregate(Id with Custom.StdDev as IdStdDev))")]
        [InlineData("?$apply=groupby((Order/Name), aggregate(Id with Custom.StdDev as IdStdDev))")]
        public void CustomAggregateStdDevWorks(string query)
        {
            // Arrange
            string queryUrl =
                string.Format(
                    AggregationTestBaseUrl + query,
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;
            System.Console.WriteLine(response.Content.ReadAsStringAsync().Result);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData("?$apply=groupby((Name), aggregate(Order/Price with Custom.Sum as PriceStdDev))")]
        [InlineData("?$apply=groupby((Address/Name), aggregate(Id with Custom.OtherMethod as IdStdDev))")]
        [InlineData("?$apply=groupby((Order/Name), aggregate(Id with Custom.YetAnotherMethod as IdStdDev))")]
        public void CustomAggregateNotDefinedHaveAppropriateAnswer(string query)
        {
            // Arrange
            string queryUrl =
                string.Format(
                    AggregationTestBaseUrl + query,
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData("?$apply=groupby((Name), aggregate(Order/Price with StdDev as PriceStdDev))")]
        [InlineData("?$apply=groupby((Address/Name), aggregate(Id with OtherMethod as IdStdDev))")]
        [InlineData("?$apply=groupby((Order/Name), aggregate(Id with YetAnotherMethod as IdStdDev))")]
        public void MethodsNotDefinedHaveAppropriateAnswer(string query)
        {
            // Arrange
            string queryUrl =
                string.Format(
                    AggregationTestBaseUrl + query,
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData("?$apply=aggregate(Order/Price with sum as Result)", "4500")]
        [InlineData("?$apply=aggregate(Order/Price with min as Result)", "0")]
        [InlineData("?$apply=aggregate(Order/Price with max as Result)", "900")]
        [InlineData("?$apply=aggregate(Order/Price with average as Result)", "450")]
        [InlineData("?$apply=aggregate(Order/Price with countdistinct as Result)", "10")]
        [InlineData("?$apply=aggregate(Order/Price with countdistinct as Result)&$orderby=Result", "10")]
        public void AggregateMethodWorks(string query, string expectedResult)
        {
            // Arrange
            string queryUrl =
                string.Format(
                    AggregationTestBaseUrl + query,
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            var result = response.Content.ReadAsAsync<JObject>().Result;
            System.Console.WriteLine(result);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var results = result["value"] as JArray;
            Assert.Equal(1, results.Count);
            Assert.Equal(expectedResult, results[0]["Result"].ToString());
        }
    }
}
