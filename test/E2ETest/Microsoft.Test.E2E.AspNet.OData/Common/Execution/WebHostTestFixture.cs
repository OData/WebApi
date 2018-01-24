// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web.Http;
using Microsoft.Owin.Hosting;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Owin;
using Xunit;

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
        private static readonly string DefaultRouteTemplate = "api/{controller}/{action}";

        private int _port;
        private IDisposable _katanaSelfHostServer = null;
        private Action<HttpConfiguration> _testConfigurationAction = null;
        private bool disposedValue = false;
        private Object thisLock = new Object();

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
        /// Initialize the fixture.
        /// </summary>
        /// <param name="testConfigurationAction">The test configuration action.</param>
        /// <returns>true of the server is initialized, false otherwise.</returns>
        /// <remarks>
        /// This is done lazily to allow the update configuration
        /// function to be passed in from the first test class instance.
        /// </remarks>
        public bool Initialize(Action<HttpConfiguration> testConfigurationAction)
        {
            SecurityHelper.AddIpListen();

            int attempts = 0;
            while (attempts++ < 3)
            {
                try
                {
                    if (_katanaSelfHostServer == null)
                    {
                        lock (thisLock)
                        {
                            if (_katanaSelfHostServer == null)
                            {
                                // setup base address
                                _port = PortArranger.Reserve();
                                string baseAddress = string.Format(NormalBaseAddressTemplate, Environment.MachineName, _port.ToString());
                                this.BaseAddress = baseAddress.Replace("localhost", Environment.MachineName);

                                // set up the server.
                                _testConfigurationAction = testConfigurationAction;
                                _katanaSelfHostServer = WebApp.Start(baseAddress, DefaultKatanaConfigure);
                            }
                        }
                    }

                    return true;
                }
                catch (HttpListenerException)
                {
                    // Retry HttpListenerException up to 3 times.
                    _katanaSelfHostServer = null;
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
                    if (_katanaSelfHostServer != null)
                    {
                        _katanaSelfHostServer.Dispose();
                        _katanaSelfHostServer = null;
                    }
                }

                disposedValue = true;
            }
        }

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

            var configuration = new HttpConfiguration();
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Filters.Add(exceptionFilter);

            configuration.Routes.MapHttpRoute("api default", DefaultRouteTemplate, new { action = RouteParameter.Optional });

            var httpServer = new HttpServer(configuration);
            configuration.SetHttpServer(httpServer);

            _testConfigurationAction(configuration);

            app.UseWebApi(httpServer: httpServer);
        }
    }
}
