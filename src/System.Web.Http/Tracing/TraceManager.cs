// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Tracing.Tracers;

namespace System.Web.Http.Tracing
{
    internal class TraceManager : ITraceManager
    {
        public void Initialize(HttpConfiguration configuration)
        {
            ITraceWriter traceWriter = configuration.Services.GetTraceWriter();
            if (traceWriter != null)
            {
                // Install tracers only when a custom trace writer has been registered
                CreateAllTracers(configuration, traceWriter);
            }
        }

        private static void CreateAllTracers(HttpConfiguration configuration, ITraceWriter traceWriter)
        {
            CreateActionInvokerTracer(configuration, traceWriter);
            CreateActionSelectorTracer(configuration, traceWriter);
            CreateActionValueBinderTracer(configuration, traceWriter);
            CreateContentNegotiatorTracer(configuration, traceWriter);
            CreateControllerActivatorTracer(configuration, traceWriter);
            CreateControllerSelectorTracer(configuration, traceWriter);
            CreateMessageHandlerTracers(configuration, traceWriter);
            CreateMediaTypeFormatterTracers(configuration, traceWriter);
        }

        private static void CreateActionInvokerTracer(HttpConfiguration configuration, ITraceWriter traceWriter)
        {
            IHttpActionInvoker invoker = configuration.Services.GetActionInvoker();
            HttpActionInvokerTracer tracer = new HttpActionInvokerTracer(invoker, traceWriter);
            configuration.Services.Replace(typeof(IHttpActionInvoker), tracer);
        }

        private static void CreateActionSelectorTracer(HttpConfiguration configuration, ITraceWriter traceWriter)
        {
            IHttpActionSelector selector = configuration.Services.GetActionSelector();
            HttpActionSelectorTracer tracer = new HttpActionSelectorTracer(selector, traceWriter);
            configuration.Services.Replace(typeof(IHttpActionSelector), tracer);
        }

        private static void CreateActionValueBinderTracer(HttpConfiguration configuration, ITraceWriter traceWriter)
        {
            IActionValueBinder binder = configuration.Services.GetActionValueBinder();
            ActionValueBinderTracer tracer = new ActionValueBinderTracer(binder, traceWriter);
            configuration.Services.Replace(typeof(IActionValueBinder), tracer);
        }

        private static void CreateContentNegotiatorTracer(HttpConfiguration configuration, ITraceWriter traceWriter)
        {
            IContentNegotiator negotiator = configuration.Services.GetContentNegotiator();
            ContentNegotiatorTracer tracer = new ContentNegotiatorTracer(negotiator, traceWriter);
            configuration.Services.Replace(typeof(IContentNegotiator), tracer);
        }

        private static void CreateControllerActivatorTracer(HttpConfiguration configuration, ITraceWriter traceWriter)
        {
            IHttpControllerActivator activator = configuration.Services.GetHttpControllerActivator();
            HttpControllerActivatorTracer tracer = new HttpControllerActivatorTracer(activator, traceWriter);
            configuration.Services.Replace(typeof(IHttpControllerActivator), tracer);
        }

        private static void CreateControllerSelectorTracer(HttpConfiguration configuration, ITraceWriter traceWriter)
        {
            IHttpControllerSelector controllerSelector = configuration.Services.GetHttpControllerSelector();
            HttpControllerSelectorTracer tracer = new HttpControllerSelectorTracer(controllerSelector, traceWriter);
            configuration.Services.Replace(typeof(IHttpControllerSelector), tracer);
        }

        private static void CreateMediaTypeFormatterTracers(HttpConfiguration configuration, ITraceWriter traceWriter)
        {
            for (int i = 0; i < configuration.Formatters.Count; i++)
            {
                configuration.Formatters[i] = MediaTypeFormatterTracer.CreateTracer(
                                                configuration.Formatters[i],
                                                traceWriter,
                                                request: null);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Will be disposed when pipeline is disposed.")]
        private static void CreateMessageHandlerTracers(HttpConfiguration configuration, ITraceWriter traceWriter)
        {
            // Insert a tracing handler before each existing message handler (in execution order)
            int handlerCount = configuration.MessageHandlers.Count;
            for (int i = 0; i < handlerCount * 2; i += 2)
            {
                DelegatingHandler innerHandler = configuration.MessageHandlers[i];
                DelegatingHandler handlerTracer = new MessageHandlerTracer(innerHandler, traceWriter);
                configuration.MessageHandlers.Insert(i + 1, handlerTracer);
            }

            configuration.MessageHandlers.Add(new RequestMessageHandlerTracer(traceWriter));
        }
    }
}
