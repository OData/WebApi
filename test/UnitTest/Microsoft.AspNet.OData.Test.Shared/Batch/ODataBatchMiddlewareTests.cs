//-----------------------------------------------------------------------------
// <copyright file="ODataBatchMiddlewareTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Xunit;
using ServiceLifetime = Microsoft.OData.ServiceLifetime;

namespace Microsoft.AspNet.OData.Test.Batch
{
    public class ODataBatchMiddlewareTests
    {
        private const string URI = "http://localhost/$batch";

        private readonly ODataBatchMiddleware _batchMiddleware;
        private readonly IRouteBuilder _routeBuilder;

        public ODataBatchMiddlewareTests()
        {
            _batchMiddleware = new ODataBatchMiddleware(CorsDelegate);
            _routeBuilder = RoutingConfigurationFactory.CreateWithRootContainer("odata");
            _routeBuilder.MapODataServiceRoute("odata", "odata", b =>
            {
                b.AddService<ODataBatchHandler>(ServiceLifetime.Singleton,
                    implementationFactory => new TestODataBatchHandler());
            });
        }

        private HttpRequest CreateRequest(HttpMethod method)
        {
            
            HttpRequest request = RequestFactory.Create(method, URI, this._routeBuilder, "odata");
            ODataBatchPathMapping batchMapping =
                request.HttpContext.RequestServices.GetRequiredService<ODataBatchPathMapping>();
            batchMapping.AddRoute("odata", "/$batch");
            return request;
        }

        [Theory]
        [InlineData("/odata/$batch", "/odata/$batch", "odata", "")]
        [InlineData("odata/$batch/", "/odata/$batch/", "odata", "")]
        [InlineData("api/v1/$batch", "/api/v1/$batch", "api/v1", "")]
        [InlineData("v2{data}/$batch", "/v2Customers/$batch", "v2Customers", "")]
        [InlineData("v2{data}/$batch", "/v2Customers/$batch/", "v2Customers", "/proxy")]
        [InlineData("tenant/{tenant}/v{version}/$batch", "/tenant/acme/v2/$batch/", "tenant/acme/v2", "/proxy")]
        [InlineData("tenant/{tenant:int}/$batch", "/tenant/42/$batch", "tenant/42", "")]
        [InlineData("{tenant?}/$batch", "/acme/$batch", "acme", "/proxy")]
        [InlineData("$batch", "/$batch", "", "")]
        [InlineData("$batch", "/$batch", "", "/proxy")]
        [InlineData("odata/$batch", "/odata/$batch", "odata", "")]
        public async Task BatchMiddlewareInitializesConcreteServiceRootBeforeHandler(string template, string path, string expectedPrefix, string pathBase)
        {
            HttpRequest request = RequestFactory.Create(HttpMethod.Post, "http://localhost" + path, _routeBuilder, "odata");
            request.PathBase = pathBase;
            request.ODataFeature().RoutePrefix = "stale";
            request.HttpContext.RequestServices.GetRequiredService<ODataBatchPathMapping>().AddRoute("odata", template);

            await _batchMiddleware.Invoke(request.HttpContext);

            Assert.Equal(expectedPrefix, request.ODataFeature().RoutePrefix);
            Assert.Equal(expectedPrefix, request.HttpContext.Items["RoutePrefix"]);
            Assert.Equal(new Uri("http://localhost" + pathBase + "/" + (expectedPrefix.Length == 0 ? "" : expectedPrefix + "/")), request.HttpContext.Items["BatchBaseUri"]);
        }

        [Theory]
        [InlineData("POST", "/other/$batch")]
        [InlineData("OPTIONS", "/odata/$batch")]
        [InlineData("POST", "/v2/$batch")]
        [InlineData("POST", "/$batch")]
        public async Task BatchMiddlewareDoesNotInitializeServiceRootForUnmatchedRequests(string method, string path)
        {
            HttpRequest request = RequestFactory.Create(new HttpMethod(method), "http://localhost" + path, _routeBuilder, "odata");
            request.ODataFeature().RoutePrefix = "existing";
            request.HttpContext.RequestServices.GetRequiredService<ODataBatchPathMapping>().AddRoute("odata", "{tenant?}/v2/$batch");

            await _batchMiddleware.Invoke(request.HttpContext);

            Assert.Equal("existing", request.ODataFeature().RoutePrefix);
            Assert.True(request.HttpContext.Items.ContainsKey("TestKey"));
            Assert.False(request.HttpContext.Items.ContainsKey("BatchBaseUri"));
        }

