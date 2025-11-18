//-----------------------------------------------------------------------------
// <copyright file="EndpointTestFixture.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;

namespace Microsoft.Test.E2E.AspNet.OData.Endpoint
{
    /// <summary>
    /// This is a XUnit Class Fixture.
    /// The EndpointTestFixture creates a <see cref="IHost"/> to be used for a test.
    /// </summary>
    /// <remarks>
    /// As such, it is instantiated per-class, which is the behavior needed here to ensure
    /// each test class has its own web server.
    /// Fixture is instantiated before all test class instaintiation.
    /// </remarks>
    public class EndpointTestFixture<T> : IDisposable
    {
        private static readonly string NormalBaseAddressTemplate = "http://{0}:{1}";

        // In versions of ASP.NET Core earlier than 3.0, the Web Host is used for HTTP workloads.
        // The Web Host is no longer recommended for web apps and remains available only for backward compatibility.
        private IHost _selfHostServer = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="EndpointTestFixture{T}"/> class
        /// </summary>
        public EndpointTestFixture()
        {
            Initialize();
        }

        /// <summary>
        /// The base address of the server.
        /// </summary>
        public string BaseAddress { get; private set; }

        /// <summary>
        /// Gets the client factory
        /// </summary>
        public IHttpClientFactory ClientFactory { get; private set; }

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
            if (disposing)
            {
                if (_selfHostServer != null)
                {
                    _selfHostServer.StopAsync();
                    _selfHostServer.WaitForShutdownAsync();
                    _selfHostServer = null;
                }
            }
        }

        /// <summary>
        /// Initialize the fixture.
        /// </summary>
        private void Initialize()
        {
            SecurityHelper.AddIpListen();

            // Be noted:
            // We use the convention as follows
            // 1) if you want to configure the service, add "protected static void UpdateConfigureServices(IServiceCollection)" method into your test class.
            // 2) if you want to configure the routing, add "protected static void updateConfigure(EndpointRouteConfiguration)" method into your test class.
            Type testType = typeof(T);
            MethodInfo configureServicesMethod = testType.GetMethod("UpdateConfigureServices", BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo configureMethod = testType.GetMethod("UpdateConfigure", BindingFlags.NonPublic | BindingFlags.Static);
            // Owing that this is used in Test only, I assume every developer can following the convention.
            // So I skip the method parameter checking.

            string serverName = "localhost";

            // setup base address
            int port = PortArranger.Reserve();
            this.BaseAddress = string.Format(NormalBaseAddressTemplate, serverName, port.ToString());

            _selfHostServer = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder => webBuilder
                    .UseKestrel(options => options.Listen(IPAddress.Loopback, port))
                    .ConfigureServices(services =>
                    {
                        services.AddHttpClient(); // Add IHttpClientFactory
                        services.AddOData();
                        services.AddRouting();

                        // Apply custom services for each test class
                        configureServicesMethod?.Invoke(null, new object[] { services });
                    })
                    .Configure(app =>
                    {
                        this.ClientFactory = app.ApplicationServices.GetRequiredService<IHttpClientFactory>();

                        // should add ODataBatch middleware before the routing middelware
                        app.UseODataBatching();
                        app.UseRouting();
                        app.UseEndpoints(endpoints =>
                        {
                            // Apply test configuration.
                            EndpointRouteConfiguration config = new EndpointRouteConfiguration(endpoints);
                            configureMethod?.Invoke(null, new object[] { config });
                        });
                    })).Build();

            _selfHostServer.Start();
        }
    }
}
