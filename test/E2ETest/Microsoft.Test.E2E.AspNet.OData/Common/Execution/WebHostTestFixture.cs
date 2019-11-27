// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System;
using System.Linq;
using System.Net;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;
#else
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web.Http;
using Microsoft.Owin.Hosting;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Owin;
using Xunit;
#endif

// Parallelism in the test framework is a feature that is new for (Xunit) version 2. However,
// since each test will spin up a number of web servers each with a listening port, disabling the
// parallel test with take a bit long but consume fewer resources with more stable results.
//
// By default, each test class is a unique test collection. Tests within the same test class will not run
// in parallel against each other. That means that there may be up to # subclasses of WebHostTestBase
// web servers running at any point during the test run, currently ~500. Without this, there would be a
// web server per test case since Xunit 2.0 spawns a new test class instance for each test case.
//
// Using both Disable and Max Threads per this discussion: https://github.com/xunit/xunit/issues/276
//
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly, MaxParallelThreads = 1)]

namespace Microsoft.Test.E2E.AspNet.OData.Common.Execution
{
    /// <summary>
    /// The WebHostTestFixture is create a web host to be used for a test.
    /// </summary>
    /// <remarks>
    /// This is a Class Fixture (see https://xunit.github.io/docs/shared-context.html).
    /// As such, it is instantiated per-class, which is the behavior needed here to ensure
    /// each test class has its own web server, as opposed to Collection Fixtures even though
    /// there is one assembly-wide collection used for serialization purposes.
    /// </remarks>
    public class WebHostTestFixture : IDisposable
    {
        private static readonly string NormalBaseAddressTemplate = "http://{0}:{1}";
        
        private int _port;
        private bool disposedValue = false;
        private Object thisLock = new Object();
        private Action<WebRouteConfiguration> _testConfigurationAction = null;

#if NETCORE
        private IWebHost _selfHostServer = null;
#else
        private IDisposable _selfHostServer = null;
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="WebHostTestFixture"/> class
        /// which uses Katana to host a web service.
        /// </summary>
        public WebHostTestFixture()
        {
            // We need to lazily initialize the fixture because we need the test
            // configuration method and the fixture doesn't know anything about
            // the test class in which is used. The first instance of a test class
            // will initialize the server. This requires that the tests within a class
            // are serialized.
        }

        /// <summary>
        /// The base address of the server.
        /// </summary>
        public string BaseAddress { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether error details should be included.
        /// </summary>
        public bool IncludeErrorDetail { get; set; } = true;

        /// <summary>
        /// Initialize the fixture.
        /// </summary>
        /// <param name="testConfigurationAction">The test configuration action.</param>
        /// <returns>true of the server is initialized, false otherwise.</returns>
        /// <remarks>
        /// This is done lazily to allow the update configuration
        /// function to be passed in from the first test class instance.
        /// </remarks>
        public bool Initialize(Action<WebRouteConfiguration> testConfigurationAction)
        {
            SecurityHelper.AddIpListen();

            int attempts = 0;
            while (attempts++ < 3)
            {
                try
                {
                    if (_selfHostServer == null)
                    {
                        lock (thisLock)
                        {
                            if (_selfHostServer == null)
                            {
#if NETCORE
                                string serverName = "localhost";
#else
                                string serverName = Environment.MachineName;
#endif
                                // setup base address
                                _port = PortArranger.Reserve();
                                this.BaseAddress = string.Format(NormalBaseAddressTemplate, serverName, _port.ToString());

                                // set up the server.
                                _testConfigurationAction = testConfigurationAction;

#if NETCORE
                                _selfHostServer = new WebHostBuilder()
                                    .UseKestrel(options =>
                                    {
                                        options.Listen(IPAddress.Loopback, _port);
                                    })
                                    .UseStartup<WebHostTestStartup>()
                                    .ConfigureServices(services =>
                                    {
                                        // Add ourself to the container so WebHostTestStartup
                                        // can call UpdateConfiguration.
                                        services.AddSingleton<WebHostTestFixture>(this);
                                    })
                                    .ConfigureLogging((hostingContext, logging) =>
                                    {
                                        logging.AddDebug();
                                        logging.SetMinimumLevel(LogLevel.Warning);
                                    })
                                    .Build();

                                _selfHostServer.Start();
#else
                                _selfHostServer = WebApp.Start(this.BaseAddress, DefaultKatanaConfigure);
#endif
                            }
                        }
                    }

                    return true;
                }
                catch (HttpListenerException)
                {
                    // Retry HttpListenerException up to 3 times.
                    _selfHostServer = null;
                }
            }

            throw new TimeoutException(string.Format("Unable to start server after {0} attempts", attempts));
        }

        /// <summary>
        /// Cleanup the server.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

        /// <summary>
        /// Cleanup the server.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_selfHostServer != null)
                    {
#if NETCORE
                        _selfHostServer.StopAsync();
                        _selfHostServer.WaitForShutdown();
#endif
                        _selfHostServer.Dispose();
                        _selfHostServer = null;
                    }
                }

