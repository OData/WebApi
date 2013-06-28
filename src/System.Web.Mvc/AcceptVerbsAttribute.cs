// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc.Routing;

namespace System.Web.Mvc
{
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "The accessor is exposed as an ICollection<string>.")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class AcceptVerbsAttribute : HttpVerbsRoutingAttribute, IDirectRouteInfoProvider
    {
        private string _routeTemplate;

        /// <summary>
        /// Initializes a new instance of the <see cref="AcceptVerbsAttribute" /> class.
        /// </summary>
        /// <param name="verbs">The HTTP methods the action supports.</param>
        public AcceptVerbsAttribute(HttpVerbs verbs)
            : base(verbs)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AcceptVerbsAttribute" /> class.
        /// </summary>
        /// <param name="verbs">The HTTP methods the action supports.</param>
        public AcceptVerbsAttribute(params string[] verbs)
            : base(verbs)
        {
        }

        /// <summary>
        /// Gets or sets the route template describing the URI pattern to match against.
        /// </summary>
        public string RouteTemplate
        {
            get
            {
                return _routeTemplate;
            }
            set
            {
                ValidateRouteTemplateProperty(value);
                _routeTemplate = value;
            }
        }
    }
}
