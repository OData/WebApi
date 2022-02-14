//-----------------------------------------------------------------------------
// <copyright file="AddRelatedObjectTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Newtonsoft.Json;
using Xunit;
#else
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Newtonsoft.Json;
using Xunit;
#endif

namespace Microsoft.Test.E2E.AspNet.OData.Routing
{
    public class AddRelatedObjectTests : WebHostTestBase
    {
        public AddRelatedObjectTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration config)
        {
            config.Routes.Clear();
            config.MapODataServiceRoute("odata", "", GetModel(config), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        private static IEdmModel GetModel(WebRouteConfiguration config)
        {
            ODataModelBuilder builder = config.CreateConventionModelBuilder();
            builder.EntitySet<AROCustomer>("AROCustomers");
            builder.EntityType<AROVipCustomer>().DerivesFrom<AROCustomer>();
            builder.EntitySet<AROAddress>("AROAddresses");
            builder.EntitySet<AROOrder>("Orders");
            return builder.GetEdmModel();
        }

        public static TheoryDataSet AddRelatedObjectConventionsWorkPropertyData
        {
            get
            {
                TheoryDataSet<string, string> dataSet = new TheoryDataSet<string, string>();
                dataSet.Add("POST", "/AROCustomers(5)/Orders");
                dataSet.Add("POST", "/AROCustomers(5)/Microsoft.Test.E2E.AspNet.OData.Routing.AROVipCustomer/Orders");
                // ConventionRouting does not support PUT to single-value navigation property
                // dataSet.Add("PUT", "/AROCustomers(5)/Microsoft.Test.E2E.AspNet.OData.Routing.AROVipCustomer/Address", new AROAddress() { Id = 5 });
                return dataSet;
            }
        }

        [Theory]
        [MemberData(nameof(AddRelatedObjectConventionsWorkPropertyData))]
        public async Task AddRelatedObjectConventionsWork(string method, string url)
        {
            object data = new AROOrder() { Id = 5 };
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), BaseAddress + url);
            request.Content = new StringContent(JsonConvert.SerializeObject(data));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            HttpResponseMessage response = await Client.SendAsync(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    public class AROCustomer
    {
        public int Id { get; set; }
        public IList<AROOrder> Orders { get; set; }
    }

    public class AROVipCustomer : AROCustomer
    {
        public AROAddress Address { get; set; }
    }

    public class AROAddress
    {
        public int Id { get; set; }
    }

    public class AROOrder
    {
        public int Id { get; set; }
    }

    public class AROCustomersController : TestODataController
    {
        public ITestActionResult PostToOrders([FromODataUri] int key, [FromBody] AROOrder order)
        {
            if (key == 5 && order != null && order.Id == 5)
            {
                return Ok();
            }
            return BadRequest();
        }

        public ITestActionResult PutToAddressFromAROVipCustomer([FromODataUri] int key, [FromBody] AROAddress entity)
        {
            if (key == 5 && entity != null && entity.Id == 5)
            {
                return Ok();
            }
            return BadRequest();
        }

    }
}
