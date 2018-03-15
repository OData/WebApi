﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
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
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.AddControllers(
                    typeof(SelectCustomerController),
                    typeof(EFSelectCustomersController),
                    typeof(EFSelectOrdersController),
                    typeof(EFWideCustomersController));
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

            builder.EntitySet<EFSelectCustomer>("EFSelectCustomers");
            builder.EntitySet<EFSelectOrder>("EFSelectOrders");
            builder.EntitySet<SelectOrderDetail>("SelectOrderDetail");
            builder.EntityType<SelectPremiumCustomer>();
            builder.EntitySet<SelectOrder>("SelectOrder");
            builder.EntitySet<SelectBonus>("SelectBonus");
            builder.EntitySet<EFWideCustomer>("EFWideCustomers");
            builder.Action("ResetDataSource-Customer");
            builder.Action("ResetDataSource-WideCustomer");
            builder.Action("ResetDataSource-Order");

            IEdmModel model = builder.GetEdmModel();
            for (int idx = 1; idx <= 5; idx++)
            {
                IEdmSchemaType nestedType = model.FindDeclaredType("Microsoft.Test.E2E.AspNet.OData.QueryComposition.CustomProperties" + idx);
                model.SetAnnotationValue(nestedType, new Microsoft.AspNet.OData.Query.ModelBoundQuerySettings()
                {
                    DefaultSelectType = SelectExpandType.Automatic
                });
            }

            return model;
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
        public async Task QueryForAnEntryWithExpandNavigationPropertyExceedPageSize()
        {
            // Arrange
            await RestoreData("-Customer");
            string queryUrl = string.Format("{0}/selectexpand/EFSelectCustomers?$expand=SelectOrders", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            
            var responseObject = JObject.Parse(await response.Content.ReadAsStringAsync());
            var result = responseObject["value"] as JArray;
            var expandProp = result[0]["SelectOrders"] as JArray;
            Assert.Equal(2, expandProp.Count);
            Assert.Equal(1, expandProp[0]["Id"]);
            Assert.Equal(2, expandProp[1]["Id"]);
        }

        [Fact]
        public async Task QueryForAnEntryWithExpandSingleNavigationPropertyFilterWorks()
        {
            await RestoreData("-Order");
            HttpClient client = new HttpClient();
            HttpResponseMessage response;

            // Arrange
            Func<string, Task<JArray>> TestBody = async (url) =>
            {
                string queryUrl = url;
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
                request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));

                // Act
                response = await client.SendAsync(request);

                // Assert
                Assert.NotNull(response);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.NotNull(response.Content);

                var responseObject = JObject.Parse(await response.Content.ReadAsStringAsync());
                return responseObject["value"] as JArray;
            };

            var result = await TestBody(string.Format("{0}/selectexpand/EFSelectOrders?$expand=SelectCustomer($filter=Id ne 1)", BaseAddress));
            Assert.False(result[0]["SelectCustomer"].HasValues);

            result = await TestBody(string.Format("{0}/selectexpand/EFSelectOrders?$expand=SelectCustomer($filter=Id eq 1)", BaseAddress));
            Assert.Equal(1, (int)result[0]["SelectCustomer"]["Id"]);
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
            Assert.True(customers.OfType<JObject>().All(x => x.Properties().Count() == 4));
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
            Assert.True(customers.OfType<JObject>().All(x => x.Properties().Count() == 4));
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
            Assert.True(customers.OfType<JObject>().All(x => x.Properties().Count() == 4));
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
        public async Task NestedTopSkipOrderByInDollarExpandWorksWithEF()
        {
            // Arrange
            await RestoreData("-Customer");
            string queryUrl = string.Format("{0}/selectexpand/EFSelectCustomers?$expand=SelectOrders($orderby=Id desc;$skip=1;$top=1)", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            var responseObject = JObject.Parse(await response.Content.ReadAsStringAsync());
            var result = responseObject["value"] as JArray;
            var expandProp = result[0]["SelectOrders"] as JArray;
            Assert.Single(expandProp);
            Assert.Equal(2, expandProp[0]["Id"]);
        }

        [Fact]
        public async Task QueryForLongSelectList()
        {
            // Arrange
            await RestoreData("-WideCustomer");
            // Create long $slect/$expand Custom1-4 will be autoexpanded to avoid maxUrl error
            string queryUrl = string.Format("{0}/selectexpand/EFWideCustomers?$select=Id&$expand=Custom1,Custom2,Custom3,Custom4,Custom5($select="
                + string.Join(",", Enumerable.Range(1601, 399).Select(i => string.Format("Prop{0:0000}", i))) + ")",
                BaseAddress);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpClient client = new HttpClient() { Timeout = TimeSpan.FromMinutes(10) };
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            
            JObject responseObject = JObject.Parse(await response.Content.ReadAsStringAsync());
            JArray result = responseObject["value"] as JArray;
            Assert.Single(result);
            Assert.Equal("Prop0001", result[0]["Custom1"]["Prop0001"]);
            Assert.Equal("Prop0099", result[0]["Custom1"]["Prop0099"]);
            Assert.Equal("Prop0199", result[0]["Custom1"]["Prop0199"]);
            Assert.Equal("Prop0298", result[0]["Custom1"]["Prop0298"]);
            Assert.Equal("Prop0798", result[0]["Custom2"]["Prop0798"]);
            Assert.Equal("Prop1198", result[0]["Custom3"]["Prop1198"]);
            Assert.Equal("Prop1598", result[0]["Custom4"]["Prop1598"]);
            Assert.Equal("Prop1998", result[0]["Custom5"]["Prop1998"]);
            Assert.Null(result[0]["Custom5"]["Prop2000"]);
        }

        private async Task RestoreData(string suffix)
        {
            string requestUri = BaseAddress + string.Format("/selectexpand/ResetDataSource{0}", suffix);
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();
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
                }).ToList()
            }).ToList();
        }

        [EnableQuery]
        public IQueryable<SelectCustomer> Get()
        {
            return Customers.AsQueryable();
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

    public class EFSelectCustomersController : TestODataController
    {
        private readonly SampleContext _db = new SampleContext();

        [EnableQuery(PageSize = 2)]
        public ITestActionResult Get()
        {
            return Ok(_db.Customers);
        }

        [HttpGet]
        [ODataRoute("ResetDataSource-Customer")]
        public ITestActionResult ResetDataSource()
        {
            if (!_db.Customers.Any())
            {
                Generate(_db);
            }

            return Ok();
        }

        public static void Generate(SampleContext db)
        {
            var customer = new EFSelectCustomer
            {
                Id = 1,
                SelectOrders = new List<EFSelectOrder>
                {
                    new EFSelectOrder
                    {
                        Id = 3,
                    },
                    new EFSelectOrder
                    {
                        Id = 1,
                    },
                    new EFSelectOrder
                    {
                        Id = 2,
                    }
                }
            };

            db.Customers.Add(customer);
            db.SaveChanges();
        }
    }

    public class EFWideCustomersController : TestODataController
    {
        private readonly SampleContext _db = new SampleContext();

        [EnableQuery(PageSize =2)]
        public IQueryable<EFWideCustomer> Get()
        {
            return (_db.WideCustomers as IQueryable<IEFCastTest>).Cast<EFWideCustomer>();
        }

        [HttpGet]
        [ODataRoute("ResetDataSource-WideCustomer")]
        public ITestActionResult ResetDataSource()
        {
            if (!_db.WideCustomers.Any())
            {
                Generate();
            }

            return Ok();
        }

        public void Generate()
        {
            var wideCustomer = new EFWideCustomer
            {
                Id = 1,
                Custom1 = new CustomProperties1
                {
                    Prop0001 = "Prop0001",
                    Prop0099 = "Prop0099",
                    Prop0199 = "Prop0199",
                    Prop0298 = "Prop0298",
                    Prop0299 = "Prop0299",
                },
                Custom2 = new CustomProperties2
                {
                    Prop0798 = "Prop0798",
                },
                Custom3 = new CustomProperties3
                {
                    Prop1198 = "Prop1198",
                },
                Custom4 = new CustomProperties4
                {
                    Prop1598 = "Prop1598",
                    Prop1600 = "Prop1600",
                },
                Custom5 = new CustomProperties5
                {
                    Prop1998 = "Prop1998",
                    Prop2000 = "Prop2000",
                },
            };

            _db.WideCustomers.Add(wideCustomer);
            _db.SaveChanges();
        }
    }

    public class EFSelectOrdersController : TestODataController
    {
        private readonly SampleContext _db = new SampleContext();

        [EnableQuery(HandleReferenceNavigationPropertyExpandFilter = true)]
        public ITestActionResult Get()
        {
            return Ok(_db.Orders);
        }

        [HttpGet]
        [ODataRoute("ResetDataSource-Order")]
        public ITestActionResult ResetDataSource()
        {
            if (!_db.Orders.Any())
            {
                EFSelectCustomersController.Generate(_db);
            }

            return Ok();
        }
    }

    public class SampleContext : DbContext
    {
        public static string ConnectionString = @"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=SelectExpandTest2";

        public SampleContext()
            : base(ConnectionString)
        {
        }

        public DbSet<EFSelectCustomer> Customers { get; set; }

        public DbSet<SelectCustomer> SelectCustomers { get; set; }

        public DbSet<EFSelectOrder> Orders { get; set; }

        public DbSet<EFWideCustomer> WideCustomers { get; set; }
    }

    public class EFSelectCustomer
    {
        public int Id { get; set; }
        public virtual IList<EFSelectOrder> SelectOrders { get; set; }
    }

    public class EFSelectOrder
    {
        public int Id { get; set; }
        public virtual EFSelectCustomer SelectCustomer { get; set; }
    }

    public class SelectCustomer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual IList<SelectOrder> SelectOrders { get; set; }
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
