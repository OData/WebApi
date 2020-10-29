// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Extensions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.OData.Test
{
    public class ApplyTest
    {
        private const string AcceptJsonFullMetadata = "application/json;odata.metadata=full";
        private const string AcceptJson = "application/json";

        private HttpClient _client;

        public ApplyTest()
        {
            Type[] controllers = new[] { typeof(ApplyTestCustomersController), typeof(NonODataApplyTestCustomersController), };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                var builder = ODataConventionModelBuilderFactory.Create(config);
                builder.EntitySet<ApplyTestCustomer>("ApplyTestCustomers");

                config.MapODataServiceRoute("odata", "odata", builder.GetEdmModel());
                config.Count().Filter().OrderBy().Expand().MaxTop(null).Select();

                config.MapNonODataRoute("api", "api/{controller}", new { controller = "NonODataApplyTestCustomers", action="Get" });
                config.EnableDependencyInjection();
            });

            _client = TestServerFactory.CreateClient(server);
        }

        [Fact]
        public async Task Apply_Works_WithODataJson()
        {
            // Arrange
            string uri = "/odata/ApplyTestCustomers?$apply=groupby((Name), aggregate($count as Cnt))";

            // Act
            HttpResponseMessage response = GetResponse(uri, AcceptJson);

            // Assert
            var content = await response.Content.ReadAsStringAsync();
            JArray result = JObject.Parse(content)["value"] as JArray;
            Assert.Equal("Name", result[0]["Name"]);
            Assert.Equal(3, result[0]["Cnt"]);
        }

        [Fact(Skip = "ToDo: (mikep) The non-OData JSON Serializer appears not to be async, so this hangs the build pipeline if AllowSynchronousIO=true is not set.")]
        public async Task Apply_Works_WithNonODataJson()
        {
            // Arrange
            string uri = "/api/?$apply=groupby((Name), aggregate($count as Cnt))";

            // Act
            HttpResponseMessage response = GetResponse(uri, AcceptJson);

            // Assert
            var content = await response.Content.ReadAsStringAsync();
            JArray result = JArray.Parse(content);
            Assert.Equal("Name", result[0]["Name"]);
            Assert.Equal(3, result[0]["Cnt"]);
        }

        private HttpResponseMessage GetResponse(string uri, string acceptHeader)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost" + uri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(acceptHeader));
            return _client.SendAsync(request).Result;
        }
    }

    public class ApplyTestCustomer
    {
        public static IList<ApplyTestCustomer> Customers
        {
            get
            {
                ApplyTestCustomer customer = new ApplyTestCustomer { ID = 42, Name = "Name" };
                ApplyTestOrder order = new ApplyTestOrder { ID = 24, Amount = 100, Customer = customer };
                ApplyTestOrder anotherOrder = new ApplyTestOrder
                {
                    ID = 28,
                    Amount = 10,
                    Customer = customer,
                };
                customer.Orders = new[] { order, anotherOrder };

                ApplyTestCustomer specialCustomer = new ApplyTestCustomer
                {
                    ID = 43,
                    Name = "Name",
                    PreviousCustomer = customer
                };
                ApplyTestOrder specialOrder = new ApplyTestOrder
                {
                    ID = 25,
                    Amount = 100,
                    Customer = specialCustomer
                };
                specialCustomer.Orders = new[] { specialOrder };

                ApplyTestCustomer nextCustomer = new ApplyTestCustomer
                {
                    ID = 44,
                    Name = "Name",
                    PreviousCustomer = specialCustomer
                };
                ApplyTestOrder nextOrder = new ApplyTestOrder
                {
                    ID = 26,
                    Amount = 100,
                    Customer = nextCustomer
                };
                nextCustomer.Orders = new[] { nextOrder };

                return new[] { customer, specialCustomer, nextCustomer };
            }
        }

        public int ID { get; set; }

        public string Name { get; set; }

        public ApplyTestOrder[] Orders { get; set; }

        public ApplyTestCustomer PreviousCustomer { get; set; }
    }


    public class ApplyTestOrder
    {
        public int ID { get; set; }

        public int Amount { get; set; }

        public ApplyTestCustomer Customer { get; set; }
    }


    public class ApplyTestCustomersController : TestODataController
    {
        [EnableQuery]
        public IEnumerable<ApplyTestCustomer> Get()
        {
            return ApplyTestCustomer.Customers;
        }
    }

    public class NonODataApplyTestCustomersController : TestNonODataController
    {
        [EnableQuery]
        public IEnumerable<ApplyTestCustomer> Get()
        {
            return ApplyTestCustomer.Customers;
        }
    }
}
