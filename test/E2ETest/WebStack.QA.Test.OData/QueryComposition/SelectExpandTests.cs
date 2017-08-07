using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Dispatcher;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using Xunit;

namespace WebStack.QA.Test.OData.QueryComposition
{
    public class SelectExpandTests : ODataTestBase
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.Services.Replace(
                  typeof(IAssembliesResolver),
                  new TestAssemblyResolver(
                      typeof(SelectCustomerController),
                      typeof(EFSelectCustomersController),
                      typeof(EFWideCustomersController),
                      typeof(EFSelectOrdersController)));
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null).Select();
            configuration.MapODataServiceRoute("selectexpand", "selectexpand", GetEdmModel(configuration));
        }

        private static IEdmModel GetEdmModel(HttpConfiguration configuration)
        {
            var builder = new ODataConventionModelBuilder(configuration);

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
            builder.Action("ResetDataSource");
            builder.Action("ResetDataSource-Order");

            IEdmModel model = builder.GetEdmModel();
            return model;
        }

        [Fact]
        public void QueryJustThePropertiesOfTheEntriesOnAFeed()
        {
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer/?$select=*", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;
            JObject result;
            string content;

            response = client.SendAsync(request).Result;

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            content = response.Content.ReadAsStringAsync().Result;

            result = JObject.Parse(content);
            Assert.NotNull(result);
            JsonAssert.ArrayLength(10, "value", result);
            JArray customers = (JArray)result["value"];
            Assert.True(customers.OfType<JObject>().All(x => x.Properties().All(p => p.Name != "#Container.CreditRating")));
        }

        [Fact]
        public void QueryJustTheActionsOfTheEntriesOnAFeed()
        {
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer/?$select=Default.*", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;
            JObject result;
            string content;

            response = client.SendAsync(request).Result;

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            content = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine(content);

            result = JObject.Parse(content);
            Assert.NotNull(result);
            JsonAssert.ArrayLength(10, "value", result);
            JArray customers = (JArray)result["value"];
            Assert.True(customers.OfType<JObject>().All(x => x.Properties().Any(p => p.Name == "#Default.CreditRating")));
        }

        [Fact]
        public void QueryASubsetOfThePropertiesOfAnEntry()
        {
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer/?$select=Name", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata=minimalmetadata"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;
            JObject result;
            string content;

            response = client.SendAsync(request).Result;

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            content = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine(content);

            result = JObject.Parse(content);
            Assert.NotNull(result);
            JsonAssert.ArrayLength(10, "value", result);
            JArray customers = (JArray)result["value"];
            Assert.True(customers.OfType<JObject>().All(x => x.Properties().Count() == 1 && x.Properties().All(p => p.Name == "Name")));
        }

        [Fact]
        public void QueryASubsetOfThePropertiesOfAnEntryAndASubsetOfThePropertiesOfARelatedEntry()
        {
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer/?$select=Id,Name&$expand=SelectOrders($select=Id)", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;
            JObject result;
            string content;

            response = client.SendAsync(request).Result;
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            content = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine(content);

            result = JObject.Parse(content);
            Assert.NotNull(result);
            JsonAssert.ArrayLength(10, "value", result);
            JArray customers = (JArray)result["value"];
            Assert.True(customers.OfType<JObject>().All(x => x.Properties().Count() == 3));
            Assert.True(customers.OfType<JObject>().All(x => (int)x["Id"] == ((JArray)x["SelectOrders"]).Count));
        }

        [Fact]
        public void QueryASubSetOfThePropertiesPresentOnlyInADerivedEntry()
        {
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer/Default.PremiumCustomers?" +
                "$select=Id,WebStack.QA.Test.OData.QueryComposition.SelectPremiumCustomer/Category", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;

            response = client.SendAsync(request).Result;

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            var content = response.Content.ReadAsAsync<JObject>().Result;

            JArray customers = content["value"] as JArray;
            Assert.Equal(10, customers.Count);
            Assert.True(customers.OfType<JObject>().All(x => x.Properties().Count() == 2));
        }

        [Fact]
        public void QueryAnEntryAndIncludeTheRelatedEntriesForAGivenNavigationPropertyInline()
        {
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer?$select=Id&$expand=SelectOrders", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;
            JObject result;
            string content;

            response = client.SendAsync(request).Result;

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            content = response.Content.ReadAsStringAsync().Result;

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
        public void QueryForAnEntryAnIncludeTheRelatedEntriesForASetOfNavigationProperties()
        {
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer/Default.PremiumCustomers?" +
                "$select=Id&" +
                "$expand=SelectOrders,WebStack.QA.Test.OData.QueryComposition.SelectPremiumCustomer/Bonuses", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;

            response = client.SendAsync(request).Result;

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            var content = response.Content.ReadAsAsync<JObject>().Result;

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
        public void QueryForAnEntryAndIncludeTheRelatedEntriesForAGivenNavigationPropertyPath()
        {
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer?$select=Id,SelectOrders&$expand=SelectOrders($expand=OrderDetails)", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;
            JObject result;
            string content;

            response = client.SendAsync(request).Result;

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            content = response.Content.ReadAsStringAsync().Result;

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
        public void QueryForAnEntryAnIncludeTheRelatedEntriesForANavigationPropertyPresentOnlyInDerivedEntries()
        {
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer/Default.PremiumCustomers?" +
                "$select=Id&" +
                "$expand=WebStack.QA.Test.OData.QueryComposition.SelectPremiumCustomer/Bonuses", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;

            response = client.SendAsync(request).Result;

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            var content = response.Content.ReadAsAsync<JObject>().Result;

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
        public void QueryForAnEntryWithExpandNavigationPropertyExceedPageSize()
        {
            // Arrange
            RestoreData();
            string queryUrl = string.Format("{0}/selectexpand/EFSelectCustomers?$expand=SelectOrders", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;

            // Act
            response = client.SendAsync(request).Result;

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            var responseObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            var result = responseObject["value"] as JArray;
            var expandProp = result[0]["SelectOrders"] as JArray;
            Assert.Equal(expandProp.Count, 2);
            Assert.Equal(expandProp[0]["Id"], 1);
            Assert.Equal(expandProp[1]["Id"], 2);
        }

        [Fact]
        public void QueryForAnEntryWithExpandSingleNavigationPropertyFilterWorks()
        {
            RestoreData("-Order");
            HttpClient client = new HttpClient();
            HttpResponseMessage response;

            // Arrange
            Func<string, JArray> TestBody = (url) =>
            {
                string queryUrl = url;
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
                request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
                
                // Act
                response = client.SendAsync(request).Result;

                // Assert
                Assert.NotNull(response);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.NotNull(response.Content);

                var responseObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                return responseObject["value"] as JArray;
            };

            var result = TestBody(string.Format("{0}/selectexpand/EFSelectOrders?$expand=SelectCustomer($filter=Id ne 1)", BaseAddress));
            Assert.False(result[0]["SelectCustomer"].HasValues);

            result = TestBody(string.Format("{0}/selectexpand/EFSelectOrders?$expand=SelectCustomer($filter=Id eq 1)", BaseAddress));
            Assert.Equal(1, (int)result[0]["SelectCustomer"]["Id"]);
        }

        [Fact]
        public void NestedDollarCountInDollarExpandWorks()
        {
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer?&$expand=SelectOrders($count=true)", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response = client.SendAsync(request).Result;

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.NotNull(response.Content);
            JObject result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
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
        public void NestedDollarCountInDollarExpandWithNestedDollarFilterWorks()
        {
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer?&$expand=SelectOrders($filter=Id lt 1;$count=true)", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response = client.SendAsync(request).Result;

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.NotNull(response.Content);
            JObject result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
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
                    Assert.Equal(0, orders.Count);
                    Assert.Equal(0, (int)customer["SelectOrders@odata.count"]);
                }
                else
                {
                    // the "SelectOrders" in other customers has only one entity, because the result is filtered by "Id" < 1.
                    Assert.Equal(1, orders.Count);
                    Assert.Equal(1, (int)customer["SelectOrders@odata.count"]);
                }

                foreach (JObject order in (IEnumerable<JToken>)orders)
                {
                    Assert.Equal(3, order.Properties().Count());
                }
            }
        }

        [Fact]
        public void NestedNestedDollarCountInDollarExpandWorks()
        {
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer?&$expand=SelectOrders($count=true;$expand=OrderDetails($count=true))", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.NotNull(response.Content);
            JObject result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
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
        public void NestedNestedSkipInDollarExpandWorks()
        {
            // Arrange
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer?" +
                    "$expand=SelectOrders($skip=1;$expand=OrderDetails($skip=1))", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.NotNull(response.Content);
            JObject result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
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
        public void NestedNestedTopInDollarExpandWorks()
        {
            // Arrange
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer?" +
                    "$skip=2&$expand=SelectOrders($skip=1;$top=1;$expand=OrderDetails($top=1))", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.NotNull(response.Content);
            JObject result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.NotNull(result);

            JsonAssert.ArrayLength(8, "value", result);
            JArray customers = (JArray)result["value"];
            foreach (JObject customer in (IEnumerable<JToken>)customers)
            {
                JArray orders = customer["SelectOrders"] as JArray;
                Assert.Equal(1, orders.Count);
                foreach (JObject order in (IEnumerable<JToken>)orders)
                {
                    JArray orderdetails = order["OrderDetails"] as JArray;
                    Assert.Equal(1, orderdetails.Count());
                }
            }
        }

        [Fact]
        public void NestedNestedOrderByInDollarExpandWorks()
        {
            // Arrange
            string queryUrl = string.Format("{0}/selectexpand/SelectCustomer?" +
                    "$expand=SelectOrders($orderby=BillingAddress/ZipCode;$expand=OrderDetails($orderby=Price desc))", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.NotNull(response.Content);
            JObject result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.NotNull(result);

            JsonAssert.ArrayLength(10, "value", result);
            JArray customers = (JArray)result["value"];
            JArray orders = customers[9]["SelectOrders"] as JArray;
            Assert.Equal(8, orders[0]["Id"]);
            JArray orderdetails = orders[0]["OrderDetails"] as JArray;
            Assert.Equal(0, orderdetails[0]["Id"]);
        }

        [Fact]
        public void NestedTopSkipOrderByInDollarExpandWorksWithEF()
        {
            // Arrange
            RestoreData();
            string queryUrl = string.Format("{0}/selectexpand/EFSelectCustomers?$expand=SelectOrders($orderby=Id desc;$skip=1;$top=1)", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;

            // Act
            response = client.SendAsync(request).Result;

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            var responseObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            var result = responseObject["value"] as JArray;
            var expandProp = result[0]["SelectOrders"] as JArray;
            Assert.Equal(expandProp.Count, 1);
            Assert.Equal(expandProp[0]["Id"], 2);
        }

        [Fact]
        public void QueryForLongSelectList()
        {
            // Arrange
            RestoreData();
            string queryUrl = string.Format("{0}/selectexpand/EFWideCustomers?" +
                "$select=Id,"+ string.Join(",", Enumerable.Range(1,298).Select(i => string.Format("Prop{0:000}", i))), BaseAddress);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;

            // Act
            response = client.SendAsync(request).Result;

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            var responseObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            var result = responseObject["value"] as JArray;
            Assert.Equal(result.Count, 1);
            Assert.Equal(result[0]["Prop001"], "Prop001");
            Assert.Equal(result[0]["Prop099"], "Prop099");
            Assert.Equal(result[0]["Prop199"], "Prop199");
            Assert.Equal(result[0]["Prop298"], "Prop298");
            Assert.Null(result[0]["Prop299"]);
        }

        private void RestoreData(string suffix = null)
        {
            string requestUri = BaseAddress + $"/selectexpand/ResetDataSource{suffix}";
            HttpClient client = new HttpClient();
            HttpResponseMessage response = client.GetAsync(requestUri).Result;
            response.EnsureSuccessStatusCode();
        }
    }

    public class SelectCustomerController : ODataController
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
                        Country = string.Format("Country {0}", j),
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
                        Country = string.Format("Country {0}", j),
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

    public class EFSelectCustomersController : ODataController
    {
        private readonly SampleContext _db = new SampleContext();

        [EnableQuery(PageSize = 2)]
        public IHttpActionResult Get()
        {
            return Ok(_db.Customers);
        }

        [HttpGet]
        [ODataRoute("ResetDataSource")]
        public IHttpActionResult ResetDataSource()
        {
            if (_db.Database.Exists())
            {
                _db.Database.Delete();
                _db.Database.Create();
            }

            Generate();
            return Ok();
        }

        public void Generate()
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
            _db.Customers.Add(customer);

            var wideCustomer = new EFWideCustomer
            {
                Id = 1,
                Prop001 = "Prop001",
                Prop099 = "Prop099",
                Prop199 = "Prop199",
                Prop298 = "Prop298",
                Prop299 = "Prop299",
            };
            _db.WideCustomers.Add(wideCustomer);
            _db.SaveChanges();
        }
    }

    public class EFWideCustomersController : ODataController
    {
        private readonly SampleContext _db = new SampleContext();

        [EnableQuery]
        public IHttpActionResult Get()
        {
            return Ok(_db.WideCustomers);
        }
    }

    public class EFSelectOrdersController : ODataController
    {
        private readonly SampleContext _db = new SampleContext();

        [EnableQuery(HandleReferenceNavigationPropertyExpandFilter = true)]
        public IHttpActionResult Get()
        {
            return Ok(_db.Orders);
        }

        [HttpGet]
        [ODataRoute("ResetDataSource-Order")]
        public IHttpActionResult ResetDataSource()
        {
            if (_db.Database.Exists())
            {
                _db.Database.Delete();
                _db.Database.Create();
            }

            Generate();
            return Ok();
        }

        public void Generate()
        {
            var order = new EFSelectOrder
            {
                Id = 1,
                SelectCustomer = new EFSelectCustomer
                {
                    Id = 1
                }
            };
            _db.Orders.Add(order);
            _db.SaveChanges();
        }
    }

    public class SampleContext : DbContext
    {
        public static string ConnectionString = @"Data Source=(LocalDb)\v11.0;Integrated Security=True;Initial Catalog=SelectExpandTest";

        public SampleContext()
            : base(ConnectionString)
        {
        }

        public DbSet<EFSelectCustomer> Customers { get; set; }

        public DbSet<SelectCustomer> SelectCustomers { get; set; }

        public DbSet<EFWideCustomer> WideCustomers { get; set; }

        public DbSet<EFSelectOrder> Orders { get; set; }
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

    public class EFWideCustomer
    {
        public int Id { get; set; }
        public string Prop001 { get; set; }
        public string Prop002 { get; set; }
        public string Prop003 { get; set; }
        public string Prop004 { get; set; }
        public string Prop005 { get; set; }
        public string Prop006 { get; set; }
        public string Prop007 { get; set; }
        public string Prop008 { get; set; }
        public string Prop009 { get; set; }
        public string Prop010 { get; set; }
        public string Prop011 { get; set; }
        public string Prop012 { get; set; }
        public string Prop013 { get; set; }
        public string Prop014 { get; set; }
        public string Prop015 { get; set; }
        public string Prop016 { get; set; }
        public string Prop017 { get; set; }
        public string Prop018 { get; set; }
        public string Prop019 { get; set; }
        public string Prop020 { get; set; }
        public string Prop021 { get; set; }
        public string Prop022 { get; set; }
        public string Prop023 { get; set; }
        public string Prop024 { get; set; }
        public string Prop025 { get; set; }
        public string Prop026 { get; set; }
        public string Prop027 { get; set; }
        public string Prop028 { get; set; }
        public string Prop029 { get; set; }

        public string Prop030 { get; set; }
        public string Prop031 { get; set; }
        public string Prop032 { get; set; }
        public string Prop033 { get; set; }
        public string Prop034 { get; set; }
        public string Prop035 { get; set; }
        public string Prop036 { get; set; }
        public string Prop037 { get; set; }
        public string Prop038 { get; set; }
        public string Prop039 { get; set; }

        public string Prop040 { get; set; }
        public string Prop041 { get; set; }
        public string Prop042 { get; set; }
        public string Prop043 { get; set; }
        public string Prop044 { get; set; }
        public string Prop045 { get; set; }
        public string Prop046 { get; set; }
        public string Prop047 { get; set; }
        public string Prop048 { get; set; }
        public string Prop049 { get; set; }

        public string Prop050 { get; set; }
        public string Prop051 { get; set; }
        public string Prop052 { get; set; }
        public string Prop053 { get; set; }
        public string Prop054 { get; set; }
        public string Prop055 { get; set; }
        public string Prop056 { get; set; }
        public string Prop057 { get; set; }
        public string Prop058 { get; set; }
        public string Prop059 { get; set; }

        public string Prop060 { get; set; }
        public string Prop061 { get; set; }
        public string Prop062 { get; set; }
        public string Prop063 { get; set; }
        public string Prop064 { get; set; }
        public string Prop065 { get; set; }
        public string Prop066 { get; set; }
        public string Prop067 { get; set; }
        public string Prop068 { get; set; }
        public string Prop069 { get; set; }

        public string Prop070 { get; set; }
        public string Prop071 { get; set; }
        public string Prop072 { get; set; }
        public string Prop073 { get; set; }
        public string Prop074 { get; set; }
        public string Prop075 { get; set; }
        public string Prop076 { get; set; }
        public string Prop077 { get; set; }
        public string Prop078 { get; set; }
        public string Prop079 { get; set; }

        public string Prop080 { get; set; }
        public string Prop081 { get; set; }
        public string Prop082 { get; set; }
        public string Prop083 { get; set; }
        public string Prop084 { get; set; }
        public string Prop085 { get; set; }
        public string Prop086 { get; set; }
        public string Prop087 { get; set; }
        public string Prop088 { get; set; }
        public string Prop089 { get; set; }

        public string Prop090 { get; set; }
        public string Prop091 { get; set; }
        public string Prop092 { get; set; }
        public string Prop093 { get; set; }
        public string Prop094 { get; set; }
        public string Prop095 { get; set; }
        public string Prop096 { get; set; }
        public string Prop097 { get; set; }
        public string Prop098 { get; set; }
        public string Prop099 { get; set; }

        public string Prop100 { get; set; }
        public string Prop101 { get; set; }
        public string Prop102 { get; set; }
        public string Prop103 { get; set; }
        public string Prop104 { get; set; }
        public string Prop105 { get; set; }
        public string Prop106 { get; set; }
        public string Prop107 { get; set; }
        public string Prop108 { get; set; }
        public string Prop109 { get; set; }
        public string Prop110 { get; set; }
        public string Prop111 { get; set; }
        public string Prop112 { get; set; }
        public string Prop113 { get; set; }
        public string Prop114 { get; set; }
        public string Prop115 { get; set; }
        public string Prop116 { get; set; }
        public string Prop117 { get; set; }
        public string Prop118 { get; set; }
        public string Prop119 { get; set; }
        public string Prop120 { get; set; }
        public string Prop121 { get; set; }
        public string Prop122 { get; set; }
        public string Prop123 { get; set; }
        public string Prop124 { get; set; }
        public string Prop125 { get; set; }
        public string Prop126 { get; set; }
        public string Prop127 { get; set; }
        public string Prop128 { get; set; }
        public string Prop129 { get; set; }

        public string Prop130 { get; set; }
        public string Prop131 { get; set; }
        public string Prop132 { get; set; }
        public string Prop133 { get; set; }
        public string Prop134 { get; set; }
        public string Prop135 { get; set; }
        public string Prop136 { get; set; }
        public string Prop137 { get; set; }
        public string Prop138 { get; set; }
        public string Prop139 { get; set; }

        public string Prop140 { get; set; }
        public string Prop141 { get; set; }
        public string Prop142 { get; set; }
        public string Prop143 { get; set; }
        public string Prop144 { get; set; }
        public string Prop145 { get; set; }
        public string Prop146 { get; set; }
        public string Prop147 { get; set; }
        public string Prop148 { get; set; }
        public string Prop149 { get; set; }

        public string Prop150 { get; set; }
        public string Prop151 { get; set; }
        public string Prop152 { get; set; }
        public string Prop153 { get; set; }
        public string Prop154 { get; set; }
        public string Prop155 { get; set; }
        public string Prop156 { get; set; }
        public string Prop157 { get; set; }
        public string Prop158 { get; set; }
        public string Prop159 { get; set; }

        public string Prop160 { get; set; }
        public string Prop161 { get; set; }
        public string Prop162 { get; set; }
        public string Prop163 { get; set; }
        public string Prop164 { get; set; }
        public string Prop165 { get; set; }
        public string Prop166 { get; set; }
        public string Prop167 { get; set; }
        public string Prop168 { get; set; }
        public string Prop169 { get; set; }

        public string Prop170 { get; set; }
        public string Prop171 { get; set; }
        public string Prop172 { get; set; }
        public string Prop173 { get; set; }
        public string Prop174 { get; set; }
        public string Prop175 { get; set; }
        public string Prop176 { get; set; }
        public string Prop177 { get; set; }
        public string Prop178 { get; set; }
        public string Prop179 { get; set; }

        public string Prop180 { get; set; }
        public string Prop181 { get; set; }
        public string Prop182 { get; set; }
        public string Prop183 { get; set; }
        public string Prop184 { get; set; }
        public string Prop185 { get; set; }
        public string Prop186 { get; set; }
        public string Prop187 { get; set; }
        public string Prop188 { get; set; }
        public string Prop189 { get; set; }

        public string Prop190 { get; set; }
        public string Prop191 { get; set; }
        public string Prop192 { get; set; }
        public string Prop193 { get; set; }
        public string Prop194 { get; set; }
        public string Prop195 { get; set; }
        public string Prop196 { get; set; }
        public string Prop197 { get; set; }
        public string Prop198 { get; set; }
        public string Prop199 { get; set; }


        public string Prop200 { get; set; }
        public string Prop201 { get; set; }
        public string Prop202 { get; set; }
        public string Prop203 { get; set; }
        public string Prop204 { get; set; }
        public string Prop205 { get; set; }
        public string Prop206 { get; set; }
        public string Prop207 { get; set; }
        public string Prop208 { get; set; }
        public string Prop209 { get; set; }
        public string Prop210 { get; set; }
        public string Prop211 { get; set; }
        public string Prop212 { get; set; }
        public string Prop213 { get; set; }
        public string Prop214 { get; set; }
        public string Prop215 { get; set; }
        public string Prop216 { get; set; }
        public string Prop217 { get; set; }
        public string Prop218 { get; set; }
        public string Prop219 { get; set; }
        public string Prop220 { get; set; }
        public string Prop221 { get; set; }
        public string Prop222 { get; set; }
        public string Prop223 { get; set; }
        public string Prop224 { get; set; }
        public string Prop225 { get; set; }
        public string Prop226 { get; set; }
        public string Prop227 { get; set; }
        public string Prop228 { get; set; }
        public string Prop229 { get; set; }

        public string Prop230 { get; set; }
        public string Prop231 { get; set; }
        public string Prop232 { get; set; }
        public string Prop233 { get; set; }
        public string Prop234 { get; set; }
        public string Prop235 { get; set; }
        public string Prop236 { get; set; }
        public string Prop237 { get; set; }
        public string Prop238 { get; set; }
        public string Prop239 { get; set; }

        public string Prop240 { get; set; }
        public string Prop241 { get; set; }
        public string Prop242 { get; set; }
        public string Prop243 { get; set; }
        public string Prop244 { get; set; }
        public string Prop245 { get; set; }
        public string Prop246 { get; set; }
        public string Prop247 { get; set; }
        public string Prop248 { get; set; }
        public string Prop249 { get; set; }

        public string Prop250 { get; set; }
        public string Prop251 { get; set; }
        public string Prop252 { get; set; }
        public string Prop253 { get; set; }
        public string Prop254 { get; set; }
        public string Prop255 { get; set; }
        public string Prop256 { get; set; }
        public string Prop257 { get; set; }
        public string Prop258 { get; set; }
        public string Prop259 { get; set; }

        public string Prop260 { get; set; }
        public string Prop261 { get; set; }
        public string Prop262 { get; set; }
        public string Prop263 { get; set; }
        public string Prop264 { get; set; }
        public string Prop265 { get; set; }
        public string Prop266 { get; set; }
        public string Prop267 { get; set; }
        public string Prop268 { get; set; }
        public string Prop269 { get; set; }

        public string Prop270 { get; set; }
        public string Prop271 { get; set; }
        public string Prop272 { get; set; }
        public string Prop273 { get; set; }
        public string Prop274 { get; set; }
        public string Prop275 { get; set; }
        public string Prop276 { get; set; }
        public string Prop277 { get; set; }
        public string Prop278 { get; set; }
        public string Prop279 { get; set; }

        public string Prop280 { get; set; }
        public string Prop281 { get; set; }
        public string Prop282 { get; set; }
        public string Prop283 { get; set; }
        public string Prop284 { get; set; }
        public string Prop285 { get; set; }
        public string Prop286 { get; set; }
        public string Prop287 { get; set; }
        public string Prop288 { get; set; }
        public string Prop289 { get; set; }

        public string Prop290 { get; set; }
        public string Prop291 { get; set; }
        public string Prop292 { get; set; }
        public string Prop293 { get; set; }
        public string Prop294 { get; set; }
        public string Prop295 { get; set; }
        public string Prop296 { get; set; }
        public string Prop297 { get; set; }
        public string Prop298 { get; set; }
        public string Prop299 { get; set; }

    }
}
