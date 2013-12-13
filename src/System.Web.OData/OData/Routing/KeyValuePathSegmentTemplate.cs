// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Web.Http.OData.Properties;
using Microsoft.OData.Core;

namespace System.Web.Http.OData.Routing
{
    /// <summary>
    /// Represents a template that can match a <see cref="KeyValuePathSegment"/>.
    /// </summary>
    public class KeyValuePathSegmentTemplate : ODataPathSegmentTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValuePathSegmentTemplate"/> class.
        /// </summary>
        /// <param name="keyValueSegment">The key value segment to be parsed as a template.</param>
        public KeyValuePathSegmentTemplate(KeyValuePathSegment keyValueSegment)
        {
            if (keyValueSegment == null)
            {
                throw Error.ArgumentNull("keyValueSegment");
            }

            ParameterMappings = BuildParameterMappings(keyValueSegment.Values, keyValueSegment.Value);
        }

        /// <summary>
        /// Gets the dictionary representing the mappings from the key names in the segment to the key names in route data.
        /// </summary>
        public IDictionary<string, string> ParameterMappings { get; private set; }

        /// <inheritdoc />
        public override bool TryMatch(ODataPathSegment pathSegment, IDictionary<string, object> values)
        {
            if (pathSegment.SegmentKind == ODataSegmentKinds.Key)
            {
                KeyValuePathSegment keySegment = (KeyValuePathSegment)pathSegment;
                return TryMatch(ParameterMappings, keySegment.Values, values);
            }

            return false;
        }

        internal static bool TryMatch(IDictionary<string, string> parameterMappings,
            IDictionary<string, string> parameterValues, IDictionary<string, object> matches)
        {
            Contract.Assert(parameterMappings != null);
            Contract.Assert(parameterValues != null);
            Contract.Assert(matches != null);

            if (parameterMappings.Count != parameterValues.Count)
            {
                return false;
            }

            Dictionary<string, string> routeData = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> parameter in parameterMappings)
            {
                string nameInSegment = parameter.Key;
                string nameInRouteData = parameter.Value;

                string value;
                if (!parameterValues.TryGetValue(nameInSegment, out value))
                {
                    // parameter not found. not a match.
                    return false;
                }

                routeData.Add(nameInRouteData, value);
            }

            foreach (KeyValuePair<string, string> kvp in routeData)
            {
                matches[kvp.Key] = kvp.Value;
            }
            return true;
        }

        internal static IDictionary<string, string> BuildParameterMappings(IDictionary<string, string> parameters, string segment)
        {
            Contract.Assert(parameters != null);

            Dictionary<string, string> parameterMappings = new Dictionary<string, string>();

            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                string parameterName = parameter.Key;
                string parameterNameInRouteData = parameter.Value;
                parameterNameInRouteData = parameterNameInRouteData.Trim();

                if (String.IsNullOrEmpty(parameterNameInRouteData))
                {
                    parameterNameInRouteData = parameterName;
                }
                else if (IsRouteParameter(parameterNameInRouteData))
                {
                    parameterNameInRouteData = parameterNameInRouteData.Substring(1, parameterNameInRouteData.Length - 2);
                    if (String.IsNullOrEmpty(parameterNameInRouteData))
                    {
                        throw new ODataException(
                            Error.Format(SRResources.EmptyParameterAlias, parameter.Value, segment));
                    }
                }
                else
                {
                    throw new ODataException(
                        Error.Format(SRResources.ParameterAliasMustBeInCurlyBraces, parameter.Value, segment));
                }

                parameterMappings[parameter.Key] = parameterNameInRouteData;
            }

            return parameterMappings;
        }

        private static bool IsRouteParameter(string parameterName)
        {
            return parameterName.StartsWith("{", StringComparison.Ordinal) &&
                    parameterName.EndsWith("}", StringComparison.Ordinal);
        }
    }
}
