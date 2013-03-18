// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Http.Properties;
using System.Web.Http.Routing.Constraints;

namespace System.Web.Http.Routing
{
    /// <summary>
    /// Builds <see cref="IHttpRoute"/> instances based on route information.
    /// </summary>
    public class HttpRouteBuilder
    {
        // One or more characters, matches "id"
        private const string ParameterNameRegex = @"(?<parameterName>.+?)";

        // Zero or more inline constraints that start with a colon followed by zero or more characters
        // Optionally the constraint can have arguments within parentheses - necessary to capture characters like ":" and "}"
        // Matches ":int", ":length(2)", ":regex(\})", ":regex(:)" zero or more times
        private const string ConstraintRegex = @"(:(?<constraint>.*?(\(.*?\))?))*";

        // Optional "?" for optional parameters or a default value with an equal sign followed by zero or more characters
        // Matches "?", "=", "=abc"
        private const string DefaultValueRegex = @"(?<defaultValue>\?|(=.*?))?";

        private static readonly Regex _parameterRegex = new Regex(
            "{" + ParameterNameRegex + ConstraintRegex + DefaultValueRegex + "}",
            RegexOptions.Compiled);

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRouteBuilder" /> class using the default inline constraint resolver.
        /// </summary>
        public HttpRouteBuilder()
            : this(new DefaultInlineConstraintResolver())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRouteBuilder" /> class.
        /// </summary>
        /// <param name="constraintResolver">The <see cref="IInlineConstraintResolver"/> to use for resolving inline constraints.</param>
        public HttpRouteBuilder(IInlineConstraintResolver constraintResolver)
        {
            if (constraintResolver == null)
            {
                throw Error.ArgumentNull("constraintResolver");
            }

            ConstraintResolver = constraintResolver;
        }

        public IInlineConstraintResolver ConstraintResolver { get; private set; }

        /// <summary>
        /// Builds an <see cref="IHttpRoute"/> based on the specified route info provider.
        /// </summary>
        /// <param name="provider">The provider used to determine the route name and the route template.</param>
        /// <param name="controllerName">The name of the associated controller.</param>
        /// <param name="actionName">The name of the associated action.</param>
        /// <returns>The generated <see cref="IHttpRoute"/>.</returns>
        public virtual IHttpRoute BuildHttpRoute(IHttpRouteInfoProvider provider, string controllerName, string actionName)
        {
            if (provider == null)
            {
                throw Error.ArgumentNull("provider");
            }

            if (controllerName == null)
            {
                throw Error.ArgumentNull("controllerName");
            }

            if (actionName == null)
            {
                throw Error.ArgumentNull("actionName");
            }

            HttpRouteValueDictionary defaults = new HttpRouteValueDictionary
            {
                { "controller", controllerName },
                { "action", actionName }
            };

            HttpRouteValueDictionary constraints = new HttpRouteValueDictionary
            {
                // Current method constraint implementation is inefficient since it matches before running the constraint.
                // Consider checking the HTTP method first in a custom route as a performance optimization.
                { "httpMethod", new HttpMethodConstraint(provider.HttpMethods.ToArray()) }
            };

            string routeTemplate = ParseRouteTemplate(provider.RouteTemplate, defaults, constraints);

            return BuildHttpRoute(defaults, constraints, routeTemplate);
        }

        /// <summary>
        /// Builds an <see cref="IHttpRoute"/>.
        /// </summary>
        /// <param name="defaults">The route defaults.</param>
        /// <param name="constraints">The route constraints.</param>
        /// <param name="routeTemplate">The detokenized route template.</param>
        /// <returns>The generated <see cref="IHttpRoute"/>.</returns>
        public virtual IHttpRoute BuildHttpRoute(HttpRouteValueDictionary defaults, HttpRouteValueDictionary constraints, string routeTemplate)
        {
            return new HttpRoute(routeTemplate, defaults, constraints);
        }

        private string ParseRouteTemplate(string routeTemplate, HttpRouteValueDictionary defaults, HttpRouteValueDictionary constraints)
        {
            Contract.Assert(defaults != null);
            Contract.Assert(constraints != null);

            MatchCollection parameterMatches = _parameterRegex.Matches(routeTemplate);

            foreach (Match parameterMatch in parameterMatches)
            {
                string parameterName = parameterMatch.Groups["parameterName"].Value;

                // Add the default value if present
                Group defaultValueGroup = parameterMatch.Groups["defaultValue"];
                object defaultValue = GetDefaultValue(defaultValueGroup);
                if (defaultValue != null)
                {
                    defaults.Add(parameterName, defaultValue);
                }

                // Register inline constraints if present
                Group constraintGroup = parameterMatch.Groups["constraint"];
                bool isOptional = defaultValue == RouteParameter.Optional;
                IHttpRouteConstraint constraint = GetInlineConstraint(constraintGroup, isOptional);
                if (constraint != null)
                {
                    constraints.Add(parameterName, constraint);
                }
            }                 
            
            // Replaces parameter matches with just the parameter name in braces
            // Strips out the optional '?', default value, inline constraints
            return _parameterRegex.Replace(routeTemplate, @"{${parameterName}}");
        }

        private static object GetDefaultValue(Group defaultValueGroup)
        {
            if (defaultValueGroup.Success)
            {
                string defaultValueMatch = defaultValueGroup.Value;
                if (defaultValueMatch == "?")
                {
                    return RouteParameter.Optional;
                }
                else
                {
                    // Strip out the equal sign at the beginning
                    Contract.Assert(defaultValueMatch.StartsWith("="));
                    return defaultValueMatch.Substring(1);
                }
            }
            return null;
        }

        private IHttpRouteConstraint GetInlineConstraint(Group constraintGroup, bool isOptional)
        {
            List<IHttpRouteConstraint> parameterConstraints = new List<IHttpRouteConstraint>();
            foreach (Capture constraintCapture in constraintGroup.Captures)
            {
                string inlineConstraint = constraintCapture.Value;
                IHttpRouteConstraint constraint = ConstraintResolver.ResolveConstraint(inlineConstraint);
                if (constraint == null)
                {
                    throw Error.InvalidOperation(SRResources.HttpRouteBuilder_CouldNotResolveConstraint, ConstraintResolver.GetType().Name, inlineConstraint);
                }
                parameterConstraints.Add(constraint);
            }

            if (parameterConstraints.Count > 0)
            {
                IHttpRouteConstraint constraint = parameterConstraints.Count == 1 ?
                    parameterConstraints[0] :
                    new CompoundHttpRouteConstraint(parameterConstraints);

                if (isOptional)
                {
                    // Constraints should match RouteParameter.Optional if the parameter is optional
                    // This prevents contraining when there's no value specified
                    constraint = new OptionalHttpRouteConstraint(constraint);
                }

                return constraint;
            }
            return null;
        }
    }
}
