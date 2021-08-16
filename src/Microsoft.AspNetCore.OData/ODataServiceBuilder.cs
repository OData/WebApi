//-----------------------------------------------------------------------------
// <copyright file="ODataServiceBuilder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Allows fine grained configuration of essential OData services.
    /// </summary>
    public class ODataBuilder : IODataBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataBuilder"/> class.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        public ODataBuilder(IServiceCollection serviceCollection)
        {
            if (serviceCollection == null)
            {
                throw Error.ArgumentNull("serviceCollection");
            }

            this.Services = serviceCollection;
        }

        /// <summary>
        /// Gets the services collection.
        /// </summary>
        public IServiceCollection Services { get; private set; }
    }
}
