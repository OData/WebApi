using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;

namespace System.Web.Http.Tracing.Tracers
{
    /// <summary>
    /// Tracer for <see cref="IHttpController"/>.
    /// </summary>
    internal class HttpControllerTracer : IHttpController
    {
        private const string ExecuteAsyncMethodName = "ExecuteAsync";

        private readonly IHttpController _innerController;
        private readonly ITraceWriter _traceWriter;

        public HttpControllerTracer(IHttpController innerController, ITraceWriter traceWriter)
        {
            _innerController = innerController;
            _traceWriter = traceWriter;
        }

        Task<HttpResponseMessage> IHttpController.ExecuteAsync(HttpControllerContext controllerContext, CancellationToken cancellationToken)
        {
            return _traceWriter.TraceBeginEndAsync<HttpResponseMessage>(
                controllerContext.Request,
                TraceCategories.ControllersCategory,
                TraceLevel.Info,
                _innerController.GetType().Name,
                ExecuteAsyncMethodName,
                beginTrace: null,
                execute: () =>
                {
                    // Critical to allow wrapped controller to have itself in ControllerContext
                    controllerContext.Controller = ActualController(controllerContext.Controller);
                    return _innerController.ExecuteAsync(controllerContext, cancellationToken);
                },
                endTrace: (tr, response) =>
                {
                    if (response != null)
                    {
                        tr.Status = response.StatusCode;
                    }
                },
                errorTrace: null);
        }

        public static IHttpController ActualController(IHttpController controller)
        {
            HttpControllerTracer tracer = controller as HttpControllerTracer;
            return tracer == null ? controller : tracer._innerController;
        }

        public static Type ActualControllerType(IHttpController controller)
        {
            return ActualController(controller).GetType();
        }
    }
}
