// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.ObjectModel;

namespace System.Web.Http.Description
{
    /// <summary>
    /// Defines the interface for getting a collection of <see cref="ApiDescription"/>.
    /// </summary>
    public interface IApiExplorer
    {
        /// <summary>
        /// Gets the API descriptions.
        /// </summary>
        Collection<ApiDescription> ApiDescriptions { get; }
    }
}
