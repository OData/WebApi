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
#if NETCOREAPP3_1
        [Fact]
        public async Task BatchMiddlewareShouldNotHandlePreflightRequests()
        {
            string uri = "http://localhost/$batch";
            var request = RequestFactory.Create(HttpMethod.Options, uri);
            RequestDelegate next = CorsDelegate;
            var sut = new ODataBatchMiddleware(next);

            await sut.Invoke(request.HttpContext);

            Assert.True(request.HttpContext.Items.ContainsKey("TestKey"));
        }


#endif
        [Fact]
        public async Task BatchMiddlewareShouldWorkNormally()
        {
            string uri = "http://localhost/$batch";
            IRouteBuilder routeBuilder = RoutingConfigurationFactory.CreateWithRootContainer("odata");
            routeBuilder.MapODataServiceRoute("odata", "odata", b =>
            {
                b.AddService<ODataBatchHandler>(ServiceLifetime.Singleton,
                    implementationFactory => new TestODataBatchHandler());
            });
            var request = RequestFactory.Create(HttpMethod.Post, uri, routeBuilder, "odata");
            ODataBatchPathMapping batchMapping = request.HttpContext.RequestServices.GetRequiredService<ODataBatchPathMapping>();
            batchMapping.AddRoute("odata", "/$batch");

            RequestDelegate next = CorsDelegate;
            var sut = new ODataBatchMiddleware(next);

            await sut.Invoke(request.HttpContext);

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
