// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Http.Routing.Constraints;

namespace System.Web.Http.Routing
{
    public static class HttpRouteBuilder
    {
        private const string ParameterNameRegex = @"(?<parameterName>.+?)";
        private const string OptionalRegex = @"(?<optional>\?)?";
        private const string DefaultValueRegex = @"(=(?<defaultValue>.*?))?";
        private const string ConstraintRegex = @"(:(?<constraint>.*?(\(.*?\))?))*";
        private static readonly Regex _parameterRegex = new Regex("{" + ParameterNameRegex + OptionalRegex + DefaultValueRegex + ConstraintRegex + "}", RegexOptions.Compiled);

        public static IHttpRoute BuildHttpRoute(IHttpRouteProvider provider, string controllerName, string actionName)
        {
            HttpRouteValueDictionary defaults = new HttpRouteValueDictionary
            {
                { "controller", controllerName },
                { "action", actionName }
            };

            HttpRouteValueDictionary constraints = new HttpRouteValueDictionary
            {
                // TODO: Improve HTTP method constraint. Current implementation is very inefficient since it matches before running the constraint.
                { "httpMethod", new HttpMethodConstraint(provider.HttpMethods.ToArray()) }
            };

            string routeTemplate = ParseRouteTemplate(provider.RouteTemplate, defaults, constraints);

            return new HttpRoute(routeTemplate, defaults, constraints);
        }

        /// <summary>
        /// Parses defaults and constraints from the given route template and returns a detokenized route template.
        /// </summary>
        /// <param name="routeTemplate">The tokenized route template to parse.</param>
        /// <param name="defaults">Dictionary for collecting defaults parsed from the route template.</param>
        /// <param name="constraints">Dictionary for collecting constraints parsed from the route template.</param>
        /// <returns>The route template with all default and constraint definitions removed.</returns>
        private static string ParseRouteTemplate(string routeTemplate, HttpRouteValueDictionary defaults, HttpRouteValueDictionary constraints)
        {
            Contract.Assert(defaults != null);
            Contract.Assert(constraints != null);

            MatchCollection matches = _parameterRegex.Matches(routeTemplate);

            foreach (Match match in matches)
            {
                string parameterName = match.Groups["parameterName"].Value;
                bool isOptional = match.Groups["optional"].Success;
                Group defaultValueGroup = match.Groups["defaultValue"];

                if (defaultValueGroup.Success)
                {
                    defaults.AddIfNew(parameterName, defaultValueGroup.Value);
                }
                else if (isOptional)
                {
                    defaults.AddIfNew(parameterName, RouteParameter.Optional);
                }

                List<IHttpRouteConstraint> parameterConstraints = new List<IHttpRouteConstraint>();
                IInlineRouteConstraintResolver inlineConstraintResolver = new DefaultInlineRouteConstraintResolver();
                foreach (Capture constraintCapture in match.Groups["constraint"].Captures)
                {
                    IHttpRouteConstraint constraint = inlineConstraintResolver.ResolveConstraint(constraintCapture.Value);
                    parameterConstraints.Add(constraint);
                }

                if (parameterConstraints.Count > 0)
                {
                    IHttpRouteConstraint constraint = parameterConstraints.Count == 1 ? parameterConstraints[0] : new CompoundHttpRouteConstraint(parameterConstraints);

                    if (isOptional)
                    {
                        constraint = new OptionalHttpRouteConstraint(constraint);
                    }

                    constraints.AddIfNew(parameterName, constraint);
                }
            }                 
            
            return _parameterRegex.Replace(routeTemplate, @"{${parameterName}}");
        }
    }
}
