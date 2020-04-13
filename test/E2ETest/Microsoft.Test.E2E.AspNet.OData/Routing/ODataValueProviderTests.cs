// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Newtonsoft.Json.Linq;
using Xunit;
#else
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Newtonsoft.Json.Linq;
using Xunit;
#endif

namespace Microsoft.Test.E2E.AspNet.OData.Routing
{
    public class ODataValueProviderTests : WebHostTestBase<ODataValueProviderTests>
    {
        public ODataValueProviderTests(WebHostTestFixture<ODataValueProviderTests> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration config)
        {
            config.Routes.Clear();
            IList<IODataRoutingConvention> conventions = ODataRoutingConventions.CreateDefault();
            conventions.Insert(0, new CustomEntityRoutingConvention());
            config.MapODataServiceRoute("odata", "odata", GetModel(config), new DefaultODataPathHandler(), conventions);
#if NETCORE
            config.MapHttpRoute("api", "api/{controller}/{keyAsCustomer}", new { action = "Get", keyAsCustomer = new BindCustomer { Id = -1 } });
#else
            config.MapHttpRoute("api", "api/{controller}/{keyAsCustomer}", new { keyAsCustomer = new BindCustomer { Id = -1 } });
#endif
        }

        private static IEdmModel GetModel(WebRouteConfiguration config)
        {
            ODataModelBuilder builder = config.CreateConventionModelBuilder();
            builder.EntitySet<BindCustomer>("BindCustomers");
            return builder.GetEdmModel();
        }

        [Theory]
        [InlineData("{0}/odata/BindCustomers({1})", 5, (int)HttpStatusCode.OK)]
        [InlineData("{0}/odata/BindCustomers({1})", 0, (int)HttpStatusCode.BadRequest)]
        public async Task CanModelBindNonStringDataFromUri(string urlTemplate, int key, int expectedStatusCode)
        {
            string url = string.Format(urlTemplate, BaseAddress, key);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            HttpResponseMessage response = await Client.SendAsync(request);
            Assert.Equal(expectedStatusCode, (int)response.StatusCode);
            JObject content = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(key, content["Id"]);
        }

        [Theory]
        [InlineData("{0}/api/BindCustomersApi/", (int)HttpStatusCode.BadRequest)]
        public async Task CanModelBindNonStringDataFromUriWebAPI(string urlTemplate, int expectedStatusCode)
        {
            string url = string.Format(urlTemplate, BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            HttpResponseMessage response = await Client.SendAsync(request);
            Assert.Equal(expectedStatusCode, (int)response.StatusCode);
            JObject content = await response.Content.ReadAsObject<JObject>();
#if NETCORE
            Assert.Equal(0, content["id"]);
#else
            Assert.Equal(0, content["Id"]);
#endif
        }
    }

    public class CustomEntityRoutingConvention : EntityRoutingConvention
    {
#if NETCORE
        public override string SelectAction(RouteContext routeContext, SelectControllerResult controllerResult, IEnumerable<ControllerActionDescriptor> actionDescriptors)
        {
            string result = base.SelectAction(routeContext, controllerResult, actionDescriptors);
            IDictionary<string, object> conventionStore = routeContext.HttpContext.Request.ODataFeature().RoutingConventionsStore;
            IDictionary<string, object> routeData = routeContext.RouteData.Values;
#else
        public override string SelectAction(ODataPath odataPath, HttpControllerContext controllerContext, ILookup<string, HttpActionDescriptor> actionMap)
        {
            string result = base.SelectAction(odataPath, controllerContext, actionMap);
            IDictionary<string, object> conventionStore = controllerContext.Request.ODataProperties().RoutingConventionsStore;
            IDictionary<string, object> routeData = controllerContext.RouteData.Values;
#endif

            if (result != null && conventionStore != null)
            {
                conventionStore["keyAsCustomer.Id"] = (int)routeData["key"];
            }

            return result;
        }
    }

    public class BindCustomersApiController : TestNonODataController
    {
        public ITestActionResult Get([FromUri] BindCustomer keyAsCustomer)
        {
            if (keyAsCustomer == null)
            {
                return NotFound();
            }
            else if (!ModelState.IsValid)
            {
                return BadRequest(keyAsCustomer);
            }
            else
            {
                return Ok(keyAsCustomer);
            }
        }
    }

    public class BindCustomersController : TestODataController
    {
        [EnableQuery(PageSize = 10, MaxExpansionDepth = 2)]
        public ITestActionResult Get([FromUri] BindCustomer keyAsCustomer)
        {
            if (keyAsCustomer == null)
            {
                return NotFound();
            }
            else if (!ModelState.IsValid)
            {
                return BadRequest(keyAsCustomer);
            }
            else
            {
                return Ok(keyAsCustomer);
            }
        }
    }

    public class BindCustomer
    {
        [Range(5, 10)]
        public int Id { get; set; }
    }
}
