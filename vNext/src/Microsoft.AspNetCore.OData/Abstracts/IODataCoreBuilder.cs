// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.OData.Abstracts
{
    /// <summary>
    /// An interface for configuring essential OData services.
    /// </summary>
    public interface IODataCoreBuilder
    {
        /// <summary>
        /// Gets the <see cref="IServiceCollection"/> where essential OData services are configured.
        /// </summary>
        IServiceCollection Services { get; }
    }
}
