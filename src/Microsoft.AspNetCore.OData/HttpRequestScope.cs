//-----------------------------------------------------------------------------
// <copyright file="HttpRequestScope.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Provides access to the <see cref="HttpRequest"/>
    /// to which the OData service container instance is scoped.
    /// </summary>
    public class HttpRequestScope
    {
        /// <summary>
        /// Provides access to the <see cref="HttpRequest"/>
        /// to which the OData service container instance is scoped.
        /// </summary>
        public HttpRequest HttpRequest { get; set; }
    }
}
