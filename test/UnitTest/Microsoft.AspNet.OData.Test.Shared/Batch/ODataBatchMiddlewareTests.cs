//-----------------------------------------------------------------------------
// <copyright file="ODataBatchMiddlewareTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
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
            return Task.CompletedTask;
        }
    }
}
#endif
