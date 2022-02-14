//-----------------------------------------------------------------------------
// <copyright file="KeySegmentTemplate.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Routing.Template
{
    /// <summary>
    /// Represents a template that can match a <see cref="KeySegment"/>.
    /// </summary>
    public class KeySegmentTemplate : ODataPathSegmentTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeySegmentTemplate"/> class.
        /// </summary>
        /// <param name="segment">The key segment</param>
        public KeySegmentTemplate(KeySegment segment)
        {
            if (segment == null)
            {
                throw Error.ArgumentNull("segment");
            }

            Segment = segment;

            ParameterMappings = BuildKeyMappings(segment.Keys);
        }

        /// <summary>
        /// Gets or sets the key segment.
        /// </summary>
        public KeySegment Segment { get; set; }

        /// <summary>
        /// Gets the dictionary representing the mappings from the key names in the segment to the key names in route data.
        /// </summary>
        public IDictionary<string, string> ParameterMappings { get; private set; }

        /// <inheritdoc/>
        public override bool TryMatch(ODataPathSegment pathSegment, IDictionary<string, object> values)
        {
            KeySegment keySegment = pathSegment as KeySegment;
            if (keySegment != null)
            {
                return keySegment.TryMatch(ParameterMappings, values);
            }

            return false;
        }

        internal static IDictionary<string, string> BuildKeyMappings(IEnumerable<KeyValuePair<string, object>> keys)
        {
            Contract.Assert(keys != null);

            Dictionary<string, string> parameterMappings = new Dictionary<string, string>();

            foreach (KeyValuePair<string, object> key in keys)
            {
                string nameInRouteData;

                UriTemplateExpression uriTemplateExpression = key.Value as UriTemplateExpression;
                if (uriTemplateExpression != null)
                {
                    nameInRouteData = uriTemplateExpression.LiteralText.Trim();
                }
                else
                {
                    // just for easy construct the key segment template
                    // it must start with "{" and end with "}"
                    nameInRouteData = key.Value as string;
                }

                if (nameInRouteData == null || !RoutingConventionHelpers.IsRouteParameter(nameInRouteData))
                {
                    throw new ODataException(
                        Error.Format(SRResources.KeyTemplateMustBeInCurlyBraces, key.Value, key.Key));
                }

                nameInRouteData = nameInRouteData.Substring(1, nameInRouteData.Length - 2);
                if (String.IsNullOrEmpty(nameInRouteData))
                {
                    throw new ODataException(
                            Error.Format(SRResources.EmptyKeyTemplate, key.Value, key.Key));
                }

                parameterMappings[key.Key] = nameInRouteData;
            }

            return parameterMappings;
        }
    }
}