        [Fact]
        public async Task BatchMiddlewareResolvesServiceRootIndependentlyForEachRequest()
        {
            HttpRequest customerRequest = RequestFactory.Create(HttpMethod.Post, "http://localhost/v2Customers/$batch", _routeBuilder, "odata");
            HttpRequest orderRequest = RequestFactory.Create(HttpMethod.Post, "http://localhost/v2Orders/$batch", _routeBuilder, "odata");
            customerRequest.HttpContext.RequestServices.GetRequiredService<ODataBatchPathMapping>().AddRoute("odata", "v2{data}/$batch");

            await _batchMiddleware.Invoke(customerRequest.HttpContext);
            await _batchMiddleware.Invoke(orderRequest.HttpContext);

            Assert.Equal("v2Customers", customerRequest.HttpContext.Items["RoutePrefix"]);
            Assert.Equal("v2Orders", orderRequest.HttpContext.Items["RoutePrefix"]);
            Assert.Equal(new Uri("http://localhost/v2Customers/"), customerRequest.HttpContext.Items["BatchBaseUri"]);
            Assert.Equal(new Uri("http://localhost/v2Orders/"), orderRequest.HttpContext.Items["BatchBaseUri"]);
            Assert.Equal("Customers", customerRequest.ODataFeature().BatchRouteData["data"]);
            Assert.Equal("Orders", orderRequest.ODataFeature().BatchRouteData["data"]);
            Assert.Equal("v2Customers", customerRequest.ODataFeature().RoutePrefix);
        }

        [Theory]
        [InlineData("v2{data}", "/v2Customers/$batch", "", "/v2Customers/Items", "/v2Customers/Items")]
        [InlineData("v2{data}", "/v2Customers/$batch", "", "http://localhost/v2Customers/Items", "/v2Customers/Items")]
        [InlineData("v2{data}", "/v2Customers/$batch", "", "Items", "/v2Customers/Items")]
        [InlineData("v2{data}", "/v2Customers/$batch/", "", "Items", "/v2Customers/Items")]
        [InlineData("v2{data}", "/v2Orders/$batch", "", "Items", "/v2Orders/Items")]
        [InlineData("v{version:int}", "/v2/$batch", "", "Items", "/v2/Items")]
        [InlineData("v{version:int}", "/v999/$batch", "", "/v999/Items", "/v999/Items")]
        [InlineData("{tenant}/v{version}", "/contoso/v2/$batch", "", "Items", "/contoso/v2/Items")]
        [InlineData("{tenant}/v{version}", "/fabrikam/v3/$batch", "", "/fabrikam/v3/Items", "/fabrikam/v3/Items")]
        [InlineData("{tenant?}/v2", "/contoso/v2/$batch", "", "Items", "/contoso/v2/Items")]
        [InlineData("{tenant?}/v2", "/fabrikam/v2/$batch", "", "http://localhost/fabrikam/v2/Items", "/fabrikam/v2/Items")]
        [InlineData("v2{data}", "/v2Customers/$batch", "/base", "Items", "/base/v2Customers/Items")]
        [InlineData("v2{data}", "/v2Orders/$batch", "/base", "http://localhost/base/v2Orders/Items", "/base/v2Orders/Items")]
        [InlineData("odata", "/odata/$batch", "", "/ODATA/Items", "/ODATA/Items")]
        [InlineData("odata", "/odata/$batch", "", "/odata", "/odata")]
        [InlineData("", "/$batch", "", "/outside/Items", "/outside/Items")]
        [InlineData("", "/$batch", "/base", "Items", "/base/Items")]
        public async Task BatchMiddlewareExecutesOperationsWithinResolvedServiceRoot(string template, string batchPath, string pathBase, string operationUrl, string expectedPath)
        {
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler();
            List<string> executedPaths = new List<string>();
            using (TestServer server = new TestServer(new WebHostBuilder().ConfigureServices(services =>
            {
                services.AddMvc();
                services.AddOData();
            }).Configure(app =>
            {
                IRouteBuilder routeBuilder = new RouteBuilder(app) { DefaultHandler = _routeBuilder.DefaultHandler };
                routeBuilder.MapODataServiceRoute("batch", template, builder => builder.AddService<ODataBatchHandler>(ServiceLifetime.Singleton, _ => batchHandler));
                if (pathBase.Length > 0)
                {
                    app.UsePathBase(pathBase);
                }
                app.UseODataBatching();
                app.Run(context =>
                {
                    executedPaths.Add(context.Request.PathBase + context.Request.Path);
                    context.Response.StatusCode = StatusCodes.Status204NoContent;
                    return Task.CompletedTask;
                });
            })))
            {
                string payload = "{\"requests\":[{\"id\":\"1\",\"method\":\"POST\",\"url\":\"" + operationUrl + "\"},{\"id\":\"2\",\"method\":\"POST\",\"url\":\"" + operationUrl + "\"}]}";
                using (HttpClient client = server.CreateClient())
                using (HttpResponseMessage response = await client.PostAsync(pathBase + batchPath, new StringContent(payload, Encoding.UTF8, "application/json")))
                {
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    Assert.Contains("\"status\":204", await response.Content.ReadAsStringAsync());
                }
                Assert.Equal(new[] { expectedPath, expectedPath }, executedPaths);
                Assert.Equal(template, batchHandler.ODataRoute.RoutePrefix);
            }
        }

