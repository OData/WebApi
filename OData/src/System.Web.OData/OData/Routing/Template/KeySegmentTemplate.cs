// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http;
using System.Web.OData.Properties;
using System.Web.OData.Routing.Conventions;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser.Semantic;

namespace System.Web.OData.Routing.Template
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
                return RoutingConventionHelpers.TryMatch(ParameterMappings,
                    keySegment.Keys.ToDictionary(e => e.Key, e => TranslateKeyValue(e.Value)), values);
            }

            return false;
        }

        private static object TranslateKeyValue(object value)
        {
            Contract.Assert(value != null);

            UriTemplateExpression uriTemplateExpression = value as UriTemplateExpression;
            if (uriTemplateExpression != null)
            {
                return uriTemplateExpression.LiteralText.Trim();
            }

            ConstantNode constantNode = value as ConstantNode;
            if (constantNode != null)
            {
                ODataEnumValue enumValue = constantNode.Value as ODataEnumValue;
                if (enumValue != null)
                {
                    return enumValue.Value;
                }

                return constantNode.Value;
            }

            return value;
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
