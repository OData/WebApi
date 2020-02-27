// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net.Http;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Endpoint
{
    /// <summary>
    /// The <see cref="EndpointTestBase{T}"/> provides the class Fixture funcationlity to be used for a test.
    /// A class Fixture process is:
    /// 1) Create the fixture class, and put the startup code in the fixture class constructor.
    /// 2) If the fixture class needs to perform cleanup, implement IDisposable on the fixture class, and put the cleanup code in the Dispose() method.
    /// 3) Add IClassFixture<> to the test class.
    /// 4) If the test class needs access to the fixture instance, add it as a constructor argument, and it will be provided automatically.
    /// </summary>
    /// <typeparamref name="T">The real test class type.</typeparamref>
    public abstract class EndpointTestBase<T> : IClassFixture<EndpointTestFixture<T>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EndpointTestBase{T}"/> class.
        /// </summary>
        /// <param name="fixture">The fixture used to initialize the Endpoint host service.</param>
        protected EndpointTestBase(EndpointTestFixture<T> fixture)
        {
            this.BaseAddress = fixture.BaseAddress;
            this.Client = fixture.ClientFactory.CreateClient();
        }

        /// <summary>
        /// The base address of the server.
        /// </summary>
        public string BaseAddress { get; }

        /// <summary>
        /// The Http client.
        /// </summary>
        public HttpClient Client { get; }
    }
}
