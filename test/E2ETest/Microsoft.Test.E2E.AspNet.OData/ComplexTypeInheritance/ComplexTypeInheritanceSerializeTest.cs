// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ComplexTypeInheritance
{
    public class ComplexTypeInheritanceSerializeTest : WebHostTestBase<ComplexTypeInheritanceSerializeTest>
    {
        public ComplexTypeInheritanceSerializeTest(WebHostTestFixture<ComplexTypeInheritanceSerializeTest> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration configuration)
        {
            var controllers = new[] { typeof(MetadataController), typeof(InheritanceCustomersController) };
            configuration.AddControllers(controllers);

            configuration.Routes.Clear();

            configuration.MapODataServiceRoute(routeName: "odata", routePrefix: "odata", model: GetEdmModel(configuration));

            configuration.EnsureInitialized();
        }

        [Fact]
        public async Task CanQueryInheritanceComplexInComplexProperty()
        {
            string requestUri = string.Format("{0}/odata/InheritanceCustomers?$format=application/json;odata.metadata=full", BaseAddress);

            HttpResponseMessage response = await Client.GetAsync(requestUri);
            string contentOfString = await response.Content.ReadAsStringAsync();

            Assert.True(HttpStatusCode.OK == response.StatusCode,
                String.Format("\nExpected status code: {0},\n actual: {1},\n request uri: {2},\n message: {3}",
                HttpStatusCode.OK,
                response.StatusCode,
                requestUri,
                contentOfString));

            JObject contentOfJObject = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(2, contentOfJObject.Count);
            Assert.Equal(5, contentOfJObject["value"].Count());

            Assert.Equal(new[]
            {
                "#Microsoft.Test.E2E.AspNet.OData.ComplexTypeInheritance.InheritanceAddress",
                "#Microsoft.Test.E2E.AspNet.OData.ComplexTypeInheritance.InheritanceAddress",
                "#Microsoft.Test.E2E.AspNet.OData.ComplexTypeInheritance.InheritanceUsAddress",
                "#Microsoft.Test.E2E.AspNet.OData.ComplexTypeInheritance.InheritanceCnAddress",
                "#Microsoft.Test.E2E.AspNet.OData.ComplexTypeInheritance.InheritanceCnAddress"
            },
            contentOfJObject["value"].Select(e => e["Location"]["Address"]["@odata.type"]).Select(c => (string)c));
        }

        public static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            ODataConventionModelBuilder builder = configuration.CreateConventionModelBuilder();
            builder.EntitySet<InheritanceCustomer>("InheritanceCustomers");
            builder.ComplexType<InheritanceLocation>();
            return builder.GetEdmModel();
        }
    }

    public class InheritanceCustomersController : TestODataController
    {
        private readonly IList<InheritanceCustomer> _customers;
        public InheritanceCustomersController()
        {
            InheritanceAddress address = new InheritanceAddress
            {
                City = "Tokyo",
                Street = "Tokyo Rd"
            };

            InheritanceAddress usAddress = new InheritanceUsAddress
            {
                City = "Redmond",
                Street = "One Microsoft Way",
                ZipCode = 98052
            };

            InheritanceAddress cnAddress = new InheritanceCnAddress
            {
                City = "Shanghai",
                Street = "ZiXing Rd",
                PostCode = "200241"
            };

            _customers = Enumerable.Range(1, 5).Select(e =>
                new InheritanceCustomer
                {
                    Id = e,
                    Location = new InheritanceLocation
                    {
                        Name = "Location #" + e,
                        Address = e < 3 ? address : e < 4 ? usAddress : cnAddress
                    }
                }).ToList();
        }

        [EnableQuery]
        public ITestActionResult Get()
        {
            return Ok(_customers);
        }
    }

    public class InheritanceCustomer
    {
        public int Id { get; set; }

        public InheritanceLocation Location { get; set; }
    }

    public class InheritanceLocation
    {
        public string Name { get; set; }

        public InheritanceAddress Address { get; set; }
    }

    public class InheritanceAddress
    {
        public string City { get; set; }

        public string Street { get; set; }
    }

    public class InheritanceUsAddress : InheritanceAddress
    {
        public int ZipCode { get; set; }
    }

    public class InheritanceCnAddress : InheritanceAddress
    {
        public string PostCode { get; set; }
    }
}
