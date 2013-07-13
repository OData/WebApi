// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http.Controllers;
using System.Web.Http.Properties;

namespace System.Web.Http.Routing
{
    /// <summary>
    /// Represents a route that is able to directly select the actions that can be reached.
    /// </summary>
    public class HttpDirectRoute : HttpRoute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpDirectRoute" /> class.
        /// </summary>
        /// <param name="routeTemplate">The route template.</param>
        /// <param name="actions">The actions that are reachable via this route.</param>
        public HttpDirectRoute(string routeTemplate, IEnumerable<ReflectedHttpActionDescriptor> actions)
            : this(routeTemplate, defaults: null, constraints: null, actions: actions)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpDirectRoute" /> class.
        /// </summary>
        /// <param name="routeTemplate">The route template.</param>
        /// <param name="defaults">The default values.</param>
        /// <param name="constraints">The route constraints.</param>
        /// <param name="actions">The actions that are reachable via this route.</param>
        public HttpDirectRoute(
            string routeTemplate,
            HttpRouteValueDictionary defaults,
            HttpRouteValueDictionary constraints,
            IEnumerable<ReflectedHttpActionDescriptor> actions)
            : base(routeTemplate, defaults: defaults, constraints: constraints, dataTokens: null, handler: null)
        {
            if (actions != null)
            {
                Actions = actions.AsArray();
                DataTokens[RouteKeys.ActionsDataTokenKey] = Actions;
            }
        }

        /// <summary>
        /// Gets the actions that are reachable via this route.
        /// </summary>
        public IReadOnlyList<ReflectedHttpActionDescriptor> Actions { get; private set; }

        /// <inheritdoc />
        public override IHttpRouteData GetRouteData(string virtualPathRoot, HttpRequestMessage request)
        {
            IHttpRouteData routeData = base.GetRouteData(virtualPathRoot, request);

            // For exactly one action, make sure to add default params to the route values so that action selection and invocation can succeed
            // Model binding will recognize the null route value and convert it into the default value for the parameter type
            // This allows optional parameters to work in attribute routing without requiring users to specify a default value
            if (routeData != null && Actions.Count == 1)
            {
                IDictionary<string, object> routeValues = routeData.Values;
                foreach (KeyValuePair<string, object> defaultValue in Defaults)
                {
                    if (defaultValue.Value == RouteParameter.Optional)
                    {
                        object parsedValue;
                        if (!routeValues.TryGetValue(defaultValue.Key, out parsedValue) || parsedValue == RouteParameter.Optional)
                        {
                            routeValues[defaultValue.Key] = null;
                        }
                    }
                }
            }

            return routeData;
        }
    }
}
