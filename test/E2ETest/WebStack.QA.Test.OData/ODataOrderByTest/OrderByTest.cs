﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData.Extensions;
using Newtonsoft.Json.Linq;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.ODataOrderByTest
{
    using System.Collections.Generic;
    using System.Web.Http.Results;

    using Newtonsoft.Json;

    [NuwaFramework]
    [NuwaTrace(NuwaTraceAttribute.Tag.Off)]
    public class ODataOrderByTest
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            var controllers = new[] { typeof(ItemsController) };
            TestAssemblyResolver resolver = new TestAssemblyResolver(new TypesInjectionAssembly(controllers));
            configuration.Services.Replace(typeof(IAssembliesResolver), resolver);

            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            configuration.Routes.Clear();
            HttpServer httpServer = configuration.GetHttpServer();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);

            configuration.MapODataServiceRoute(
                routeName: "odata",
                routePrefix: "odata",
                model: OrderByEdmModel.GetModel());

            configuration.EnsureInitialized();
        }

        [Fact]
        public async Task TestOrderByResult()
        {   // Arrange
            string requestUri = $"{BaseAddress}/odata/Items";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var rawResult = response.Content.ReadAsStringAsync().Result;
            var jsonResult = JObject.Parse(rawResult);
            var jsonValue = jsonResult.SelectToken("value");
            Assert.NotNull(jsonValue);
            var concreteResult = jsonValue.ToObject<List<Item>>();
            Assert.NotEmpty(concreteResult);
            for (var i = 0; i < concreteResult.Count - 1; i++)
            {
                Assert.True(concreteResult[i].Name.StartsWith($"#{i+1}"), "Incorrect order.");
            }
        }
    }
}