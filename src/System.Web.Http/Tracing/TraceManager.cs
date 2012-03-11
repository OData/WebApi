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
            ITraceWriter traceWriter = configuration.ServiceResolver.GetTraceWriter();
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
            CreateControllerFactoryTracer(configuration, traceWriter);
            CreateMessageHandlerTracers(configuration, traceWriter);
            CreateMediaTypeFormatterTracers(configuration, traceWriter);
        }

        private static void CreateActionInvokerTracer(HttpConfiguration configuration, ITraceWriter traceWriter)
        {
            IHttpActionInvoker invoker = configuration.ServiceResolver.GetService(typeof(IHttpActionInvoker)) as IHttpActionInvoker;
            HttpActionInvokerTracer tracer = new HttpActionInvokerTracer(invoker, traceWriter);
            configuration.ServiceResolver.SetService(typeof(IHttpActionInvoker), tracer);
        }

        private static void CreateActionSelectorTracer(HttpConfiguration configuration, ITraceWriter traceWriter)
        {
            IHttpActionSelector selector = configuration.ServiceResolver.GetService(typeof(IHttpActionSelector)) as IHttpActionSelector;
            HttpActionSelectorTracer tracer = new HttpActionSelectorTracer(selector, traceWriter);
            configuration.ServiceResolver.SetService(typeof(IHttpActionSelector), tracer);
        }

        private static void CreateActionValueBinderTracer(HttpConfiguration configuration, ITraceWriter traceWriter)
        {
            IActionValueBinder binder = configuration.ServiceResolver.GetService(typeof(IActionValueBinder)) as IActionValueBinder;
            ActionValueBinderTracer tracer = new ActionValueBinderTracer(binder, traceWriter);
            configuration.ServiceResolver.SetService(typeof(IActionValueBinder), tracer);
        }

        private static void CreateContentNegotiatorTracer(HttpConfiguration configuration, ITraceWriter traceWriter)
        {
            IContentNegotiator negotiator = configuration.ServiceResolver.GetService(typeof(IContentNegotiator)) as IContentNegotiator;
            ContentNegotiatorTracer tracer = new ContentNegotiatorTracer(negotiator, traceWriter);
            configuration.ServiceResolver.SetService(typeof(IContentNegotiator), tracer);
        }

        private static void CreateControllerActivatorTracer(HttpConfiguration configuration, ITraceWriter traceWriter)
        {
            IHttpControllerActivator activator = configuration.ServiceResolver.GetService(typeof(IHttpControllerActivator)) as IHttpControllerActivator;
            HttpControllerActivatorTracer tracer = new HttpControllerActivatorTracer(activator, traceWriter);
            configuration.ServiceResolver.SetService(typeof(IHttpControllerActivator), tracer);
        }

        private static void CreateControllerFactoryTracer(HttpConfiguration configuration, ITraceWriter traceWriter)
        {
            IHttpControllerFactory factory = configuration.ServiceResolver.GetService(typeof(IHttpControllerFactory)) as IHttpControllerFactory;
            HttpControllerFactoryTracer tracer = new HttpControllerFactoryTracer(factory, traceWriter);
            configuration.ServiceResolver.SetService(typeof(IHttpControllerFactory), tracer);
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
