// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Properties;

namespace System.Web.Http.Tracing.Tracers
{
    /// <summary>
    /// Tracer for <see cref="IHttpActionSelector"/>.
    /// </summary>
    internal class HttpActionSelectorTracer : IHttpActionSelector
    {
        private const string SelectActionMethodName = "SelectAction";

        private readonly IHttpActionSelector _innerSelector;
        private readonly ITraceWriter _traceWriter;

        public HttpActionSelectorTracer(IHttpActionSelector innerSelector, ITraceWriter traceWriter)
        {
            _innerSelector = innerSelector;
            _traceWriter = traceWriter;
        }

        public ILookup<string, HttpActionDescriptor> GetActionMapping(HttpControllerDescriptor controllerDescriptor)
        {
            return _innerSelector.GetActionMapping(controllerDescriptor);
        }

        HttpActionDescriptor IHttpActionSelector.SelectAction(HttpControllerContext controllerContext)
        {
            HttpActionDescriptor actionDescriptor = null;

              _traceWriter.TraceBeginEnd(
                        controllerContext.Request, 
                        TraceCategories.ActionCategory, 
                        TraceLevel.Info,
                        _innerSelector.GetType().Name,
                        SelectActionMethodName,
                        beginTrace: null,
                        execute: () => { actionDescriptor = _innerSelector.SelectAction(controllerContext); },
                        endTrace: (tr) =>
                        {
                            tr.Message = Error.Format(
                                SRResources.TraceActionSelectedMessage,
                                FormattingUtilities.ActionDescriptorToString(actionDescriptor));
                        },

                        errorTrace: null);

            // Intercept returned HttpActionDescriptor with a tracing version
            return actionDescriptor == null ? null : new HttpActionDescriptorTracer(controllerContext, actionDescriptor, _traceWriter);
        }
    }
}
