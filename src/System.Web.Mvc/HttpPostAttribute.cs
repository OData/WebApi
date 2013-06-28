// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    /// <summary>
    /// Specifies that an action supports the POST HTTP method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class HttpPostAttribute : HttpVerbAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpPostAttribute" /> class.
        /// </summary>
        public HttpPostAttribute()
            : base(HttpVerbs.Post)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpPostAttribute" /> class.
        /// </summary>
        /// <param name="routeTemplate">The route template describing the URI pattern to match against.</param>
        public HttpPostAttribute(string routeTemplate)
            : base(HttpVerbs.Post, routeTemplate)
        {
        }
    }
}
