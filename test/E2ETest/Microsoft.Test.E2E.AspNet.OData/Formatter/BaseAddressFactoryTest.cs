// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Formatter
{
    public class BaseAddressFactoryModel
    {
        public int ID { get; set; }
    }

    public class BaseAddressFactoryModelsController : TestODataController
    {
        public ITestActionResult Get()
        {
            return Ok(new BaseAddressFactoryModel[] { new BaseAddressFactoryModel { ID = 1 } });
        }
    }

    public class BaseAddressFactoryTest : WebHostTestBase<BaseAddressFactoryTest>
    {
        public BaseAddressFactoryTest(WebHostTestFixture<BaseAddressFactoryTest> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration configuration)
        {
            var controllers = new[] { typeof(BaseAddressFactoryModelsController) };
            configuration.AddControllers(controllers);
            var model = GetEdmModel(configuration);
            configuration.Routes.Clear();
            configuration.MapODataServiceRoute("odata", "odata", model);
            configuration.EnsureInitialized();

            ServicesContainer services = configuration.Services;
            IHttpControllerSelector controllerSelector = services.GetHttpControllerSelector();
            var controllerMappings = controllerSelector.GetControllerMapping().Values;

            foreach (var c in controllerMappings)
            {
                var odataFormatter = c.Configuration.Formatters.OfType<ODataMediaTypeFormatter>();
                foreach (var f in odataFormatter)
                {
                    f.BaseAddressFactory = (m) => new Uri("http://foo.bar/", UriKind.Absolute);
                }
            }
        }

        protected static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var mb = configuration.CreateConventionModelBuilder();
            mb.EntitySet<BaseAddressFactoryModel>("BaseAddressFactoryModels");
            return mb.GetEdmModel();
        }

        [Fact]
        public async Task ShouldReturnTheCustomizedBaseAddress()
        {
            string requestUri = string.Format("{0}/odata/BaseAddressFactoryModels", BaseAddress);

            HttpResponseMessage response = await Client.GetAsync(requestUri);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JObject content = await response.Content.ReadAsObject<JObject>();

            Assert.Contains("http://foo.bar/", (string)content["@odata.context"]);
        }
    }
}
