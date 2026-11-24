//-----------------------------------------------------------------------------
// <copyright file="ControllerBatchServiceRootTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
#if !NETCOREAPP2_1
using Microsoft.AspNetCore.Http.Features;
#endif
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Batch
{
    public class ControllerBatchServiceRootTests
    {
        [Theory]
        [InlineData("v2{data}", "/v2Customers/$batch", "", "/v2Customers/BatchTestCustomers", true)]
        [InlineData("v2{data}", "/v2Customers/$batch", "", "http://localhost/v2Customers/BatchTestCustomers", true)]
        [InlineData("v2{data}", "/v2Customers/$batch", "", "BatchTestCustomers", true)]
        [InlineData("v2{data}", "/v2Customers/$batch/", "", "/v2Customers/BatchTestCustomers", true)]
        [InlineData("v2{data}", "/v2Customers/$batch", "", "/v2Orders/BatchTestCustomers", false)]
        [InlineData("v2{data}", "/v2Customers/$batch", "", "http://localhost/v2Orders/BatchTestCustomers", false)]
        [InlineData("v2{data}", "/v2Customers/$batch", "", "/v2CustomersAdmin/BatchTestCustomers", false)]
        [InlineData("v2{data}", "/v2Customers/$batch", "", "../v2Orders/BatchTestCustomers", false)]
        [InlineData("v{version:int}", "/v2/$batch", "", "/v2/BatchTestCustomers", true)]
        [InlineData("v{version:int}", "/v3/$batch", "", "/v3/BatchTestCustomers", true)]
        [InlineData("v{version:int}", "/v4/$batch", "", "/v4/BatchTestCustomers", true)]
        [InlineData("v{version:int}", "/v10/$batch", "", "/v10/BatchTestCustomers", true)]
        [InlineData("v{version:int}", "/v999/$batch", "", "/v999/BatchTestCustomers", true)]
        [InlineData("v{version:int}", "/v2/$batch", "", "/v3/BatchTestCustomers", false)]
        [InlineData("v{version:int}", "/v2/$batch", "", "/v4/BatchTestCustomers", false)]
        [InlineData("v{version:int}", "/v2/$batch", "", "/v5/BatchTestCustomers", false)]
        [InlineData("{tenant}/v{version}", "/contoso/v2/$batch", "", "/contoso/v2/BatchTestCustomers", true)]
        [InlineData("{tenant}/v{version}", "/contoso/v3/$batch", "", "/contoso/v3/BatchTestCustomers", true)]
        [InlineData("{tenant}/v{version}", "/fabrikam/v2/$batch", "", "/fabrikam/v2/BatchTestCustomers", true)]
        [InlineData("{tenant}/v{version}", "/some-tenant/v1001/$batch", "", "/some-tenant/v1001/BatchTestCustomers", true)]
        [InlineData("{tenant}/v{version}", "/contoso/v2/$batch", "", "/fabrikam/v2/BatchTestCustomers", false)]
        [InlineData("{tenant}/v{version}", "/contoso/v2/$batch", "", "/contoso/v3/BatchTestCustomers", false)]
        [InlineData("{tenant?}/v2", "/contoso/v2/$batch", "", "/contoso/v2/BatchTestCustomers", true)]
        [InlineData("{tenant?}/v2", "/contoso/v2/$batch", "", "http://localhost/contoso/v2/BatchTestCustomers", true)]
        [InlineData("{tenant?}/v2", "/fabrikam/v2/$batch", "", "/fabrikam/v2/BatchTestCustomers", true)]
        [InlineData("{tenant?}/v2", "/fabrikam/v2/$batch", "", "http://localhost/fabrikam/v2/BatchTestCustomers", true)]
        [InlineData("{tenant?}/v2", "/some-tenant/v2/$batch", "", "/some-tenant/v2/BatchTestCustomers", true)]
        [InlineData("{tenant?}/v2", "/some-tenant/v2/$batch", "", "http://localhost/some-tenant/v2/BatchTestCustomers", true)]
        [InlineData("{tenant?}/v2", "/fabrikam/v2/$batch", "", "/contoso/v2/BatchTestCustomers", false)]
        [InlineData("{tenant?}/v2", "/contoso/v2/$batch", "", "/fabrikam/v2/BatchTestCustomers", false)]
        [InlineData("{tenant?}/v2", "/oya/v2/$batch", "", "/some-tenant/v2/BatchTestCustomers", false)]
        [InlineData("v2{data}", "/v2Customers/$batch", "/base", "/base/v2Customers/BatchTestCustomers", true)]
        [InlineData("v2{data}", "/v2Customers/$batch", "/base", "http://localhost/base/v2Customers/BatchTestCustomers", true)]
        [InlineData("v2{data}", "/v2Orders/$batch", "/base", "/base/v2Orders/BatchTestCustomers", true)]
        [InlineData("v2{data}", "/v2Orders/$batch", "/base", "http://localhost/base/v2Orders/BatchTestCustomers", true)]
        [InlineData("v2{data}", "/v2Orders/$batch", "/base", "/base/v2Customers/BatchTestCustomers", false)]
        [InlineData("v2{data}", "/v2Customers/$batch", "/base", "/base/v2Orders/BatchTestCustomers", false)]
        public async Task ControllerBatchEnforcesResolvedTemplateServiceRoot(string template, string batchPath, string pathBase, string operationUrl, bool withinServiceRoot)
        {
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler();
            using (TestServer server = new TestServer(new WebHostBuilder().UseContentRoot(AppContext.BaseDirectory).ConfigureServices(services =>
            {
#if NETCOREAPP2_1
                services.AddMvc();
#else
                services.AddMvc(options => options.EnableEndpointRouting = false).AddNewtonsoftJson();
#endif
                services.AddOData();
            }).Configure(app =>
            {
#if !NETCOREAPP2_1
                app.Use(next => context =>
                {
                    IHttpBodyControlFeature body = context.Features.Get<IHttpBodyControlFeature>();
                    if (body != null)
                    {
                        body.AllowSynchronousIO = true;
                    }
                    return next(context);
                });
#endif
                if (pathBase.Length > 0)
                {
                    app.UsePathBase(pathBase);
                }
                app.UseODataBatching();
                app.UseMvc(routes =>
                {
                    var builder = ODataConventionModelBuilderFactory.Create(routes);
                    builder.EntitySet<BatchTestCustomer>("BatchTestCustomers");
                    routes.MapODataServiceRoute("batch", template, builder.GetEdmModel(), batchHandler);
                    ApplicationPartManager parts = routes.ApplicationBuilder.ApplicationServices.GetRequiredService<ApplicationPartManager>();
                    parts.ApplicationParts.Clear();
                    parts.ApplicationParts.Add(new AssemblyPart(new MockAssembly(new[] { typeof(BatchTestCustomersController) })));
                    parts.FeatureProviders.Clear();
                    parts.FeatureProviders.Add(new ControllerFeatureProvider());
                });
            })))
            using (HttpClient client = server.CreateClient())
            {
                string payload = "{\"requests\":[{\"id\":\"1\",\"method\":\"GET\",\"url\":\"" + operationUrl + "\"}]}";
                if (withinServiceRoot)
                {
                    using (HttpResponseMessage response = await client.PostAsync(pathBase + batchPath, new StringContent(payload, Encoding.UTF8, "application/json")))
                    {
                        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                        JObject batchResponse = JObject.Parse(await response.Content.ReadAsStringAsync());
                        Assert.True((int)Assert.Single(batchResponse["responses"])["status"] == 200, batchResponse.ToString());
                    }
                }
                else
                {
                    InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => client.PostAsync(pathBase + batchPath, new StringContent(payload, Encoding.UTF8, "application/json")));
                    Assert.Contains("is not within the OData service root", exception.Message);
                }
                Assert.Equal(template, batchHandler.ODataRoute.RoutePrefix);
                Assert.Equal("batch", batchHandler.ODataRouteName);
            }
        }

        [Fact]
        public async Task ControllerBatchResolvesTemplateIndependentlyForEachRequest()
        {
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler();
            using (TestServer server = TestServerFactory.Create(new[] { typeof(BatchTestCustomersController) }, routes =>
            {
                var builder = ODataConventionModelBuilderFactory.Create(routes);
                builder.EntitySet<BatchTestCustomer>("BatchTestCustomers");
                routes.MapODataServiceRoute("batch", "v2{data}", builder.GetEdmModel(), batchHandler);
            }))
            using (HttpClient client = server.CreateClient())
            {
                const string customerPayload = "{\"requests\":[{\"id\":\"1\",\"method\":\"GET\",\"url\":\"/v2Customers/BatchTestCustomers\"}]}";
                using (HttpResponseMessage response = await client.PostAsync("/v2Customers/$batch", new StringContent(customerPayload, Encoding.UTF8, "application/json")))
                {
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    Assert.Equal(200, (int)Assert.Single(JObject.Parse(await response.Content.ReadAsStringAsync())["responses"])["status"]);
                }

                const string orderPayload = "{\"requests\":[{\"id\":\"1\",\"method\":\"GET\",\"url\":\"/v2Orders/BatchTestCustomers\"}]}";
                using (HttpResponseMessage response = await client.PostAsync("/v2Orders/$batch", new StringContent(orderPayload, Encoding.UTF8, "application/json")))
                {
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    Assert.Equal(200, (int)Assert.Single(JObject.Parse(await response.Content.ReadAsStringAsync())["responses"])["status"]);
                }
                Assert.Equal("v2{data}", batchHandler.ODataRoute.RoutePrefix);
            }
        }

        [Fact]
        public async Task ControllerBatchDoesNotMatchWhenOptionalLeadingSegmentIsOmitted()
        {
            using (TestServer server = TestServerFactory.Create(new[] { typeof(BatchTestCustomersController) }, routes =>
            {
                var builder = ODataConventionModelBuilderFactory.Create(routes);
                builder.EntitySet<BatchTestCustomer>("BatchTestCustomers");
                routes.MapODataServiceRoute("batch", "{tenant?}/v2", builder.GetEdmModel(), new DefaultODataBatchHandler());
            }))
            using (HttpClient client = server.CreateClient())
            using (HttpResponseMessage response = await client.PostAsync("/v2/$batch", new StringContent("{\"requests\":[{\"id\":\"1\",\"method\":\"GET\",\"url\":\"BatchTestCustomers\"}]}", Encoding.UTF8, "application/json")))
            {
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
        }

        [Fact]
        public async Task ControllerBatchRejectsOutsidePrefixAfterProxyForwardsBatch()
        {
            bool targetReached = false;
            string blockedPath = null;
            string forwardedPath = null;
            string forwardedHost = null;
            string capturedRoutePrefix = null;
            InvalidOperationException forwardingException = null;
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler();
            using (TestServer backend = new TestServer(new WebHostBuilder().ConfigureServices(services =>
            {
                services.AddMvc();
                services.AddOData();
            }).Configure(app =>
            {
                var routes = new RouteBuilder(app) { DefaultHandler = new MyMvcRouteHandler() };
                var builder = ODataConventionModelBuilderFactory.Create(routes);
                builder.EntitySet<BatchTestCustomer>("BatchTestCustomers");
                routes.MapODataServiceRoute("batch", "odata", builder.GetEdmModel(), batchHandler);
                app.Use(async (context, next) =>
                {
                    try
                    {
                        await next();
                    }
                    finally
                    {
                        capturedRoutePrefix = context.Request.ODataFeature().RoutePrefix;
                    }
                });
                app.UseODataBatching();
                app.Run(context =>
                {
                    if (HttpMethods.IsPost(context.Request.Method) && context.Request.Path == "/admin/write")
                    {
                        targetReached = true;
                        context.Response.StatusCode = StatusCodes.Status204NoContent;
                    }
                    return Task.CompletedTask;
                });
            })))
            using (HttpClient backendClient = backend.CreateClient())
            using (TestServer proxy = new TestServer(new WebHostBuilder().Configure(app => app.Run(async context =>
            {
                if (context.Request.Path.StartsWithSegments("/admin"))
                {
                    blockedPath = context.Request.Path;
                    context.Response.Headers["X-Proxy-Decision"] = "blocked-admin-path";
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return;
                }
                if (context.Request.Path != "/odata/$batch")
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }
                forwardedPath = context.Request.Path;
                forwardedHost = context.Request.Host.Value;
                using (HttpRequestMessage forwardedRequest = new HttpRequestMessage(new HttpMethod(context.Request.Method), new Uri(context.Request.Scheme + "://" + context.Request.Host + context.Request.Path + context.Request.QueryString)))
                {
                    forwardedRequest.Content = new StreamContent(context.Request.Body);
                    foreach (var header in context.Request.Headers)
                    {
                        if (header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase) || header.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                        if (!forwardedRequest.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()))
                        {
                            forwardedRequest.Content.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                        }
                    }
                    try
                    {
                        using (HttpResponseMessage response = await backendClient.SendAsync(forwardedRequest))
                        {
                            context.Response.StatusCode = (int)response.StatusCode;
                            await response.Content.CopyToAsync(context.Response.Body);
                        }
                    }
                    catch (InvalidOperationException exception)
                    {
                        forwardingException = exception;
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    }
                }
            }))))
            using (HttpClient proxyClient = proxy.CreateClient())
            {
                using (HttpResponseMessage response = await proxyClient.PostAsync("/admin/write", new StringContent("{}", Encoding.UTF8, "application/json")))
                {
                    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
                    Assert.Equal("blocked-admin-path", Assert.Single(response.Headers.GetValues("X-Proxy-Decision")));
                    Assert.Equal("/admin/write", blockedPath);
                    Assert.False(targetReached);
                }

                const string payload = "{\"requests\":[{\"id\":\"1\",\"method\":\"POST\",\"url\":\"http://localhost/admin/write\",\"headers\":{\"Content-Type\":\"application/json\"},\"body\":{}}]}";
                using (HttpResponseMessage response = await proxyClient.PostAsync("/odata/$batch", new StringContent(payload, Encoding.UTF8, "application/json")))
                {
                    Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
                    Assert.Empty(await response.Content.ReadAsStringAsync());
                }
                Assert.NotNull(forwardingException);
                Assert.Contains("is not within the OData service root", forwardingException.Message);
                Assert.False(targetReached);
                Assert.Equal("/odata/$batch", forwardedPath);
                Assert.Equal("localhost", forwardedHost);
                Assert.Equal("odata", batchHandler.ODataRoute.RoutePrefix);
                Assert.Equal("odata", capturedRoutePrefix);
            }
        }
    }
}
