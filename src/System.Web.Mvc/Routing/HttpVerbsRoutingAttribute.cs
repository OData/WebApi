// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web.Mvc.Properties;

namespace System.Web.Mvc.Routing
{
    /// <summary>
    /// Basic implementation for attributes used for defining routes on an action, optionally specifying that the action supports particular HTTP methods.
    /// </summary>
    public abstract class HttpVerbsRoutingAttribute : ActionMethodSelectorAttribute
    {
        private readonly ICollection<string> _verbs;
        private static readonly ConcurrentDictionary<HttpVerbs, ReadOnlyCollection<string>> _verbsToVerbCollections = new ConcurrentDictionary<HttpVerbs, ReadOnlyCollection<string>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpVerbsRoutingAttribute" /> class.
        /// </summary>
        protected HttpVerbsRoutingAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpVerbsRoutingAttribute" /> class.
        /// </summary>
        /// <param name="verbs">The HTTP methods the action supports.</param>
        protected HttpVerbsRoutingAttribute(HttpVerbs verbs)
            : this(ConvertVerbs(verbs))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpVerbsRoutingAttribute" /> class.
        /// </summary>
        /// <param name="verbs">The HTTP methods the action supports.</param>
        protected HttpVerbsRoutingAttribute(params string[] verbs)
            : this((IList<string>)verbs)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpVerbsRoutingAttribute" /> class.
        /// </summary>
        /// <param name="verbs">The HTTP methods the action supports.</param>
        private HttpVerbsRoutingAttribute(IList<string> verbs)
        {
            ValidateVerbs(verbs);
            _verbs = verbs as ReadOnlyCollection<string>;
            if (_verbs == null)
            {
                _verbs = new ReadOnlyCollection<string>(verbs);
            }
        }

        /// <summary>
        /// Gets or sets the name of the route to generate for this action.
        /// </summary>
        public string RouteName { get; set; }

        /// <summary>
        /// Gets or sets the order of the route relative to other routes. The default order is 0.
        /// </summary>
        public int RouteOrder { get; set; }

        /// <summary>
        /// Gets the set of allowed HTTP methods for that route. If the route allow any method to be used, the value is null.
        /// </summary>
        public ICollection<string> Verbs
        {
            get { return _verbs; }
        }

        /// <inheritdoc />
        public override bool IsValidForRequest(ControllerContext controllerContext, MethodInfo methodInfo)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }

            if (_verbs == null)
            {
                return true;
            }

            string incomingVerb = controllerContext.HttpContext.Request.GetHttpMethodOverride();

            return _verbs.Contains(incomingVerb, StringComparer.OrdinalIgnoreCase);
        }

        private static ReadOnlyCollection<string> ConvertVerbs(HttpVerbs verbs)
        {
            ReadOnlyCollection<string> verbsAsCollection;
            if (_verbsToVerbCollections.TryGetValue(verbs, out verbsAsCollection))
            {
                return verbsAsCollection;
            }

            verbsAsCollection = new ReadOnlyCollection<string>(EnumToArray(verbs));
            _verbsToVerbCollections.TryAdd(verbs, verbsAsCollection);
            return verbsAsCollection;
        }

        private static void ValidateVerbs(ICollection<string> verbs)
        {
            if (verbs == null || verbs.Count == 0)
            {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "verbs");
            }
        }

        internal static string[] EnumToArray(HttpVerbs verbs)
        {
            List<string> verbList = new List<string>();

            AddEntryToList(verbs, HttpVerbs.Get, verbList, "GET");
            AddEntryToList(verbs, HttpVerbs.Post, verbList, "POST");
            AddEntryToList(verbs, HttpVerbs.Put, verbList, "PUT");
            AddEntryToList(verbs, HttpVerbs.Delete, verbList, "DELETE");
            AddEntryToList(verbs, HttpVerbs.Head, verbList, "HEAD");
            AddEntryToList(verbs, HttpVerbs.Patch, verbList, "PATCH");
            AddEntryToList(verbs, HttpVerbs.Options, verbList, "OPTIONS");

            return verbList.ToArray();
        }

        private static void AddEntryToList(HttpVerbs verbs, HttpVerbs match, List<string> verbList, string entryText)
        {
            if ((verbs & match) != 0)
            {
                Contract.Assert(verbList != null);
                verbList.Add(entryText);
            }
        }

        protected static void ValidateRouteTemplateProperty(string routeTemplate)
        {
            ValidateRouteTemplate(routeTemplate, "RouteTemplate");
        }

        protected static void ValidateRouteTemplateArgument(string routeTemplate)
        {
            ValidateRouteTemplate(routeTemplate, "routeTemplate");
        }

        private static void ValidateRouteTemplate(string routeTemplate, string routeTemplateArgumentName)
        {
            if (routeTemplate == null)
            {
                throw new ArgumentNullException(routeTemplateArgumentName);
            }

            if (routeTemplate.StartsWith("/", StringComparison.Ordinal) || routeTemplate.EndsWith("/", StringComparison.Ordinal))
            {
                string errorMessage = String.Format(CultureInfo.CurrentCulture, MvcResources.RouteTemplate_CannotStartOrEnd_WithForwardSlash, routeTemplate);
                throw new ArgumentException(errorMessage, routeTemplateArgumentName);
            }
        }
    }
}