                disposedValue = true;
            }
        }

#if NETCORE
        private class WebHostTestStartup
        {
            DelayLoadFilterProvider<EnableQueryAttribute> enableQueryProvider = new DelayLoadFilterProvider<EnableQueryAttribute>();
            DelayLoadFilterProvider<IActionFilter> actionFilterProvider = new DelayLoadFilterProvider<IActionFilter>();

            public void ConfigureServices(IServiceCollection services)
            {
                services.TryAddEnumerable(ServiceDescriptor.Singleton<IFilterProvider>(enableQueryProvider));
                services.TryAddEnumerable(ServiceDescriptor.Singleton<IFilterProvider>(actionFilterProvider));

                var coreBuilder = services.AddMvcCore(options =>
                {
                    options.Filters.Add(typeof(WebHostLogExceptionFilter));
                    options.Filters.Add(new DelayLoadFilterFactory<ETagMessageHandler>());
#if NETCOREAPP3_0
                    options.EnableEndpointRouting = false;
#else
#endif
                });

#if NETCOREAPP2_1
                coreBuilder.AddJsonFormatters();
#else
                coreBuilder.AddNewtonsoftJson();
#endif
                coreBuilder.AddDataAnnotations();
                services.AddOData();
            }

#if NETCOREAPP2_1
            public void Configure(IApplicationBuilder app, IHostingEnvironment env)
#else
            public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
#endif
            {
                // Add a custom exception handler that returns exception as a raw string. Many of the
                // tests expect to search for the string and UseDeveloperPage() will encode the string
                // in Html format.
                app.Use(async (context, next) =>
                {
                    try
                    {
                        await next.Invoke();
                    }
                    catch (Exception ex)
                    {
                        if (context.Response.HasStarted)
                        {
                            throw;
                        }

                        try
                        {
                            // Write error by default.
                            WebHostTestFixture testBase = context.RequestServices.GetService<WebHostTestFixture>();
                            if (testBase == null || testBase.IncludeErrorDetail)
                            {
                                context.Response.Clear();
                                context.Response.StatusCode = 500;
                                await context.Response.WriteAsync(ex.ToString());
                                return;
                            }
                        }
                        catch (Exception)
                        {
                            // If there's a Exception while generating the error page, re-throw the original exception.
                        }
                        throw;
                    }
                });

                app.UseODataBatching();
                app.UseMvc(routeBuilder =>
                {
                    // Setup regular route.
                    routeBuilder.MapRoute("api default", "api/{controller}/{action=Get}");

                    // Apply test configuration.
                    WebRouteConfiguration config = new WebRouteConfiguration(routeBuilder);
                    WebHostTestFixture testBase = routeBuilder.ServiceProvider.GetRequiredService<WebHostTestFixture>();
                    testBase?._testConfigurationAction(config);

                    // Apply error details
                    testBase.IncludeErrorDetail = config.IncludeErrorDetail;

                    // Apply MvcActions Options.
                    IOptions<MvcOptions> options = routeBuilder.ServiceProvider.GetService<IOptions<MvcOptions>>();
                    if (config.MvcOptionsActions.Any() && options != null)
                    {
                        foreach (var action in config.MvcOptionsActions)
                        {
                            action(options.Value);
                        }
                    }

                    // Apply Json options.
#if NETCORE
#if NETCOREAPP2_1
                    IOptions<MvcJsonOptions> jsonOptions = routeBuilder.ServiceProvider.GetService<IOptions<MvcJsonOptions>>();
#else
                    IOptions<MvcNewtonsoftJsonOptions> jsonOptions = routeBuilder.ServiceProvider.GetService<IOptions<MvcNewtonsoftJsonOptions>>();
#endif
#else
                    IOptions<MvcJsonOptions> jsonOptions = routeBuilder.ServiceProvider.GetService<IOptions<MvcJsonOptions>>();
#endif
                    if (config.JsonReferenceLoopHandling.HasValue && jsonOptions != null)
                    {
                        jsonOptions.Value.SerializerSettings.ReferenceLoopHandling = config.JsonReferenceLoopHandling.Value;
                    }

                    if (config.JsonFormatterIndent.HasValue && jsonOptions != null)
                    {
                        jsonOptions.Value.SerializerSettings.Formatting = config.JsonFormatterIndent.Value
                            ? Newtonsoft.Json.Formatting.Indented
                            : Newtonsoft.Json.Formatting.None;
                    }

                    // Apply Kestrel options.
                    if (config.MaxReceivedMessageSize.HasValue)
                    {
                        IOptions<KestrelServerOptions> serverOptions = routeBuilder.ServiceProvider.GetService<IOptions<KestrelServerOptions>>();
                        if (serverOptions != null)
                        {
                            serverOptions.Value.Limits.MaxRequestBodySize = config.MaxReceivedMessageSize.Value;
                        }
                    }

                    // Apply filters
                    if (config.EnableQueryAttributeFilter != null && options != null)
                    {
                        enableQueryProvider.WrappedFilter = config.EnableQueryAttributeFilter;
                    }

                    if (config.IActionFilterFilter != null && options != null)
                    {
                        actionFilterProvider.WrappedFilter = config.IActionFilterFilter;
                    }

                    if (config.ETagMessageHandlerFilter != null && options != null)
                    {
                        var provider = options.Value.Filters
                            .OfType<DelayLoadFilterFactory<ETagMessageHandler>>()
                            .FirstOrDefault();

                        if (provider != null)
                        {
                            provider.WrappedFilter = config.ETagMessageHandlerFilter;
                        }
                    }
                });
            }

            private class DelayLoadFilterProvider<T> : IFilterProvider where T : IActionFilter
            {
                public T WrappedFilter { get; set; }

                public int Order { get { return 0; } }

                public void OnProvidersExecuting(FilterProviderContext context)
                {
                    if (WrappedFilter != null)
                    {
                        QueryFilterProvider filterProvider = new QueryFilterProvider(WrappedFilter);
                        filterProvider.OnProvidersExecuting(context);
                    }
                }
                public void OnProvidersExecuted(FilterProviderContext context)
                {
                    if (WrappedFilter != null)
                    {
                        QueryFilterProvider filterProvider = new QueryFilterProvider(WrappedFilter);
                        filterProvider.OnProvidersExecuted(context);
                    }
                }
            }

            private class DelayLoadFilterFactory<T> : IFilterFactory where T : IActionFilter
            {
                public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
                {
                    if (WrappedFilter != null)
                    {
                        return WrappedFilter;
                    }

                    return new InternalNoOpFilter();
                }

                public T WrappedFilter { get; set; }

                public bool IsReusable
                {
                    get { return true; }
                }

                private class InternalNoOpFilter : IActionFilter
                {
                    public void OnActionExecuted(ActionExecutedContext context)
                    {
                    }

                    public void OnActionExecuting(ActionExecutingContext context)
                    {
                    }
                }
            }
        }
