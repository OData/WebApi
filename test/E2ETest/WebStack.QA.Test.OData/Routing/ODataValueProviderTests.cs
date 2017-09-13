// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Nuwa;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.Routing
{
    [NuwaFramework]
    public class ODataValueProviderTests
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration config)
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
        [InlineData("{0}/odata/BindCustomers({1})", 5, HttpStatusCode.OK)]
        [InlineData("{0}/odata/BindCustomers({1})", 0, HttpStatusCode.BadRequest)]
        public void CanModelBindNonStringDataFromUri(string urlTemplate, int key, HttpStatusCode expectedStatusCode)
        {
            string url = string.Format(urlTemplate, BaseAddress, key);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            HttpResponseMessage response = Client.SendAsync(request).Result;
            Assert.Equal(expectedStatusCode, response.StatusCode);
            dynamic content = response.Content.ReadAsAsync<JObject>().Result;
            Assert.Equal(key, (int)content.Id);
        }

        [Theory]
        [InlineData("{0}/api/BindCustomersApi/", HttpStatusCode.BadRequest)]
        public void CanModelBindNonStringDataFromUriWebAPI(string urlTemplate, HttpStatusCode expectedStatusCode)
        {
            string url = string.Format(urlTemplate, BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            HttpResponseMessage response = Client.SendAsync(request).Result;
            Assert.Equal(expectedStatusCode, response.StatusCode);
            dynamic content = response.Content.ReadAsAsync<JObject>().Result;
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
