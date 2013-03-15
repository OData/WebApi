// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Http.Controllers;
using System.Web.Http.Routing.Constraints;

namespace System.Web.Http.Routing
{
    public class HttpRouteBuilder
    {
        private static readonly Regex _constraintDefinitionsRegex = new Regex(@"(\:(?<key>\w*)(\((?<parameters>.*?)\)(?=[\}:=?]))?)");

        public IHttpRoute BuildHttpRoute(IHttpRouteProvider provider, HttpActionDescriptor actionDescriptor)
        {
            var defaults = new Dictionary<string, object>
            {
                { "controller", actionDescriptor.ControllerDescriptor.ControllerName },
                { "action", actionDescriptor.ActionName }
            };

            var constraints = new Dictionary<string, object>
            {
                // TODO: Improve HTTP method constraint. Current implementation is very inefficient since it matches before running the constraint.
                { "methodConstraint", new HttpMethodConstraint(provider.HttpMethods.ToArray()) }
            };
            
            var routeTemplate = ParseRouteTemplate(provider.RouteTemplate, defaults, constraints);
            
            return new HttpRoute(routeTemplate, new HttpRouteValueDictionary(defaults), new HttpRouteValueDictionary(constraints));
        }

        /// <summary>
        /// Parses defaults and constraints from the given route template and returns a detokenized route template.
        /// </summary>
        /// <param name="routeTemplate">The tokenized route template to parse.</param>
        /// <param name="defaults">Dictionary for collecting defaults parsed from the route template.</param>
        /// <param name="constraints">Dictionary for collecting constraints parsed from the route template.</param>
        /// <returns>The route template with all default and constraint definitions removed.</returns>
        private static string ParseRouteTemplate(string routeTemplate, IDictionary<string, object> defaults, IDictionary<string, object> constraints)
        {
            if (String.IsNullOrWhiteSpace(routeTemplate))
            {
                return routeTemplate;
            }
            if (defaults == null)
            {
                throw new ArgumentNullException("defaults");
            }
            if (constraints == null)
            {
                throw new ArgumentNullException("constraints");
            }

            var cleanRouteTemplate = new StringBuilder();
            var parameterName = "";
            var constraintDefinitions = _constraintDefinitionsRegex.Matches(routeTemplate);

            for (var i = 0; i < routeTemplate.Length; i++)
            {
                var c = routeTemplate[i];
                
                if (c == '{')
                {
                    // Parse the parameter name:
                    // =========================
                    // - find the next token or closing brace,
                    // - take up to the found index,
                    // - parse the parameter name,
                    // - update the clean route template.
                    // - position the loop index and continue.

                    var iNextTokenOrClosingBrace = routeTemplate.IndexOfAny(new[] { ':', '=', '?', '}' }, i);
                    if (iNextTokenOrClosingBrace == -1)
                    {
                        // TODO: Throw malformed parameter exception
                    }

                    parameterName = routeTemplate.Substring(i + 1, iNextTokenOrClosingBrace - i - 1);
                    cleanRouteTemplate.AppendFormat("{{{0}", parameterName);
                    i = iNextTokenOrClosingBrace - 1;
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
                            var key = constraintDefinition.Groups["key"].Value;
                            
                            // Parse constraint parameters.
                            var parameters = new List<object>();
                            var parametersGroup = constraintDefinition.Groups["parameters"];
                            if (parametersGroup.Success)
                            {
                                var parametersValue = parametersGroup.Value;

                                // In the case of regex constraints, take the full parameter content as a single value.
                                // This ensures we don't get fooled by characters that are route template tokens: {}()=?.
                                // For all other constraint types, split the params on commas.
                                if (key.Equals("regex", StringComparison.OrdinalIgnoreCase))
                                {
                                    parameters.Add(parametersValue);
                                }
                                else
                                {
                                    var cleanParametersValue = parametersValue.Replace(" ", "").Trim(',');
                                    parameters.AddRange(cleanParametersValue.Split(','));
                                }
                            }

                            // TODO: Resolve the constraint type from the key.
                            // Build the constraint object.
                            var constraintType = typeof(RegexHttpRouteConstraint);
                            var constraint = (IHttpRouteConstraint)Activator.CreateInstance(constraintType, parameters.ToArray());
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

                    var iClosingBrace = routeTemplate.IndexOf('}', i);
                    if (iClosingBrace == -1)
                    {
                        // TODO: Throw malformed parameter exception
                    }

                    var value = routeTemplate.Substring(i + 1, iClosingBrace - i - 1);
                    defaults.AddIfNew(parameterName, value);
                    i = iClosingBrace - 1;
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