#else

                    private void DefaultKatanaConfigure(IAppBuilder app)
        {
            // Set default principal to avoid OWIN selfhost bug with VS debugger
            app.Use(async (context, next) =>
            {
                Thread.CurrentPrincipal = null;
                await next();
            });

            // Inject error logging for 500.
            WebHostLogExceptionFilter exceptionFilter = new WebHostLogExceptionFilter();
            app.Use(async (context, next) =>
            {
                await next();

                int[] printExceptionFor = new int[] { 400, 500 };
                if (printExceptionFor.Contains(context.Response.StatusCode) &&
                    exceptionFilter.Exceptions.Count > 0)
                {
                    Console.WriteLine("**************** Internal Server Error ****************");
                    foreach (WebHostErrorRecord error in exceptionFilter.Exceptions)
                    {
                        Console.WriteLine();
                        Console.WriteLine("  Method: " + error.Controller + "::" + error.Method);
                        Console.WriteLine("  Exception: " + error.Exception.ToString());
                        if (error.Exception.InnerException != null)
                        {
                            Console.WriteLine();
                            Console.WriteLine("  Inner Exception: " + error.Exception.InnerException.ToString());
                        }
                    }

                    Console.WriteLine();
                    exceptionFilter.Exceptions.Clear();
                }
            });

            var configuration = new WebRouteConfiguration();
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Filters.Add(exceptionFilter);

            configuration.Routes.MapHttpRoute("api default", "api/{controller}/{action}", new { action = RouteParameter.Optional });

            var httpServer = new HttpServer(configuration);
            configuration.SetHttpServer(httpServer);

            _testConfigurationAction(configuration);

            app.UseWebApi(httpServer: httpServer);
        }
#endif
    }
}