        [Theory]
        [InlineData("v2{data}", "/v2Customers/$batch", "", "/v2Orders/Items")]
        [InlineData("v2{data}", "/v2Customers/$batch", "", "http://localhost/v2Orders/Items")]
        [InlineData("v2{data}", "/v2Customers/$batch", "", "/v2CustomersAdmin/Items")]
        [InlineData("v2{data}", "/v2Customers/$batch", "", "../v2Orders/Items")]
        [InlineData("v{version:int}", "/v2/$batch", "", "/v3/Items")]
        [InlineData("{tenant}/v{version}", "/contoso/v2/$batch", "", "/fabrikam/v2/Items")]
        [InlineData("{tenant}/v{version}", "/contoso/v2/$batch", "", "/contoso/v3/Items")]
        [InlineData("{tenant?}/v2", "/contoso/v2/$batch", "", "/fabrikam/v2/Items")]
        [InlineData("v2{data}", "/v2Customers/$batch", "/base", "/base/v2Orders/Items")]
        [InlineData("v2{data}", "/v2Customers/$batch", "/base", "/v2Customers/Items")]
        [InlineData("odata", "/odata/$batch", "", "http://localhost/admin/write")]
        [InlineData("odata", "/odata/$batch", "", "../admin/write")]
        [InlineData("odata", "/odata/$batch", "", "/odata-admin/write")]
        [InlineData("odata", "/odata/$batch", "/base", "/base/outside/write")]
        [InlineData("", "/$batch", "/base", "/outside/write")]
        [InlineData("", "/$batch", "/base", "/base-admin/write")]
        public async Task BatchMiddlewareRejectsMixedPathsBeforeExecutingAnyOperation(string template, string batchPath, string pathBase, string operationUrl)
        {
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler();
            int executedOperations = 0;
            using (TestServer server = new TestServer(new WebHostBuilder().ConfigureServices(services =>
            {
                services.AddMvc();
                services.AddOData();
            }).Configure(app =>
            {
                IRouteBuilder routeBuilder = new RouteBuilder(app) { DefaultHandler = _routeBuilder.DefaultHandler };
                routeBuilder.MapODataServiceRoute("batch", template, builder => builder.AddService<ODataBatchHandler>(ServiceLifetime.Singleton, _ => batchHandler));
                if (pathBase.Length > 0)
                {
                    app.UsePathBase(pathBase);
                }
                app.UseODataBatching();
                app.Run(context =>
                {
                    executedOperations++;
                    return Task.CompletedTask;
                });
            })))
            {
                string payload = "{\"requests\":[{\"id\":\"1\",\"method\":\"POST\",\"url\":\"Items\"},{\"id\":\"2\",\"method\":\"POST\",\"url\":\"" + operationUrl + "\"}]}";
                using (HttpClient client = server.CreateClient())
                {
                    InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => client.PostAsync(pathBase + batchPath, new StringContent(payload, Encoding.UTF8, "application/json")));

                    Assert.Contains("is not within the OData service root", exception.Message);
                    Assert.Equal(0, executedOperations);
                }
            }
        }

        [Theory]
        [InlineData("/$batch", "")]
        [InlineData("/odata/$batch/", "odata")]
        [InlineData("/tenant/acme/v2/$batch", "tenant/acme/v2")]
        public void GetRoutePrefix_UsesResolvedBatchPath(string path, string expected)
        {
            Assert.Equal(expected, new PathString(path).GetRoutePrefix());
        }

        [Fact]
        public async Task BatchMiddlewareShouldNotHandlePreflightRequests()
        {
            var request = CreateRequest(HttpMethod.Options);

            await _batchMiddleware.Invoke(request.HttpContext);

            Assert.True(request.HttpContext.Items.ContainsKey("TestKey"));
        }

        [Fact]
        public async Task BatchMiddlewareShouldWorkNormallyForNonPreflightRequests()
        {
            var request = CreateRequest(HttpMethod.Post);
            await _batchMiddleware.Invoke(request.HttpContext);

            Assert.False(request.HttpContext.Items.ContainsKey("TestKey"));
        }



        private Task CorsDelegate(HttpContext context)
        {
            // This mocks the execution of the cors middleware which should be registered for cors to work normally.
            // the ODatabatch middleware should not process the request but instead pass it on to the next middleware for cors to work as expected.
            // https://github.com/dotnet/aspnetcore/blob/master/src/Middleware/CORS/src/Infrastructure/CorsMiddleware.cs
            context.Items.Add("TestKey", "TestValue");
            return Task.CompletedTask;
        }

    }

    public class TestODataBatchHandler : DefaultODataBatchHandler
    {
        public override Task ProcessBatchAsync(HttpContext context, RequestDelegate nextHandler)
        {
            context.Items["RoutePrefix"] = context.Request.ODataFeature().RoutePrefix;
            context.Items["BatchBaseUri"] = GetBaseUri(context.Request);
            return Task.CompletedTask;
        }
    }
}
#endif
