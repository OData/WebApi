using System.Collections.Generic;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Properties;

namespace System.Web.Http.Tracing.Tracers
{
    /// <summary>
    /// Tracer for <see cref="IHttpControllerFactory"/>.
    /// </summary>
    internal class HttpControllerFactoryTracer : IHttpControllerFactory
    {
        private const string CreateControllerMethodName = "CreateController";
        private const string ReleaseControllerMethodName = "ReleaseController";

        private readonly IHttpControllerFactory _innerFactory;
        private readonly ITraceWriter _traceWriter;

        public HttpControllerFactoryTracer(IHttpControllerFactory innerFactory, ITraceWriter traceWriter)
        {
            _innerFactory = innerFactory;
            _traceWriter = traceWriter;
        }

        IHttpController IHttpControllerFactory.CreateController(HttpControllerContext controllerContext, string controllerName)
        {
            IHttpController controller = null;

            _traceWriter.TraceBeginEnd(
                controllerContext.Request,
                TraceCategories.ControllersCategory,
                TraceLevel.Info,
                _innerFactory.GetType().Name,
                CreateControllerMethodName,
                beginTrace: (tr) =>
                {
                    tr.Message = Error.Format(
                                    SRResources.TraceControllerNameAndRouteMessage, 
                                    controllerName, 
                                    FormattingUtilities.RouteToString(controllerContext.RouteData));
                },
                execute: () =>
                {
                    controller = _innerFactory.CreateController(controllerContext, controllerName);
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
                controller = new HttpControllerTracer(controller, _traceWriter);
            }

            return controller;
        }

        IDictionary<string, HttpControllerDescriptor> IHttpControllerFactory.GetControllerMapping()
        {
            return _innerFactory.GetControllerMapping();
        }

        void IHttpControllerFactory.ReleaseController(HttpControllerContext controllerContext, IHttpController controller)
        {
            _traceWriter.TraceBeginEnd(
                controllerContext.Request,
                TraceCategories.ControllersCategory,
                TraceLevel.Info,
                _innerFactory.GetType().Name,
                ReleaseControllerMethodName,
                beginTrace: (tr) =>
                {
                    tr.Message = HttpControllerTracer.ActualControllerType(controller).FullName;
                },
                execute: () =>
                {
                    IHttpController actualController = HttpControllerTracer.ActualController(controller);
                    _innerFactory.ReleaseController(controllerContext, actualController);
                },
                endTrace: null,
                errorTrace: null);
        }
    }
}
