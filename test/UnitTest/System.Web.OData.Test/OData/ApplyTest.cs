// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Query;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace System.Web.OData
{
    public class ApplyTest
    {
        private const string AcceptJsonFullMetadata = "application/json;odata.metadata=full";
        private const string AcceptJson = "application/json";

        private HttpConfiguration _configuration;
        private HttpClient _client;

        public ApplyTest()
        {
            _configuration =
                new[]
                {
                    typeof(ApplyTestCustomersController),
                    typeof(NonODataApplyTestCustomersController),
                }.GetHttpConfiguration();
            _configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            _configuration.Count().Filter().OrderBy().Expand().MaxTop(null).Select();

            _configuration.MapODataServiceRoute("odata", "odata", GetModel());
            _configuration.Routes.MapHttpRoute("api", "api/{controller}", new { controller = "NonODataApplyTestCustomers" });
            _configuration.EnableDependencyInjection();

            HttpServer server = new HttpServer(_configuration);
            _client = new HttpClient(server);
        }

        [Fact]
        public void Apply_Works_WithODataJson()
        {
            // Arrange
            string uri = "/odata/ApplyTestCustomers?$apply=groupby((Name), aggregate($count as Cnt))";

            // Act
            HttpResponseMessage response = GetResponse(uri, AcceptJson);

            // Assert
            var r = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine(r);
            JArray result = JObject.Parse(response.Content.ReadAsStringAsync().Result)["value"] as JArray;
            Assert.Equal("Name", result[0]["Name"]);
            Assert.Equal(3, result[0]["Cnt"]);
        }

        [Fact]
        public void Apply_Works_WithNonODataJson()
        {
            // Arrange
            string uri = "/api/?$apply=groupby((Name), aggregate($count as Cnt))";

            // Act
            HttpResponseMessage response = GetResponse(uri, AcceptJson);

            // Assert
            JArray result = JArray.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.Equal("Name", result[0]["Name"]);
            Assert.Equal(3, result[0]["Cnt"]);
        }

        private HttpResponseMessage GetResponse(string uri, string acceptHeader)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost" + uri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(acceptHeader));
            request.SetConfiguration(_configuration);
            return _client.SendAsync(request).Result;
        }
        private IEdmModel GetModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<ApplyTestCustomer>("ApplyTestCustomers");
            return builder.GetEdmModel();
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


    public class ApplyTestCustomersController : ODataController
    {
        [EnableQuery]
        public IEnumerable<ApplyTestCustomer> Get()
        {
            return ApplyTestCustomer.Customers;
        }
    }

    public class NonODataApplyTestCustomersController : ApiController
    {
        [EnableQuery]
        public IEnumerable<ApplyTestCustomer> Get()
        {
            return ApplyTestCustomer.Customers;
        }
    }
}
