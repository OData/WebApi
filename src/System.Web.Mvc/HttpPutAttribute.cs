// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    /// <summary>
    /// Specifies that an action supports the PUT HTTP method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class HttpPutAttribute : HttpVerbAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpPutAttribute" /> class.
        /// </summary>
        public HttpPutAttribute()
            : base(HttpVerbs.Put)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpPutAttribute" /> class.
        /// </summary>
        /// <param name="routeTemplate">The route template describing the URI pattern to match against.</param>
        public HttpPutAttribute(string routeTemplate)
            : base(HttpVerbs.Put, routeTemplate)
        {
        }
    }
}
