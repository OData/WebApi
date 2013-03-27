// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;

namespace System.Web.Http
{
    /// <summary>
    /// Specifies that an action supports the PATCH HTTP method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class HttpPatchAttribute : HttpVerbAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpPatchAttribute" /> class.
        /// </summary>
        public HttpPatchAttribute()
            : base(new HttpMethod("PATCH"))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpPatchAttribute" /> class.
        /// </summary>
        /// <param name="routeTemplate">The route template describing the URI pattern to match against.</param>
        public HttpPatchAttribute(string routeTemplate)
            : base(new HttpMethod("PATCH"), routeTemplate)
        {
        }
    }
}
