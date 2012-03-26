using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Properties;

namespace System.Web.Http.Tracing.Tracers
{
    /// <summary>
    /// Tracer for <see cref=" HttpControllerDescriptor"/>
    /// </summary>
    public class HttpControllerDescriptorTracer : HttpControllerDescriptor
    {
        private const string CreateControllerMethodName = "CreateController";
        private const string ReleaseControllerMethodName = "ReleaseController";

        private readonly HttpControllerDescriptor _innerDescriptor;
        private readonly ITraceWriter _traceWriter;

        public HttpControllerDescriptorTracer(HttpConfiguration configuration, string controllerName, Type controllerType, HttpControllerDescriptor innerDescriptor, ITraceWriter traceWriter)
            : base(configuration, controllerName, controllerType)
        {
            _innerDescriptor = innerDescriptor;
            _traceWriter = traceWriter;
        }

        public override IHttpController CreateController(HttpRequestMessage request)
        {
            IHttpController controller = null;

            _traceWriter.TraceBeginEnd(
                request,
                TraceCategories.ControllersCategory,
                TraceLevel.Info,
                _innerDescriptor.GetType().Name,
                CreateControllerMethodName,
                beginTrace: null,
                execute: () =>
                {
                    controller = _innerDescriptor.CreateController(request);
                },
                endTrace: (tr) =>
                {
                    tr.Message = controller == null
                                        ? SRResources.TraceNoneObjectMessage
                                        : HttpControllerTracer.ActualControllerType(controller).FullName;
                },
                errorTrace: null);

            if (controller != null && !(controller is HttpControllerTracer))
            {
                return new HttpControllerTracer(controller, _traceWriter);
            }

            return controller;
        }

        public override void ReleaseController(IHttpController controller, HttpControllerContext controllerContext)
        {
            _traceWriter.TraceBeginEnd(
                controllerContext.Request,
                TraceCategories.ControllersCategory,
                TraceLevel.Info,
                _innerDescriptor.GetType().Name,
                ReleaseControllerMethodName,
                beginTrace: (tr) =>
                {
                    tr.Message = HttpControllerTracer.ActualControllerType(controller).FullName;
                },
                execute: () =>
                {
                    IHttpController actualController = HttpControllerTracer.ActualController(controller);
                    _innerDescriptor.ReleaseController(actualController, controllerContext);
                },
                endTrace: null,
                errorTrace: null);
        }
    }
}
