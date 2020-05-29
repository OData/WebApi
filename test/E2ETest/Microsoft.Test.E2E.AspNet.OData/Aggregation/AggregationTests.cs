// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Aggregation
{
    public class AggregationTestsEFClassic: AggregationTests
    {
        public AggregationTestsEFClassic(WebHostTestFixture fixture)
            : base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.AddControllers(typeof(CustomersController));
            base.UpdateConfiguration(configuration);
        }

        [Theory]
        [InlineData("?$apply=groupby((Name), aggregate(Order/Price with Custom.StdDev as PriceStdDev))")]
        [InlineData("?$apply=groupby((Address/Name), aggregate(Id with Custom.StdDev as IdStdDev))")]
        [InlineData("?$apply=groupby((Order/Name), aggregate(Id with Custom.StdDev as IdStdDev))")]
        public async Task CustomAggregateStdDevWorks(string query)
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
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

#if NETCORE
    public class AggregationTestsEFCoreInMemory : AggregationTests
    {
        public AggregationTestsEFCoreInMemory(WebHostTestFixture fixture)
            : base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.AddControllers(typeof(CoreCustomersController<AggregationContextCoreInMemory>));
            base.UpdateConfiguration(configuration);
        }
    }

    public class AggregationTestsEFCoreSql : AggregationTests
    {
        public AggregationTestsEFCoreSql(WebHostTestFixture fixture)
            : base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.AddControllers(typeof(CoreCustomersController<AggregationContextCoreSql>));
            base.UpdateConfiguration(configuration);
        }
    }
#endif


#if !NETCORE
    public class LinqToSqlAggregationTests : WebHostTestBase
    {
        protected string AggregationTestBaseUrl => "{0}/aggregation/Customers";

        public LinqToSqlAggregationTests(WebHostTestFixture fixture)
            : base(fixture)
        {
        }


        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {

            configuration.AddControllers(typeof(LinqToSqlCustomersController));
            configuration.JsonReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.MapODataServiceRoute("aggregation", "aggregation",
                AggregationEdmModel.GetEdmModel(configuration));
        }

        [Fact]
        public async Task ApplyThrows()
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
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            var result = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("$apply query options not supported for LINQ to SQL providers",result);
        }
    }
#endif

    public abstract class AggregationTests : WebHostTestBase
    {
        protected string AggregationTestBaseUrl => "{0}/aggregation/Customers";

        public AggregationTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.JsonReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.MapODataServiceRoute("aggregation", "aggregation",
                AggregationEdmModel.GetEdmModel(configuration));
        }

#region "SQL logging"
        public static async Task CleanUpSQlCommandsLog(string BaseAddress)
        {
            string queryUrl = $"{BaseAddress}/aggregation/CleanCommands()";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.SendAsync(request);
        }

        public async Task<string> GetLastSQLCommand(string BaseAddress)
        {
            string queryUrl = $"{BaseAddress}/aggregation/GetLastCommand()";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.SendAsync(request);
            var lastCommandResponse = await response.Content.ReadAsStringAsync();
            return  (string)JObject.Parse(lastCommandResponse)["value"];
        }
