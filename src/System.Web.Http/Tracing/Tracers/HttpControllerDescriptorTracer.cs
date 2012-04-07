// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Properties;

namespace System.Web.Http.Tracing.Tracers
{
    /// <summary>
    /// Tracer for <see cref=" HttpControllerDescriptor"/>
    /// </summary>
    internal class HttpControllerDescriptorTracer : HttpControllerDescriptor
    {
        private const string CreateControllerMethodName = "CreateController";

        private readonly HttpControllerDescriptor _innerDescriptor;
        private readonly ITraceWriter _traceWriter;

        public HttpControllerDescriptorTracer(HttpConfiguration configuration, string controllerName, Type controllerType, HttpControllerDescriptor innerDescriptor, ITraceWriter traceWriter)
            : base(configuration, controllerName, controllerType)
        {
            _innerDescriptor = innerDescriptor;
            _traceWriter = traceWriter;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "This object is returned back to the caller")]
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
                return new HttpControllerTracer(request, controller, _traceWriter);
            }

            return controller;
        }
    }
}
