// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;

namespace Microsoft.AspNet.OData.Batch
{
    /// <summary>
    /// A class for storing batch route names and prefixes used to determine if a route is a
    /// batch route.
    /// </summary>
    public class ODataBatchPathMapping
    {
        private Dictionary<TemplateMatcher, string> templateMappings = new Dictionary<TemplateMatcher, string>();

        /// <summary>
        /// Gets/sets a boolean value indicating whether it's endpoint routing.
        /// </summary>
        internal bool IsEndpointRouting { get; set; } = false;

        /// <summary>
        /// Add a route name and template for batching.
        /// </summary>
        /// <param name="routeName">The route name.</param>
        /// <param name="routeTemplate">The route template.</param>
        public void AddRoute(string routeName, string routeTemplate)
        {
            string newRouteTemplate = routeTemplate.StartsWith("/") ? routeTemplate.Substring(1) : routeTemplate;
            RouteTemplate parsedTemplate = TemplateParser.Parse(newRouteTemplate);
            TemplateMatcher matcher = new TemplateMatcher(parsedTemplate, new RouteValueDictionary());
            templateMappings[matcher] = routeName;
        }

        /// <summary>
        /// Try and get the batch handler for a given path.
        /// </summary>
        /// <param name="context">The http context.</param>
        /// <param name="routeName">The route name if found or null.</param>
        /// <returns>true if a route name is found, otherwise false.</returns>
        public bool TryGetRouteName(HttpContext context, out string routeName)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            routeName = null;
            string path = context.Request.Path;
            foreach (var item in templateMappings)
            {
                RouteValueDictionary routeData = new RouteValueDictionary();
                if (item.Key.TryMatch(path, routeData))
                {
                    routeName = item.Value;
                    if (routeData.Count > 0)
                    {
                        context.ODataFeature().BatchRouteData = routeData;
                    }

                    return true;
                }
            }

            return false;
        }
    }
}
