// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.OData
{
    /// <summary>
    /// Allows fine grained configuration of essential OData services.
    /// </summary>
    public class ODataCoreBuilder : IODataCoreBuilder
    {
        private readonly ODataContextProvider _provider;
        private readonly IServiceCollection _serviceCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataCoreBuilder"/> class.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        public ODataCoreBuilder(IServiceCollection serviceCollection)
        {
            if (serviceCollection == null)
            {
                throw Error.ArgumentNull("serviceCollection");
            }

            this._serviceCollection = serviceCollection;
            this._provider = new ODataContextProvider();
            _serviceCollection.AddSingleton(_=>this._provider);
        }

        public void Register<T>(string prefix) where T : class
        {
            this._provider.Register<T>(prefix);
        }

        public IServiceCollection Services => _serviceCollection;
    }

    public class ODataContextProvider
    {
        internal IDictionary<string, ODataContext> ContextMap { get; } = new Dictionary<string, ODataContext>();

        internal void Register<T>(string prefix) where T : class
        {
            ContextMap[prefix] = new ODataContext(typeof(T));
        }

        internal ODataContext Lookup(string prefix)
        {
            return ContextMap[prefix];
        }
    }
}
