// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net.Http;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Provides access to the <see cref="HttpRequestMessage"/>
    /// to which the OData service container instance is scoped.
    /// </summary>
    public class HttpRequestScope
    {
        /// <summary>
        /// Provides access to the <see cref="HttpRequestMessage"/>
        /// to which the OData service container instance is scoped.
        /// </summary>
        public HttpRequestMessage HttpRequest { get; set; }
    }
}
