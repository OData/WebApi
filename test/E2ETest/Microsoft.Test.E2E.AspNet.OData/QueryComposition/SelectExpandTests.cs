//-----------------------------------------------------------------------------
// <copyright file="SelectExpandTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.QueryComposition
{
    public class SelectExpandTests : WebHostTestBase
    {
        public SelectExpandTests(WebHostTestFixture fixture)
            : base(fixture)
        {
        }

        private static readonly int SelectCustomerPropertyCount =
            typeof(SelectCustomer).GetProperties().Length + 1;  // The +1 is for SelectOrders@odata.count.

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.AddControllers(typeof(SelectCustomerController));
            configuration.JsonReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null).Select();
            configuration.MapODataServiceRoute("selectexpand", "selectexpand", GetEdmModel(configuration));
        }

        private static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var builder = configuration.CreateConventionModelBuilder();

            EntitySetConfiguration<SelectCustomer> customers = builder.EntitySet<SelectCustomer>("SelectCustomer");
            customers.EntityType.Action("CreditRating").Returns<double>();
            customers.EntityType.Collection.Action("PremiumCustomers").ReturnsCollectionFromEntitySet<SelectCustomer>("SelectCustomer");
            builder.EntitySet<SelectOrderDetail>("SelectOrderDetail");
            builder.EntityType<SelectPremiumCustomer>();
            builder.EntitySet<SelectOrder>("SelectOrder");
            builder.EntitySet<SelectBonus>("SelectBonus");

            return builder.GetEdmModel();
        }

        [Fact]
        public async Task QueryJustThePropertiesOfTheEntriesOnAFeed()
        {
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer/?$select=*", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;
            JObject result;
            string content;

            response = await client.SendAsync(request);

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            content = await response.Content.ReadAsStringAsync();

            result = JObject.Parse(content);
            Assert.NotNull(result);
            JsonAssert.ArrayLength(10, "value", result);
            JArray customers = (JArray)result["value"];
            Assert.True(customers.OfType<JObject>().All(x => x.Properties().All(p => p.Name != "#Container.CreditRating")));
        }

        [Fact]
        public async Task QueryJustTheActionsOfTheEntriesOnAFeed()
        {
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer/?$select=Default.*", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;
            JObject result;
            string content;

            response = await client.SendAsync(request);

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            content = await response.Content.ReadAsStringAsync();

            result = JObject.Parse(content);
            Assert.NotNull(result);
            JsonAssert.ArrayLength(10, "value", result);
            JArray customers = (JArray)result["value"];
            Assert.True(customers.OfType<JObject>().All(x => x.Properties().Any(p => p.Name == "#Default.CreditRating")));
        }

        [Fact]
        public async Task QueryASubsetOfThePropertiesOfAnEntry()
        {
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer/?$select=Name", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata=minimalmetadata"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;
            JObject result;
            string content;

            response = await client.SendAsync(request);

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            content = await response.Content.ReadAsStringAsync();

            result = JObject.Parse(content);
            Assert.NotNull(result);
            JsonAssert.ArrayLength(10, "value", result);
            JArray customers = (JArray)result["value"];
            Assert.True(customers.OfType<JObject>().All(x => x.Properties().Count() == 1 && x.Properties().All(p => p.Name == "Name")));
        }

        [Fact]
        public async Task QueryComplexPropertiesOfAnEntry()
        {
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer/?$select=LocationAddresses/City", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata=minimalmetadata"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;

            response = await client.SendAsync(request);

            Assert.NotNull(response);
            Assert.NotNull(response.Content);
            string content = await response.Content.ReadAsStringAsync();

            Assert.True(
                response.StatusCode == HttpStatusCode.OK,
                string.Format("Expected status code OK. Actual status code: {0}. Response content: {1}", response.StatusCode, content));

            JObject result = JObject.Parse(content);
            Assert.NotNull(result);
            JsonAssert.ArrayLength(10, "value", result);
            JArray customers = (JArray)result["value"];

            Action<JProperty> assertCityProperty = cityProperty =>
            {
                Assert.Equal("City", cityProperty.Name);
                Assert.Equal(JTokenType.String, cityProperty.Value.Type);
            };

            Action<JObject> assertLocationAddress = locationAddress =>
            {
                Assert.Collection(locationAddress.Properties(), assertCityProperty);
            };

            Action<JProperty> assertLocationAddressesProperty = locationAddressesProperty =>
            {
                Assert.Equal("LocationAddresses", locationAddressesProperty.Name);
                Assert.Equal(JTokenType.Array, locationAddressesProperty.Value.Type);

                var locationAddresses = (JArray)locationAddressesProperty.Value;
                int count = locationAddresses.Count;
                Assert.Collection(locationAddresses.OfType<JObject>(), Enumerable.Repeat(assertLocationAddress, count).ToArray());
            };

            foreach (JObject customer in customers.OfType<JObject>())
            {
                Assert.Collection(customer.Properties(), assertLocationAddressesProperty);
            }
        }

        [Fact]
        public async Task QueryASubsetOfThePropertiesOfAnEntryAndASubsetOfThePropertiesOfARelatedEntry()
        {
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer/?$select=Id,Name&$expand=SelectOrders($select=Id)", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;
            JObject result;
            string content;

            response = await client.SendAsync(request);

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            content = await response.Content.ReadAsStringAsync();

            result = JObject.Parse(content);
            Assert.NotNull(result);
            JsonAssert.ArrayLength(10, "value", result);
            JArray customers = (JArray)result["value"];
            Assert.True(customers.OfType<JObject>().All(x => x.Properties().Count() == 3));
            Assert.True(customers.OfType<JObject>().All(x => (int)x["Id"] == ((JArray)x["SelectOrders"]).Count));
        }

        [Fact]
        public async Task QueryASubSetOfThePropertiesPresentOnlyInADerivedEntry()
        {
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer/Default.PremiumCustomers?" +
                "$select=Id,Microsoft.Test.E2E.AspNet.OData.QueryComposition.SelectPremiumCustomer/Category", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;

            response = await client.SendAsync(request);

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            var content = await response.Content.ReadAsObject<JObject>();

            JArray customers = content["value"] as JArray;
            Assert.Equal(10, customers.Count);
            Assert.True(customers.OfType<JObject>().All(x => x.Properties().Count() == 2));
        }

        [Fact]
        public async Task QueryAnEntryAndIncludeTheRelatedEntriesForAGivenNavigationPropertyInline()
        {
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer?$select=Id&$expand=SelectOrders", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;
            JObject result;
            string content;

            response = await client.SendAsync(request);

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            content = await response.Content.ReadAsStringAsync();

            result = JObject.Parse(content);
            Assert.NotNull(result);
            JsonAssert.ArrayLength(10, "value", result);
            JArray customers = (JArray)result["value"];
            Assert.True(customers.OfType<JObject>().All(x => x.Properties().Count() == 2));
            foreach (JObject customer in (IEnumerable<JToken>)customers)
            {
                JArray orders = customer["SelectOrders"] as JArray;
                Assert.Equal((int)customer["Id"], orders.Count);
                foreach (JObject order in (IEnumerable<JToken>)orders)
                {
                    Assert.Equal(3, order.Properties().Count());
                }
            }
        }

        [Fact]
        public async Task QueryForAnEntryAnIncludeTheRelatedEntriesForASetOfNavigationProperties()
        {
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer/Default.PremiumCustomers?" +
                "$select=Id&" +
                "$expand=SelectOrders,Microsoft.Test.E2E.AspNet.OData.QueryComposition.SelectPremiumCustomer/Bonuses", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;

            response = await client.SendAsync(request);

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            var content = await response.Content.ReadAsObject<JObject>();

            JArray customers = content["value"] as JArray;
            Assert.Equal(10, customers.Count);
            Assert.True(customers.OfType<JObject>().All(x => x.Properties().Count() == 3));
            foreach (JObject customer in (IEnumerable<JToken>)customers)
            {
                JArray orders = customer["SelectOrders"] as JArray;
                Assert.Equal((int)customer["Id"], orders.Count);
                foreach (JObject order in (IEnumerable<JToken>)orders)
                {
                    Assert.Equal(3, order.Properties().Count());
                }

                JArray bonuses = customer["Bonuses"] as JArray;
                Assert.Equal((int)customer["Id"], bonuses.Count);
                foreach (JObject bonus in (IEnumerable<JToken>)bonuses)
                {
                    Assert.Equal(2, bonus.Properties().Count());
                }
            }
        }

        [Fact]
        public async Task QueryForAnEntryAndIncludeTheRelatedEntriesForAGivenNavigationPropertyPath()
        {
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer?$select=Id,SelectOrders&$expand=SelectOrders($expand=OrderDetails)", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;
            JObject result;
            string content;

            response = await client.SendAsync(request);

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            content = await response.Content.ReadAsStringAsync();

            result = JObject.Parse(content);
            Assert.NotNull(result);
            JsonAssert.ArrayLength(10, "value", result);
            JArray customers = (JArray)result["value"];
            Assert.True(customers.OfType<JObject>().All(x => x.Properties().Count() == 2));
            foreach (JObject customer in (IEnumerable<JToken>)customers)
            {
                JArray orders = customer["SelectOrders"] as JArray;
                Assert.Equal((int)customer["Id"], orders.Count);
                foreach (JObject order in (IEnumerable<JToken>)orders)
                {
                    Assert.Equal(4, order.Properties().Count());
                    JArray orderDetails = order["OrderDetails"] as JArray;
                    Assert.Equal((int)order["Id"], orderDetails.Count);
                    foreach (JObject orderDetail in (IEnumerable<JToken>)orderDetails)
                    {
                        Assert.Equal(4, orderDetail.Properties().Count());
                    }
                }
            }
        }

        [Fact]
        public async Task QueryForAnEntryAnIncludeTheRelatedEntriesForANavigationPropertyPresentOnlyInDerivedEntries()
        {
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer/Default.PremiumCustomers?" +
                "$select=Id&" +
                "$expand=Microsoft.Test.E2E.AspNet.OData.QueryComposition.SelectPremiumCustomer/Bonuses", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;

            response = await client.SendAsync(request);

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            var content = await response.Content.ReadAsObject<JObject>();

            JArray customers = content["value"] as JArray;
            Assert.Equal(10, customers.Count);
            Assert.True(customers.OfType<JObject>().All(x => x.Properties().Count() == 2));
            foreach (JObject customer in (IEnumerable<JToken>)customers)
            {
                JArray bonuses = customer["Bonuses"] as JArray;
                Assert.Equal((int)customer["Id"], bonuses.Count);
                foreach (JObject bonus in (IEnumerable<JToken>)bonuses)
                {
                    Assert.Equal(2, bonus.Properties().Count());
                }
            }
        }

        [Fact]
        public async Task NestedDollarCountInDollarExpandWorks()
        {
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer?&$expand=SelectOrders($count=true)", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.SendAsync(request);

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.NotNull(response.Content);
            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.NotNull(result);

            JsonAssert.ArrayLength(10, "value", result);
            JArray customers = (JArray)result["value"];
            Assert.True(customers.OfType<JObject>().All(x => x.Properties().Count() == SelectCustomerPropertyCount));
            foreach (JObject customer in (IEnumerable<JToken>)customers)
            {
                JArray orders = customer["SelectOrders"] as JArray;
                Assert.Equal((int)customer["Id"], orders.Count);
                Assert.Equal((int)customer["Id"], (int)customer["SelectOrders@odata.count"]);
                foreach (JObject order in (IEnumerable<JToken>)orders)
                {
                    Assert.Equal(3, order.Properties().Count());
                }
            }
        }

        [Fact]
        public async Task NestedDollarCountInDollarExpandWithNestedDollarFilterWorks()
        {
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer?&$expand=SelectOrders($filter=Id lt 1;$count=true)", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.SendAsync(request);

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.NotNull(response.Content);
            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.NotNull(result);
            Console.WriteLine(result);
            JsonAssert.ArrayLength(10, "value", result);
            JArray customers = (JArray)result["value"];
            Assert.True(customers.OfType<JObject>().All(x => x.Properties().Count() == SelectCustomerPropertyCount));
            foreach (JObject customer in (IEnumerable<JToken>)customers)
            {
                JArray orders = customer["SelectOrders"] as JArray;
                Assert.NotNull(orders);

                int customerId = (int)customer["Id"];
                if (customerId == 0)
                {
                    // the "SelectOrders" in the first customer is empty.
                    Assert.Empty(orders);
                    Assert.Equal(0, (int)customer["SelectOrders@odata.count"]);
                }
                else
                {
                    // the "SelectOrders" in other customers has only one entity, because the result is filtered by "Id" < 1.
                    Assert.Single(orders);
                    Assert.Equal(1, (int)customer["SelectOrders@odata.count"]);
                }

                foreach (JObject order in (IEnumerable<JToken>)orders)
                {
                    Assert.Equal(3, order.Properties().Count());
                }
            }
        }

        [Fact]
        public async Task NestedNestedDollarCountInDollarExpandWorks()
        {
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer?&$expand=SelectOrders($count=true;$expand=OrderDetails($count=true))", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.NotNull(response.Content);
            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.NotNull(result);

            JsonAssert.ArrayLength(10, "value", result);
            JArray customers = (JArray)result["value"];
            Assert.True(customers.OfType<JObject>().All(x => x.Properties().Count() == SelectCustomerPropertyCount));
            foreach (JObject customer in (IEnumerable<JToken>)customers)
            {
                JArray orders = customer["SelectOrders"] as JArray;
                Assert.Equal((int)customer["Id"], orders.Count);
                Assert.Equal((int)customer["Id"], (int)customer["SelectOrders@odata.count"]);
                foreach (JObject order in (IEnumerable<JToken>)orders)
                {
                    Assert.Equal(5, order.Properties().Count());
                    Assert.Equal((int)order["Id"], (int)order["OrderDetails@odata.count"]);
                }
            }
        }

        [Fact]
        public async Task NestedNestedSkipInDollarExpandWorks()
        {
            // Arrange
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer?" +
                    "$expand=SelectOrders($skip=1;$expand=OrderDetails($skip=1))", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.NotNull(response.Content);
            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.NotNull(result);

            JsonAssert.ArrayLength(10, "value", result);
            JArray customers = (JArray)result["value"];
            foreach (JObject customer in (IEnumerable<JToken>)customers)
            {
                JArray orders = customer["SelectOrders"] as JArray;
                Assert.Equal(Math.Max(0, (int)customer["Id"] - 1), orders.Count);
                foreach (JObject order in (IEnumerable<JToken>)orders)
                {
                    JArray orderdetails = order["OrderDetails"] as JArray;
                    Assert.Equal(Math.Max(0, (int)order["Id"] - 1), orderdetails.Count());
                }
            }
        }

        [Fact]
        public async Task NestedNestedTopInDollarExpandWorks()
        {
            // Arrange
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer?" +
                    "$skip=2&$expand=SelectOrders($skip=1;$top=1;$expand=OrderDetails($top=1))", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.NotNull(response.Content);
            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.NotNull(result);

            JsonAssert.ArrayLength(8, "value", result);
            JArray customers = (JArray)result["value"];
            foreach (JObject customer in (IEnumerable<JToken>)customers)
            {
                JArray orders = customer["SelectOrders"] as JArray;
                Assert.Single(orders);
                foreach (JObject order in (IEnumerable<JToken>)orders)
                {
                    JArray orderdetails = order["OrderDetails"] as JArray;
                    Assert.Single(orderdetails);
                }
            }
        }

        [Fact]
        public async Task NestedNestedOrderByInDollarExpandWorks()
        {
            // Arrange
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer?" +
                    "$expand=SelectOrders($orderby=BillingAddress/ZipCode;$expand=OrderDetails($orderby=Price desc))", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.NotNull(response.Content);
            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.NotNull(result);

            JsonAssert.ArrayLength(10, "value", result);
            JArray customers = (JArray)result["value"];
            JArray orders = customers[9]["SelectOrders"] as JArray;
            Assert.Equal(8, orders[0]["Id"]);
            JArray orderdetails = orders[0]["OrderDetails"] as JArray;
            Assert.Equal(0, orderdetails[0]["Id"]);
        }

        [Fact]
        public async Task DollarRefAfterDollarExpandWorks()
        {
            // Arrange
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer?$expand=SelectOrders/$ref", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.NotNull(response.Content);
            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.NotNull(result);

            JsonAssert.ArrayLength(10, "value", result);
            JArray customers = (JArray)result["value"];
            JArray orders = customers[9]["SelectOrders"] as JArray;

            Assert.Equal(9, orders.Count);
            for (int i = 0; i < 9; i++)
            {
                Assert.Contains("SelectOrder(" + i + ")", (string)(orders[i]["@odata.id"]));
            }
        }

        [Fact]
        public async Task DollarRefAfterDollarExpandWithNestedQueryOptionsWorks()
        {
            // Arrange
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer?$expand=SelectOrders/$ref($filter=Id eq 6)", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.NotNull(response.Content);
            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.NotNull(result);

            JsonAssert.ArrayLength(10, "value", result);
            JArray customers = (JArray)result["value"];

            JArray orders = customers[9]["SelectOrders"] as JArray;
            JToken order = Assert.Single(orders); // only one
            Assert.Contains("SelectOrder(6)", (string)order["@odata.id"]);
        }

        [Fact]
        public async Task SelectWithByteArrayAndCharArrayAndIntArrayAndDoubleArrayWorks()
        {
            // Arrange
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer(1)?$select=ByteData,CharData,IntData,DoubleData($top=1)", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.NotNull(response.Content);
            JObject result = await response.Content.ReadAsObject<JObject>();

            Assert.Contains("$metadata#SelectCustomer(ByteData,CharData,IntData,DoubleData)/$entity", (string)result["@odata.context"]);

            Assert.Equal("AQID", result["ByteData"]);
            Assert.Equal("abc;", result["CharData"]);

            JArray intData = (JArray)result["IntData"];
            Assert.Equal(3, intData.Count); // has 3 items

            JArray doubleData = (JArray)result["DoubleData"];
            Assert.Single(doubleData); // only one item
        }

        [Fact]
        public async Task DollarCountSegmentAfterDollarExpandWorks()
        {
            // Arrange
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer?$expand=SelectOrders/$count", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.NotNull(response.Content);
            JObject json = await response.Content.ReadAsObject<JObject>();
            Assert.NotNull(json);

            Assert.Equal("10", (string)json["SelectOrders@odata.count"]);
        }

        [Fact]
        public async Task DollarCountSegmentAfterDollarExpandWithNestedFilterWorks()
        {
            // Arrange
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer?$expand=SelectOrders/$count($filter=Id gt 5)", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.NotNull(response.Content);
            JObject json = await response.Content.ReadAsObject<JObject>();
            Assert.NotNull(json);

            Assert.Equal("4", (string)json["SelectOrders@odata.count"]);
        }
    }

    public class SelectCustomerController : TestODataController
    {
        public IList<SelectCustomer> Customers { get; set; }

        public SelectCustomerController()
        {
            Customers = Enumerable.Range(0, 10).Select(i => new SelectCustomer
            {
                Id = i,
                Name = string.Format("Customer{0}", i),
                ByteData = new byte[] { 1, 2, 3 },
                IntData = new [] { 1, 2, 3 },
                DoubleData = new double[] { 1.1, 2.2, 3.983 },
                CharData = new char[] { 'a', 'b', 'c', ';' },
                SelectOrders = Enumerable.Range(0, i).Select(j => new SelectOrder
                {
                    Id = j,
                    Date = DateTime.Now.Subtract(TimeSpan.FromDays(j)),
                    BillingAddress = new SelectAddress
                    {
                        FirstLine = string.Format("First line {0}", j),
                        SecondLine = string.Format("Second line {0}", j),
                        ZipCode = j * -100,
                        City = string.Format("City {0}", j),
                        State = string.Format("State {0}", j),
                        Country = string.Format("CountryOrRegion {0}", j),
                    },
                    OrderDetails = Enumerable.Range(0, j).Select(k => new SelectOrderDetail
                    {
                        Id = k,
                        Ammount = k,
                        Name = string.Format("Name {0}", k),
                        Price = k * -1000
                    }).ToList().AsQueryable()
                }).ToList(),
                LocationAddresses = Enumerable.Range(0, i).Select(j => new SelectAddress
                {
                    FirstLine = string.Format("First line {0}", j),
                    SecondLine = string.Format("Second line {0}", j),
                    ZipCode = j * -100,
                    City = string.Format("City {0}", j),
                    State = string.Format("State {0}", j),
                    Country = string.Format("CountryOrRegion {0}", j),
                }).ToList()
            }).ToList();
        }

        [EnableQuery]
        public IQueryable<SelectCustomer> Get()
        {
            return Customers.AsQueryable();
        }

        [EnableQuery]
        public SelectCustomer Get(int key)
        {
            return Customers.FirstOrDefault(c => c.Id == key);
        }

        [EnableQuery]
        public IQueryable<SelectCustomer> PremiumCustomers()
        {
            return Enumerable.Range(0, 10).Select(i => new SelectPremiumCustomer
            {
                Id = i,
                Name = string.Format("Customer{0}", i),
                Category = string.Format("Category{0}", i),
                Bonuses = Enumerable.Range(0, i).Select(j => new SelectBonus
                {
                    Id = j,
                    Ammount = j * 1000
                }).ToList(),
                SelectOrders = Enumerable.Range(0, i).Select(j => new SelectOrder
                {
                    Id = j,
                    Date = DateTime.Now.Subtract(TimeSpan.FromDays(j)),
                    BillingAddress = new SelectAddress
                    {
                        FirstLine = string.Format("First line {0}", j),
                        SecondLine = string.Format("Second line {0}", j),
                        ZipCode = j * 100,
                        City = string.Format("City {0}", j),
                        State = string.Format("State {0}", j),
                        Country = string.Format("CountryOrRegion {0}", j),
                    },
                    OrderDetails = new List<SelectOrderDetail>().AsQueryable()
                }).ToList(),

            }).AsQueryable();
        }

        public double CreditRating(int key)
        {
            return 0;
        }
    }

    public class SelectCustomer
    {
        public int Id { get; set; }
        
        public string Name { get; set; }

        public byte[] ByteData { get; set; }
        
        public int[] IntData { get; set; }
        
        public double[] DoubleData { get; set; }

        public char[] CharData { get; set; }

        public virtual IList<SelectOrder> SelectOrders { get; set; }

        public virtual IList<SelectAddress> LocationAddresses { get; set; }
    }

    public class SelectPremiumCustomer : SelectCustomer
    {
        public string Category { get; set; }
        public IList<SelectBonus> Bonuses { get; set; }
    }

    public class SelectBonus
    {
        public int Id { get; set; }
        public double Ammount { get; set; }
    }

    public class SelectOrder
    {
        public int Id { get; set; }
        public DateTimeOffset Date { get; set; }
        public SelectAddress BillingAddress { get; set; }
        public virtual IQueryable<SelectOrderDetail> OrderDetails { get; set; }
    }

    public class SelectAddress
    {
        public string FirstLine { get; set; }
        public string SecondLine { get; set; }
        public int ZipCode { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
    }

    public class SelectOrderDetail
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Ammount { get; set; }
        public double Price { get; set; }
    }
}
