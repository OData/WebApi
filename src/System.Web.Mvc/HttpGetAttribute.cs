// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    /// <summary>
    /// Specifies that an action supports the GET HTTP method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class HttpGetAttribute : HttpVerbAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpGetAttribute" /> class.
        /// </summary>
        public HttpGetAttribute()
            : base(HttpVerbs.Get)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpGetAttribute" /> class.
        /// </summary>
        /// <param name="routeTemplate">The route template describing the URI pattern to match against.</param>
        public HttpGetAttribute(string routeTemplate)
            : base(HttpVerbs.Get, routeTemplate)
        {
        }
    }
}
