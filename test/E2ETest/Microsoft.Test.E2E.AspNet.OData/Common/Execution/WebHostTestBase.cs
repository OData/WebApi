//-----------------------------------------------------------------------------
// <copyright file="WebHostTestBase.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Net.Http;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Execution
{
    /// <summary>
    /// The WebHostTestBase creates a web host to be used for a test.
    /// </summary>
    public abstract class WebHostTestBase : IClassFixture<WebHostTestFixture>, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebHostTestBase"/> class.
        /// </summary>
        /// <param name="fixture">The fixture used to initialize the web service.</param>
        protected WebHostTestBase(WebHostTestFixture fixture)
        {
            // Initialize the fixture and get the client and base address.
            fixture.Initialize(this.UpdateConfiguration);
            this.BaseAddress = fixture.BaseAddress;
            this.Client = new HttpClient();
        }

        /// <summary>
        /// The base address of the server.
        /// </summary>
        public string BaseAddress { get; private set; }

        /// <summary>
        /// An HttpClient to use with the server.
        /// </summary>
        public HttpClient Client { get; set; }

        /// <summary>
        /// A configuration method for the server.
        /// </summary>
        /// <param name="configuration"></param>
        protected abstract void UpdateConfiguration(WebRouteConfiguration configuration);

        public void Dispose()
        {
            if (Client != null)
            {
                Client.Dispose();
            }

            Client = null;
        }
    }
}
