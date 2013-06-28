// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    /// <summary>
    /// Specifies that an action supports the DELETE HTTP method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class HttpDeleteAttribute : HttpVerbAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpDeleteAttribute" /> class.
        /// </summary>
        public HttpDeleteAttribute()
            : base(HttpVerbs.Delete)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpDeleteAttribute" /> class.
        /// </summary>
        /// <param name="routeTemplate">The route template describing the URI pattern to match against.</param>
        public HttpDeleteAttribute(string routeTemplate)
            : base(HttpVerbs.Delete, routeTemplate)
        {
        }
    }
}
