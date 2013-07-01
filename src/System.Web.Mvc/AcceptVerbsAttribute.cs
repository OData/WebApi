// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Web.Mvc.Routing;

namespace System.Web.Mvc
{
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "The accessor is exposed as an ICollection<string>.")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class AcceptVerbsAttribute : ActionMethodSelectorAttribute, IDirectRouteInfoProvider
    {
        private readonly HttpVerbsValidator _httpVerbsValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="AcceptVerbsAttribute" /> class.
        /// </summary>
        /// <param name="verbs">The HTTP methods the action supports.</param>
        public AcceptVerbsAttribute(HttpVerbs verbs)
        {
            _httpVerbsValidator = new HttpVerbsValidator(verbs);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AcceptVerbsAttribute" /> class.
        /// </summary>
        /// <param name="verbs">The HTTP methods the action supports.</param>
        public AcceptVerbsAttribute(params string[] verbs)
        {
            _httpVerbsValidator = new HttpVerbsValidator(verbs);
        }

        /// <summary>
        /// Gets or sets the route template describing the URI pattern to match against.
        /// </summary>
        public string RouteTemplate { get; set; }

        /// <summary>
        /// Gets or sets the name of the route to generate for this action.
        /// </summary>
        public string RouteName { get; set; }

        /// <summary>
        /// Gets or sets the order of the route relative to other routes. The default order is 0.
        /// </summary>
        public int RouteOrder { get; set; }

        /// <inheritdoc />
        public ICollection<string> Verbs
        {
            get { return _httpVerbsValidator.Verbs; }
        }

        public override bool IsValidForRequest(ControllerContext controllerContext, MethodInfo methodInfo)
        {
            return _httpVerbsValidator.IsValidForRequest(controllerContext);
        }
    }
}
