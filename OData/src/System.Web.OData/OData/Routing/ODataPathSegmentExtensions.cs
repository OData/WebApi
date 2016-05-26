// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Edm;

namespace System.Web.OData.Routing
{
    internal static class ODataPathSegmentExtensions
    {
        public static string TranslateKeyValueToString(this KeySegment segment)
        {
            Contract.Assert(segment != null);

            IEdmEntityType entityType = segment.EdmType as IEdmEntityType;
            Contract.Assert(entityType != null);

            var keys = segment.Keys.ToList();

            if (keys.Count < 2)
            {
                var keyValue = keys.First();
                bool isDeclaredKey = entityType.Key().Any(k => k.Name == keyValue.Key);

                // alternate keys are always using the "key=value"
                if (isDeclaredKey)
                {
                    return String.Join(
                        ",",
                        keys.Select(keyValuePair =>
                            TranslateKeySegmentValue(keyValuePair.Value)).ToArray());
                }
            }

            return String.Join(
                ",",
                keys.Select(keyValuePair =>
                    (keyValuePair.Key +
                     "=" +
                     TranslateKeySegmentValue(keyValuePair.Value))).ToArray());
        }

        public static IDictionary<string, string> TranslateKeyValueToDictionary(this KeySegment segment)
        {
            Contract.Assert(segment != null);

            return segment.Keys.ToDictionary(e => e.Key, e => TranslateKeySegmentValue(e.Value));
        }

        private static string TranslateKeySegmentValue(object value)
        {
            if (value == null)
            {
                throw Error.ArgumentNull("value");
            }

            UriTemplateExpression uriTemplateExpression = value as UriTemplateExpression;
            if (uriTemplateExpression != null)
            {
                return uriTemplateExpression.LiteralText;
            }

            ConstantNode constantNode = value as ConstantNode;
            if (constantNode != null)
            {
                ODataEnumValue enumValue = constantNode.Value as ODataEnumValue;
                if (enumValue != null)
                {
                    return ODataUriUtils.ConvertToUriLiteral(enumValue, ODataVersion.V4);
                }
            }

            return ODataUriUtils.ConvertToUriLiteral(value, ODataVersion.V4);
        }

        public static bool TryMatch(this KeySegment keySegment, IDictionary<string, string> mapping,
            IDictionary<string, object> values)
        {
            Contract.Assert(keySegment != null);
            Contract.Assert(mapping != null);
            Contract.Assert(values != null);

            if (keySegment.Keys.Count() != mapping.Count)
            {
                return false;
            }

            IEdmEntityType entityType = keySegment.EdmType as IEdmEntityType;
            Contract.Assert(entityType != null);

            Dictionary<string, object> routeData = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> key in keySegment.Keys)
            {
                string mappingName;
                if (!mapping.TryGetValue(key.Key, out mappingName))
                {
                    // mapping name is not found. it's not a match.
                    return false;
                }

                // get the key property from the entity type
                IEdmTypeReference typeReference;
                IEdmStructuralProperty keyProperty = entityType.Key().FirstOrDefault(k => k.Name == key.Key);

                if (keyProperty == null)
                {
                    // If it's alternate key.
                    keyProperty = entityType.Properties().OfType<IEdmStructuralProperty>().FirstOrDefault(p => p.Name == key.Key);
                }
                Contract.Assert(keyProperty != null);
                typeReference = keyProperty.Type;

                AddKeyValues(mappingName, key.Value, typeReference, routeData, routeData);
            }

            foreach (KeyValuePair<string, object> kvp in routeData)
            {
                values[kvp.Key] = kvp.Value;
            }

            return true;
        }

        public static void AddKeyValues(string name, object value, IEdmTypeReference edmTypeReference,
            IDictionary<string, object> routeValues,
            IDictionary<string, object> odataValues)
        {
            Contract.Assert(routeValues != null);
            Contract.Assert(odataValues != null);

            object routeValue = null;
            object odataValue = null;
            ConstantNode node = value as ConstantNode;
            if (node != null)
            {
                ODataEnumValue enumValue = node.Value as ODataEnumValue;
                if (enumValue != null)
                {
                    odataValue = new ODataParameterValue(enumValue, edmTypeReference);
                    routeValue = enumValue.Value;
                }
            }
            else
            {
                odataValue = new ODataParameterValue(value, edmTypeReference);
                routeValue = value;
            }

            // for without FromODataUri
            routeValues[name] = routeValue;

            // For FromODataUri
            string prefixName = ODataParameterValue.ParameterValuePrefix + name;
            odataValues[prefixName] = odataValue;
        }
    }
}
