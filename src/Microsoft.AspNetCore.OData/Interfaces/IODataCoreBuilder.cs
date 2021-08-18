//-----------------------------------------------------------------------------
// <copyright file="IODataCoreBuilder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.OData.Interfaces
{
    /// <summary>
    /// An interface for configuring essential OData services.
    /// </summary>
    public interface IODataBuilder
    {
        /// <summary>
        /// Gets the <see cref="IServiceCollection"/> where essential OData services are configured.
        /// </summary>
        IServiceCollection Services { get; }
    }
}
