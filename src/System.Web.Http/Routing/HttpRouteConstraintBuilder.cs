// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Http.Routing.Constraints;

namespace System.Web.Http.Routing
{
    public static class HttpRouteConstraintBuilder
    {
        private static readonly Regex _routeConstraintKeyRegex = new Regex(@"HttpRouteConstraint$");
        private static readonly IDictionary<string, Type> _inlineRouteConstraintMap = GetDefaultInlineRouteConstraints();

        public static IHttpRouteConstraint BuildInlineRouteConstraint(string constraintKey, params object[] args)
        {
            // Make sure the key exists in the key/Type map.
            string upperCasedConstraintKey = constraintKey.ToUpperInvariant();
            if (!_inlineRouteConstraintMap.ContainsKey(upperCasedConstraintKey))
            {
                // TODO: Throw via the Error helper.
                throw new KeyNotFoundException(
                    Error.Format("Could not resolve a route constraint for the key '{0}'.", constraintKey));
            }

            Type httpRouteConstraintType = typeof(IHttpRouteConstraint);
            Type type = _inlineRouteConstraintMap[upperCasedConstraintKey];
            
            // Make sure the type is an IHttpRouteConstraint.
            if (!httpRouteConstraintType.IsAssignableFrom(type))
            {
                // TODO: Throw via Error helper.
                throw Error.InvalidOperation(
                    "The type '{0}' must implement '{1}'", type.FullName, httpRouteConstraintType.FullName);
            }

            return (IHttpRouteConstraint)Activator.CreateInstance(type, args);
        }

        private static IDictionary<string, Type> GetDefaultInlineRouteConstraints()
        {
            Dictionary<string, Type> defaultInlineRouteConstraints = new Dictionary<string, Type>();
            Type inlineRouteConstraintType = typeof(IInlineRouteConstraint);
            
            IEnumerable<Type> types = from t in typeof(IInlineRouteConstraint).Assembly.GetTypes()
                                      where !t.IsAbstract &&
                                            inlineRouteConstraintType.IsAssignableFrom(t)
                                      select t;

            foreach (var type in types)
            {
                var key = _routeConstraintKeyRegex.Replace(type.Name, String.Empty).ToUpperInvariant();
                defaultInlineRouteConstraints.Add(key, type);
            }
            return defaultInlineRouteConstraints;
        }
    }
}