#endregion

        [Fact]
        public async Task AggregateNavigationPropertyWorks()
        {
            // Arrange
            try
            {
                await CleanUpSQlCommandsLog(BaseAddress);

                string queryUrl =
                    string.Format(
                        AggregationTestBaseUrl + "?$apply=groupby((Name), aggregate(Order/Price with sum as TotalPrice))&$orderby=TotalPrice",
                        BaseAddress);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
                request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
                HttpClient client = new HttpClient();

                // Act
                HttpResponseMessage response = await client.SendAsync(request);

                // Assert
                var result = await response.Content.ReadAsObject<JObject>();
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                var results = result["value"] as JArray;
                Assert.Equal(3, results.Count);
                Assert.Equal("0", results[0]["TotalPrice"].ToString());
                Assert.Equal(JValue.CreateNull(), results[0]["Name"]);
                Assert.Equal("2000", results[1]["TotalPrice"].ToString());
                Assert.Equal("Customer0", results[1]["Name"].ToString());
                Assert.Equal("2500", results[2]["TotalPrice"].ToString());
                Assert.Equal("Customer1", results[2]["Name"].ToString());

                string lastCommand = await GetLastSQLCommand(BaseAddress);
                if (lastCommand != "")
                {
                    // Only one join with Customers table
                    var num = Regex.Matches(lastCommand, @"\[Customers]").Count;
                    Assert.True(num == 1, $"More than one Customers table reference in the output {lastCommand}");
                }
            }
            finally
            {
                await CleanUpSQlCommandsLog(BaseAddress);
            }
        }

        [Fact]
        public async Task GroupByNavigationPropertyWorks()
        {
            // Arrange
            string queryUrl =
                string.Format(
                    AggregationTestBaseUrl + "?$apply=groupby((Order/Name), aggregate(Id with sum as TotalId))&$orderby=TotalId desc",
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsObject<JObject>();
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
        public async Task GroupByNavigationPropertyWithComputeWorks()
        {
            // Arrange
            string queryUrl =
                string.Format(
                    AggregationTestBaseUrl + "?$apply=groupby((Order/Name), aggregate(Id with sum as TotalId))/compute(TotalId add TotalId as DoubleId)&$orderby=Order/Name",
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert

            var result = await response.Content.ReadAsObject<JObject>();
            System.Console.WriteLine(result);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var results = result["value"] as JArray;
            Assert.Equal(2, results.Count);
            Assert.Equal("30", results[0]["TotalId"].ToString());
            Assert.Equal("25", results[1]["TotalId"].ToString());
            Assert.Equal("60", results[0]["DoubleId"].ToString());
            Assert.Equal("50", results[1]["DoubleId"].ToString());
            var order0 = results[0]["Order"] as JObject;
            var order1 = results[1]["Order"] as JObject;
            Assert.Equal("Order0", order0["Name"].ToString());
            Assert.Equal("Order1", order1["Name"].ToString());
        }

        [Fact]
        public async Task GroupByEnumPropertyWorks()
        {
            // Arrange
            string queryUrl =
                string.Format(
                    AggregationTestBaseUrl + "?$apply=groupby((Bucket))&$orderby=Bucket",
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            var result = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var results = result["value"] as JArray;
            Assert.Equal(3, results.Count);
            Assert.Equal(JValue.CreateNull(), results[0]["Bucket"]);
            Assert.Equal("Small", results[1]["Bucket"].ToString());
            Assert.Equal("Big", results[2]["Bucket"].ToString());
        }

        [Fact]
        public async Task GroupByComplexPropertyWorks()
        {
            // Arrange
            string queryUrl =
                string.Format(
                    AggregationTestBaseUrl + "?$apply=groupby((Address/Name), aggregate(Id with sum as TotalId))&$orderby=TotalId",
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsObject<JObject>();
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
        public async Task GroupByMultipleNestedPropertiesWorks()
        {
            // Arrange
            string queryUrl =
                string.Format(
                    AggregationTestBaseUrl + "?$apply=groupby((Order/Name, Address/Street), aggregate(Id with sum as TotalId))&$orderby=TotalId",
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            var result = await response.Content.ReadAsObject<JObject>();
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
        public async Task AggregateWithCastWorks()
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
            var result = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var results = result["value"] as JArray;
            Assert.Single(results);
            Assert.Equal("4500", results[0]["TotalAmount"].ToString());
        }

        [Fact]
        public async Task AggregateWithConstantWorks()
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
            var result = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var results = result["value"] as JArray;
            Assert.Single(results);
            Assert.Equal("450", results[0]["TotalAmount"].ToString());
        }

        [Fact]
        public async Task AggregateAggregatedPropertyWorks()
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
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            var result = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var results = result["value"] as JArray;
            Assert.Single(results);
            Assert.Equal("4500", results[0]["TotalAmount"].ToString());
        }

        [Fact]
        public async Task AggregateAggregatedWitCastPropertyWorks()
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
            var result = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var results = result["value"] as JArray;
            Assert.Single(results);
            Assert.Equal("4500", results[0]["TotalAmount"].ToString());
        }

        [Fact]
        public async Task AggregateVirtualCountWorks()
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
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            var res = await response.Content.ReadAsStringAsync();
            JObject json = JObject.Parse(res);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JToken value = json["value"].Children().First();

            var anonymousResponse = new { Count = 0 };
            var responseObj = JsonConvert.DeserializeAnonymousType(value.ToString(), anonymousResponse);

            Assert.Equal(10, responseObj.Count);
        }

        [Fact]
        public async Task GroupByVirtualCountWorks()
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
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject json = JObject.Parse(await response.Content.ReadAsStringAsync());
            IList<JToken> value = json["value"].Children().ToList();

            var responseObj = new { Name = "", Count = 0 };
            var dict = value
                .Select(x => JsonConvert.DeserializeAnonymousType(x.ToString(), responseObj))
                .ToDictionary(x => x.Name);

            dict.TryGetValue("Customer1", out responseObj);
            Assert.Equal(5, responseObj.Count);

            dict.TryGetValue("Customer0", out responseObj);
            Assert.Equal(4, responseObj.Count);
        }


        [Fact]
        public async Task AggregateAggregatedWithGRoupByPropertyWorks()
        {
            // Arrange
            string queryUrl =
                string.Format(
                    AggregationTestBaseUrl +
                    "?$apply=groupby((Name), aggregate(Order/Price with sum as TotalPrice))/groupby((Name),aggregate(TotalPrice with sum as TotalAmount))&$orderby=TotalAmount",
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert

            var result = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var results = result["value"] as JArray;
            Assert.Equal(3, results.Count);
            Assert.Equal("0", results[0]["TotalAmount"].ToString());
            Assert.Equal("2000", results[1]["TotalAmount"].ToString());
            Assert.Equal("2500", results[2]["TotalAmount"].ToString());
            Assert.Equal(JValue.CreateNull(), results[0]["Name"]);
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
        public async Task FilterWorks(string query)
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
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            var result = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var results = result["value"] as JArray;
            Assert.Single(results);
        }

        [Theory]
        [InlineData("?$apply=groupby((Name), aggregate(Order/Price with Custom.Sum as PriceStdDev))")]
        [InlineData("?$apply=groupby((Address/Name), aggregate(Id with Custom.OtherMethod as IdStdDev))")]
        [InlineData("?$apply=groupby((Order/Name), aggregate(Id with Custom.YetAnotherMethod as IdStdDev))")]
        public async Task CustomAggregateNotDefinedHaveAppropriateAnswer(string query)
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
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData("?$apply=groupby((Name), aggregate(Order/Price with StdDev as PriceStdDev))")]
        [InlineData("?$apply=groupby((Address/Name), aggregate(Id with OtherMethod as IdStdDev))")]
        [InlineData("?$apply=groupby((Order/Name), aggregate(Id with YetAnotherMethod as IdStdDev))")]
        public async Task MethodsNotDefinedHaveAppropriateAnswer(string query)
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
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData("?$apply=aggregate(Order/Price with sum as Result)", "4500")]
        [InlineData("?$apply=aggregate(Order/Price with min as Result)", "0")]
        [InlineData("?$apply=aggregate(Order/Price with max as Result)", "900")]
        [InlineData("?$apply=aggregate(Order/Price with average as Result)", "450")]
#if !NETCORE3x
        [InlineData("?$apply=aggregate(Order/Price with countdistinct as Result)", "10")]
        [InlineData("?$apply=aggregate(Order/Price with countdistinct as Result)&$orderby=Result", "10")]
#endif
        public async Task AggregateMethodWorks(string query, string expectedResult)
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
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            var result = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var results = result["value"] as JArray;
            Assert.Single(results);
            Assert.Equal(expectedResult, results[0]["Result"].ToString());
        }

        [Fact]
        public async Task ComputeAfterAggregateWorks()
        {
            // Arrange
            string queryUrl =
                string.Format(
                    AggregationTestBaseUrl +
                    "?$apply=aggregate(cast(Order/Price, Edm.Decimal) with sum as TotalAmount)"
                    + "/compute(TotalAmount mul 2 as DoubleAmount)",
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert

            var result = await response.Content.ReadAsObject<JObject>();
            System.Console.WriteLine(result);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var results = result["value"] as JArray;
            Assert.Single(results);
            Assert.Equal("4500", results[0]["TotalAmount"].ToString());
            Assert.Equal("9000", results[0]["DoubleAmount"].ToString());
        }

        [Fact]
        public async Task ComputeAfterGroupByWorks()
        {
            // Arrange
            string queryUrl =
                string.Format(
                    AggregationTestBaseUrl + "?$apply=groupby((Name), aggregate(Order/Price with sum as TotalAmount))"
                    + "/filter(Name ne null)/compute(TotalAmount mul 2 as DoubleAmount, length(Name) as NameLen)"
                    + "&$orderby=TotalAmount",
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            var result = await response.Content.ReadAsObject<JObject>();
            System.Console.WriteLine(result);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var results = result["value"] as JArray;
            Assert.Equal(2, results.Count);
            Assert.Equal("2000", results[0]["TotalAmount"].ToString());
            Assert.Equal("4000", results[0]["DoubleAmount"].ToString());
            Assert.Equal("9", results[0]["NameLen"].ToString());
            Assert.Equal("Customer0", results[0]["Name"].ToString());
            Assert.Equal("2500", results[1]["TotalAmount"].ToString());
            Assert.Equal("5000", results[1]["DoubleAmount"].ToString());
            Assert.Equal("9", results[1]["NameLen"].ToString());
            Assert.Equal("Customer1", results[1]["Name"].ToString());
        }

        [Fact]
        public async Task ComputeBeforeGroupByWorks()
        {
            // Arrange
            string queryUrl =
                string.Format(
                    AggregationTestBaseUrl + "?$apply=filter(Name ne null)/compute(length(Name) as NameLen)/groupby((Name), aggregate(Id with sum as TotalId, NameLen with max as NameLen))"
                    + "&$orderby=TotalId",
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            var result = await response.Content.ReadAsObject<JObject>();
            System.Console.WriteLine(result);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var results = result["value"] as JArray;
            Assert.Equal(2, results.Count);
            Assert.Equal("20", results[0]["TotalId"].ToString());
            Assert.Equal("9", results[0]["NameLen"].ToString());
            Assert.Equal("Customer0", results[0]["Name"].ToString());
            Assert.Equal("25", results[1]["TotalId"].ToString());
            Assert.Equal("9", results[1]["NameLen"].ToString());
            Assert.Equal("Customer1", results[1]["Name"].ToString());
        }

        [Fact]
        public async Task ComputeWorks()
        {
            // Arrange
            string queryUrl =
                string.Format(
                    AggregationTestBaseUrl + "?$apply=compute(length(Name) as NameLen)&$filter=Name ne null",
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            var result = await response.Content.ReadAsObject<JObject>();
            System.Console.WriteLine(result);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var results = result["value"] as JArray;
            Assert.Equal(9, results.Count);
            foreach (var customer in results)
            {
                Assert.NotNull(customer["Id"]);
                var name = customer["Name"]?.ToString();
                if (JValue.CreateNull().Equals(customer["Name"]))
                {
                    Assert.Equal(JValue.CreateNull(), customer["NameLen"]);
                }
                else
                {
                    Assert.Equal(customer["Name"].ToString().Length.ToString(), customer["NameLen"].ToString());
                }
            }
        }
    }
}
