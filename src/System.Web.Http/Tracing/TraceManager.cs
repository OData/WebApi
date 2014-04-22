// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;
using System.Web.Http.Services;
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
            CreateHttpControllerTypeResolverTracer(configuration, traceWriter);
            CreateGlobalMessageHandlerTracers(configuration, traceWriter);
            CreateRouteSpecificMessageHandlerTracers(configuration, traceWriter);
            CreateMediaTypeFormatterTracers(configuration, traceWriter);
        }

        // Get services from the global config. These are normally per-controller services, but we're getting the global fallbacks.
        private static TService GetService<TService>(ServicesContainer services)
        {
            return (TService)services.GetService(typeof(TService));
        }

        private static void CreateActionInvokerTracer(HttpConfiguration configuration, ITraceWriter traceWriter)
        {
            IHttpActionInvoker invoker = GetService<IHttpActionInvoker>(configuration.Services);
            if (invoker != null && !(invoker is HttpActionInvokerTracer))
            {
                HttpActionInvokerTracer tracer = new HttpActionInvokerTracer(invoker, traceWriter);
                configuration.Services.Replace(typeof(IHttpActionInvoker), tracer);
            }
        }

        private static void CreateActionSelectorTracer(HttpConfiguration configuration, ITraceWriter traceWriter)
        {
            IHttpActionSelector selector = GetService<IHttpActionSelector>(configuration.Services);
            if (selector != null && !(selector is HttpActionSelectorTracer))
            {
                HttpActionSelectorTracer tracer = new HttpActionSelectorTracer(selector, traceWriter);
                configuration.Services.Replace(typeof(IHttpActionSelector), tracer);
            }
        }

        private static void CreateActionValueBinderTracer(HttpConfiguration configuration, ITraceWriter traceWriter)
        {
            IActionValueBinder binder = GetService<IActionValueBinder>(configuration.Services);
            if (binder != null && !(binder is ActionValueBinderTracer))
            {
                ActionValueBinderTracer tracer = new ActionValueBinderTracer(binder, traceWriter);
                configuration.Services.Replace(typeof(IActionValueBinder), tracer);
            }
        }

        private static void CreateContentNegotiatorTracer(HttpConfiguration configuration, ITraceWriter traceWriter)
        {
            IContentNegotiator negotiator = configuration.Services.GetContentNegotiator();
            if (negotiator != null && !(negotiator is ContentNegotiatorTracer))
            {
                ContentNegotiatorTracer tracer = new ContentNegotiatorTracer(negotiator, traceWriter);
                configuration.Services.Replace(typeof(IContentNegotiator), tracer);
            }
        }

        private static void CreateControllerActivatorTracer(HttpConfiguration configuration, ITraceWriter traceWriter)
        {
            IHttpControllerActivator activator = GetService<IHttpControllerActivator>(configuration.Services);
            if (activator != null && !(activator is HttpControllerActivatorTracer))
            {
                HttpControllerActivatorTracer tracer = new HttpControllerActivatorTracer(activator, traceWriter);
                configuration.Services.Replace(typeof(IHttpControllerActivator), tracer);
            }
        }

        private static void CreateControllerSelectorTracer(HttpConfiguration configuration, ITraceWriter traceWriter)
        {
            IHttpControllerSelector controllerSelector = configuration.Services.GetHttpControllerSelector();
            if (controllerSelector != null && !(controllerSelector is HttpControllerSelectorTracer))
            {
                HttpControllerSelectorTracer tracer = new HttpControllerSelectorTracer(controllerSelector, traceWriter);
                configuration.Services.Replace(typeof(IHttpControllerSelector), tracer);
            }
        }

        private static void CreateHttpControllerTypeResolverTracer(HttpConfiguration configuration, ITraceWriter traceWriter)
        {
            DefaultHttpControllerTypeResolver resolver =
                configuration.Services.GetHttpControllerTypeResolver() as DefaultHttpControllerTypeResolver;
            if (resolver != null)
            {
                IHttpControllerTypeResolver tracer = new DefaultHttpControllerTypeResolverTracer(resolver, traceWriter);
                configuration.Services.Replace(typeof(IHttpControllerTypeResolver), tracer);
            }
        }

        private static void CreateMediaTypeFormatterTracers(HttpConfiguration configuration, ITraceWriter traceWriter)
        {
            for (int i = 0; i < configuration.Formatters.Count; i++)
            {
                MediaTypeFormatter formatter = configuration.Formatters[i];
                if (!(formatter is IFormatterTracer))
                {
                    configuration.Formatters[i] = MediaTypeFormatterTracer.CreateTracer(
                                                    configuration.Formatters[i],
                                                    traceWriter,
                                                    request: null);
                }
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "Will be disposed when pipeline is disposed.")]
        private static void CreateGlobalMessageHandlerTracers(HttpConfiguration configuration, ITraceWriter traceWriter)
        {
            Collection<DelegatingHandler> handlers = configuration.MessageHandlers;
            if (handlers.Count == 0)
            {
                handlers.Add(new RequestMessageHandlerTracer(traceWriter));
                return;
            }

            // If message handlers have already been wired into the pipeline,
            // we do not install tracing message handlers. This scenario occurs
            // when initialization is attempted twice, such as per-controller configuration. 
            if (handlers[0].InnerHandler != null)
            {
                return;
            }

            // RequestMessageHandlerTracer will be the first tracer to get executed and each messageHandlerTracer
            // will execute before its respective handler. For the MessageHandlerTracers to be registered, 
            // the message handler list must be of the form:
            // requestMessageHandlerTracer
            // messageHandler1Tracer
            // messageHandler1
            // ...
            // ...
            // messageHandlerNTracer
            // messageHandlerN
            // Where "N" is a non-negative integer. That is, there could be zero or more pairs of handlers/tracers, plus a
            // request tracer at the beginning.
            // If the state matches this pattern, no need to recreate.
            if (handlers[0] is RequestMessageHandlerTracer &&
                AreMessageHandlerTracersRegistered(handlers, startIndex: 1))
            {
                return;
            }

            // If the state does not match this pattern, the tracer list should be recreated.
            CreateMessageHandlerTracers(handlers, traceWriter);

            // CreateMessageHandlerTracers will clean up all RequestMessageHandlerTracer and MessageHandlerTracer
            // and then add a new MessageHandlerTracer for each handler.
            // We want to put RequestMessageHandlerTracer back so that it is always placed at the head of the
            // global handlers collection.
            handlers.Insert(0, new RequestMessageHandlerTracer(traceWriter));
        }

        private static void CreateRouteSpecificMessageHandlerTracers(HttpConfiguration configuration, ITraceWriter traceWriter)
        {
            foreach (HttpRoute route in configuration.Routes)
            {
                if (route.Handler == null)
                {
                    continue;
                }

                Collection<DelegatingHandler> handlers = new Collection<DelegatingHandler>();
                HttpMessageHandler innerHandler = null;
                DelegatingHandler delegatingHandler = route.Handler as DelegatingHandler;
                while (delegatingHandler != null)
                {
                    handlers.Add(delegatingHandler);
                    innerHandler = delegatingHandler.InnerHandler;
                    delegatingHandler = innerHandler as DelegatingHandler;
                }

                // We install tracers for route specific handlers only once if the user did not install them by
                // following the form:
                // [0]: messageHandler0Tracer
                // [1]: messageHandler0
                // [2]: messageHandler1Tracer
                // [3]: messageHandler1
                // ...
                // ...
                // [2*N]: messageHandlerNTracer
                // [2*N + 1]: messageHandlerN
                // Where "N" >= 0 
                // Further initialization attempts are prevented, such as per-controller configuration.
                // If the state matches this pattern, no need to recreate.
                if (AreMessageHandlerTracersRegistered(handlers, startIndex: 0))
                {
                    return;
                }

                // If the state does not match this pattern, the tracer list should be recreated.
                CreateMessageHandlerTracers(handlers, traceWriter);

                // No need to add the RequestMessageHandlerTracer because there will be one in the global handlers.

                // For global handlers, configuration.MessageHandlers will be arranged to a pipeline later in
                // HttpServer.Initialize.
                // Different from that, we have to create a pipeline for route specific handlers here.
                HttpMessageHandler outerHandler = innerHandler;
                IEnumerable<DelegatingHandler> reversedHandlers = handlers.Reverse();
                foreach (DelegatingHandler handler in reversedHandlers)
                {
                    handler.InnerHandler = outerHandler;
                    outerHandler = handler;
                }

                route.Handler = outerHandler;
            }
        }

        // Check whether the sub collection starting from [startIndex] is empty or in the correct order, matching:
        // [startIndex + 0]: messageHandler0Tracer
        // [startIndex + 1]: messageHandler0
        // [startIndex + 2]: messageHandler1Tracer
        // [startIndex + 3]: messageHandler1
        // ...
        // ...
        // [startIndex + 2*N]: messageHandlerNTracer
        // [startIndex + (2*N + 1)]: messageHandlerN
        // Where "N" >= 0
        private static bool AreMessageHandlerTracersRegistered(Collection<DelegatingHandler> messageHandlers, int startIndex)
        {
            Contract.Assert(startIndex >= 0 && startIndex <= messageHandlers.Count);

            int handlerCount = messageHandlers.Count - startIndex;

            if (handlerCount == 0)
            {
                return true;
            }

            if (handlerCount % 2 != 0)
            {
                return false;
            }

            // Check if all [startIndex + 2*N] positions have tracers and
            // [startIndex + (2*N + 1)] positions have their corresponding handlers.
            for (int i = startIndex; i < handlerCount; i += 2)
            {
                DelegatingHandler tracer = messageHandlers[i];
                DelegatingHandler messageHandler = messageHandlers[i + 1];
                if (!(tracer is MessageHandlerTracer))
                {
                    return false;
                }

                DelegatingHandler innerHandler = Decorator.GetInner(tracer);
                if (innerHandler != messageHandler)
                {
                    return false;
                }
            }

            return true;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "Will be disposed when pipeline is disposed.")]
        private static void CreateMessageHandlerTracers(Collection<DelegatingHandler> messageHandlers,
            ITraceWriter traceWriter)
        {
            int handlerCount = messageHandlers.Count;

            // Removing the MessageHandlerTracer and RequestMessageHandlerTracer in the reverse order.
            for (int i = handlerCount - 1; i >= 0; i--)
            {
                if (messageHandlers[i] is RequestMessageHandlerTracer || messageHandlers[i] is MessageHandlerTracer)
                {
                    messageHandlers.RemoveAt(i);
                }
            }
            handlerCount = messageHandlers.Count;

            // Insert a tracing handler before each existing message handler (in execution order).
            for (int i = 0; i < handlerCount * 2; i += 2)
            {
                DelegatingHandler innerHandler = messageHandlers[i];
                DelegatingHandler handlerTracer = new MessageHandlerTracer(innerHandler, traceWriter);
                messageHandlers.Insert(i, handlerTracer);
            }
        }
    }
}
