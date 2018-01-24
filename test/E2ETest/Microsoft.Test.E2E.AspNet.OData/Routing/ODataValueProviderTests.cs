// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Routing
{
    public class ODataValueProviderTests : WebHostTestBase
    {
        public ODataValueProviderTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(HttpConfiguration config)
        {
            config.Routes.Clear();
            IList<IODataRoutingConvention> conventions = ODataRoutingConventions.CreateDefault();
            conventions.Insert(0, new CustomEntityRoutingConvention());
            config.MapODataServiceRoute("odata", "odata", GetModel(), new DefaultODataPathHandler(), conventions);
            config.Routes.MapHttpRoute("api", "api/{controller}/{keyAsCustomer}", new { keyAsCustomer = new BindCustomer { Id = -1 } });
        }

        private static IEdmModel GetModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
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
            dynamic content = await response.Content.ReadAsAsync<JObject>();
            Assert.Equal(key, (int)content.Id);
        }

        [Theory]
        [InlineData("{0}/api/BindCustomersApi/", (int)HttpStatusCode.BadRequest)]
        public async Task CanModelBindNonStringDataFromUriWebAPI(string urlTemplate, int expectedStatusCode)
        {
            string url = string.Format(urlTemplate, BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            HttpResponseMessage response = await Client.SendAsync(request);
            Assert.Equal(expectedStatusCode, (int)response.StatusCode);
            dynamic content = await response.Content.ReadAsAsync<JObject>();
            Assert.Equal(0, (int)content.Id);
        }
    }

    public class CustomEntityRoutingConvention : EntityRoutingConvention
    {
        public override string SelectAction(ODataPath odataPath, HttpControllerContext controllerContext, ILookup<string, HttpActionDescriptor> actionMap)
        {
            string result = base.SelectAction(odataPath, controllerContext, actionMap);
            IDictionary<string, object> conventionStore = controllerContext.Request.ODataProperties().RoutingConventionsStore;
            if (result != null && conventionStore != null)
            {
                conventionStore["keyAsCustomer"] = new BindCustomer { Id = (int)controllerContext.RouteData.Values["key"] };
            }
            return result;
        }
    }

    public class BindCustomersApiController : ApiController
    {
        public IHttpActionResult Get([FromUri] BindCustomer keyAsCustomer)
        {
            if (keyAsCustomer == null)
            {
                return NotFound();
            }
            else if (!ModelState.IsValid)
            {
                return Content(HttpStatusCode.BadRequest, keyAsCustomer);
            }
            else
            {
                return Ok(keyAsCustomer);
            }
        }
    }

    public class BindCustomersController : ODataController
    {
        [EnableQuery(PageSize = 10, MaxExpansionDepth = 2)]
        public IHttpActionResult Get([FromUri] BindCustomer keyAsCustomer)
        {
            if (keyAsCustomer == null)
            {
                return NotFound();
            }
            else if (!ModelState.IsValid)
            {
                return Content(HttpStatusCode.BadRequest, keyAsCustomer);
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
