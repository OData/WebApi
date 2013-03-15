// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Http.Routing.Constraints;

namespace System.Web.Http.Routing
{
    public static class HttpRouteBuilder
    {
        private static readonly Regex _constraintDefinitionsRegex = new Regex(@"(\:(?<key>\w*)(\((?<parameters>.*?)\)(?=[\}:=?]))?)");

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
                { "methodConstraint", new HttpMethodConstraint(provider.HttpMethods.ToArray()) }
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

            if (String.IsNullOrWhiteSpace(routeTemplate))
            {
                return routeTemplate;
            }

            StringBuilder cleanRouteTemplate = new StringBuilder();
            string parameterName = String.Empty;
            MatchCollection constraintDefinitions = _constraintDefinitionsRegex.Matches(routeTemplate);

            for (int i = 0; i < routeTemplate.Length; i++)
            {
                char c = routeTemplate[i];
                
                if (c == '{')
                {
                    // Parse the parameter name:
                    // =========================
                    // - find the next token or closing brace,
                    // - take up to the found index,
                    // - parse the parameter name,
                    // - update the clean route template.
                    // - position the loop index and continue.

                    int indexOfNextTokenOrClosingBrace = routeTemplate.IndexOfAny(new[] { ':', '=', '?', '}' }, i);
                    if (indexOfNextTokenOrClosingBrace == -1)
                    {
                        // TODO: Throw malformed parameter exception
                    }

                    parameterName = routeTemplate.Substring(i + 1, indexOfNextTokenOrClosingBrace - i - 1);
                    cleanRouteTemplate.AppendFormat("{{{0}", parameterName);
                    i = indexOfNextTokenOrClosingBrace - 1;
                    continue;
                }
                
                if (c == ':')
                {
                    // Parse the constraint:
                    // =====================
                    // - get the constraint definition from the collection of constraints parsed from the route template,
                    // - parse the constraint key and any parameters,
                    // - build a constraint from the key and params,
                    // - repeat previous steps until all chained constraints are processed,
                    // - add a constraint for the current parameter,
                    // - position the loop and continue.

                    var parameterConstraints = new Collection<IHttpRouteConstraint>();
                    foreach (Match constraintDefinition in constraintDefinitions)
                    {
                        if (constraintDefinition.Index < i)
                        {
                            continue;
                        }

                        if (constraintDefinition.Index > i)
                        {
                            break;
                        }
                        
                        if (constraintDefinition.Index == i)
                        {
                            string constraintKey = constraintDefinition.Groups["key"].Value;
                            
                            // Parse constraint parameters.
                            List<object> parameters = new List<object>();
                            Group parametersGroup = constraintDefinition.Groups["parameters"];
                            if (parametersGroup.Success)
                            {
                                string parametersValue = parametersGroup.Value;

                                // In the case of regex constraints, take the full parameter content as a single value.
                                // This ensures we don't get fooled by characters that are route template tokens: {}()=?.
                                // For all other constraint types, split the params on commas.
                                if (constraintKey.Equals("regex", StringComparison.OrdinalIgnoreCase))
                                {
                                    parameters.Add(parametersValue);
                                }
                                else
                                {
                                    string cleanParametersValue = parametersValue.Replace(" ", null).Trim(',');
                                    parameters.AddRange(cleanParametersValue.Split(','));
                                }
                            }

                            // Build the constraint object.
                            IHttpRouteConstraint constraint = HttpRouteConstraintBuilder.BuildInlineRouteConstraint(constraintKey, parameters.ToArray());
                            parameterConstraints.Add(constraint);

                            // Advance the outer loop index just beyond the current constraint definition.
                            // This will setup the check for a chained constraint, which will begin just after this one ends.
                            i = constraintDefinition.Index + constraintDefinition.Length;
                        }
                    } // ... next constraint definition

                    // We're done processing constraints for this parameter.
                    // Wrap compound and/or optional constraints.
                    if (parameterConstraints.Count > 0)
                    {
                        IHttpRouteConstraint constraint;
                        
                        if (parameterConstraints.Count == 1)
                        {
                            constraint = parameterConstraints[0];
                        }
                        else
                        {
                            constraint = new CompoundHttpRouteConstraint(parameterConstraints);
                        }

                        if (routeTemplate[i] == '?')
                        {
                            constraint = new OptionalHttpRouteConstraint(constraint);
                        }

                        constraints.AddIfNew(parameterName, constraint);
                    }

                    // Need to adjust outer loop index so that it picks up the character following the constraints.
                    i = i - 1;
                    continue;
                }

                if (c == '=')
                {
                    // Parse the default value:
                    // ========================
                    // - find the closing brace,
                    // - take everything to the brace,
                    // - add a default for the current parameter,
                    // - position the loop index and continue.

                    int indexOfClosingBrace = routeTemplate.IndexOf('}', i);
                    if (indexOfClosingBrace == -1)
                    {
                        // TODO: Throw malformed parameter exception
                    }

                    string value = routeTemplate.Substring(i + 1, indexOfClosingBrace - i - 1);
                    defaults.AddIfNew(parameterName, value);
                    i = indexOfClosingBrace - 1;
                    continue;
                }
                
                if (c == '?')
                {
                    // Parse the optional token:
                    // =========================
                    // - add a default for the current parameter,
                    // - continue to the next char in the route template.

                    defaults.AddIfNew(parameterName, RouteParameter.Optional);
                    continue;
                }
                
                cleanRouteTemplate.Append(c.ToString());
            }

            return cleanRouteTemplate.ToString();
        }
    }
}
