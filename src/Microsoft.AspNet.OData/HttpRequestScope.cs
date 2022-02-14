//-----------------------------------------------------------------------------
// <copyright file="HttpRequestScope.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
