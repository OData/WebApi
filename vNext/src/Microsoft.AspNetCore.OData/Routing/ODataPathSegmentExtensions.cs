// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.AspNetCore.OData.Common;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    /// Provides an object representation for an OData path with additional information about the EDM type and entity set for the path.
    /// </summary>
    internal static class ODataPathSegmentExtensions
    {
        public static string TranslatePathTemplateSegment(this PathTemplateSegment pathTemplatesegment, out string value)
        {
            if (pathTemplatesegment == null)
            {
                throw Error.ArgumentNull("pathTemplatesegment");
            }

            string pathTemplateSegmentLiteralText = pathTemplatesegment.LiteralText;
            if (pathTemplateSegmentLiteralText == null)
            {
                throw new ODataException(Error.Format(SRResources.InvalidAttributeRoutingTemplateSegment, String.Empty));
            }

            if (pathTemplateSegmentLiteralText.StartsWith("{", StringComparison.Ordinal)
                && pathTemplateSegmentLiteralText.EndsWith("}", StringComparison.Ordinal))
            {
                string[] keyValuePair = pathTemplateSegmentLiteralText.Substring(1,
                    pathTemplateSegmentLiteralText.Length - 2).Split(':');
                if (keyValuePair.Length != 2)
                {
                    throw new ODataException(Error.Format(
                        SRResources.InvalidAttributeRoutingTemplateSegment,
                        pathTemplateSegmentLiteralText));
                }
                value = "{" + keyValuePair[0] + "}";
                return keyValuePair[1];
            }

            value = String.Empty;
            return String.Empty;
        }

        #region Uri Literal

        public static string ToUriLiteral(this MetadataSegment segment)
        {
            if (segment == null)
            {
                throw Error.ArgumentNull("segment");
            }

            return ODataSegmentKinds.Metadata;
        }

        public static string ToUriLiteral(this ValueSegment segment)
        {
            if (segment == null)
            {
                throw Error.ArgumentNull("segment");
            }

            return ODataSegmentKinds.Value;
        }

        public static string ToUriLiteral(this BatchSegment segment)
        {
            if (segment == null)
            {
                throw Error.ArgumentNull("segment");
            }

            return ODataSegmentKinds.Batch;
        }

        public static string ToUriLiteral(this CountSegment segment)
        {
            if (segment == null)
            {
                throw Error.ArgumentNull("segment");
            }

            return ODataSegmentKinds.Count;
        }

        public static string ToUriLiteral(this TypeSegment segment)
        {
            if (segment == null)
            {
                throw Error.ArgumentNull("segment");
            }

            IEdmType elementType = segment.EdmType;
            if (segment.EdmType.TypeKind == EdmTypeKind.Collection)
            {
                elementType = ((IEdmCollectionType)segment.EdmType).ElementType.Definition;
            }

            return elementType.FullTypeName();
        }

        public static string ToUriLiteral(this SingletonSegment segment)
        {
            if (segment == null)
            {
                throw Error.ArgumentNull("segment");
            }

            return segment.Singleton.Name;
        }

        public static string ToUriLiteral(this PropertySegment segment)
        {
            if (segment == null)
            {
                throw Error.ArgumentNull("segment");
            }

            return segment.Property.Name;
        }

        public static string ToUriLiteral(this PathTemplateSegment segment)
        {
            if (segment == null)
            {
                throw Error.ArgumentNull("segment");
            }

            return segment.LiteralText;
        }

        public static string ToUriLiteral(this DynamicPathSegment segment)
        {
            if (segment == null)
            {
                throw Error.ArgumentNull("segment");
            }

            return segment.Identifier;
        }

        public static string ToUriLiteral(this NavigationPropertySegment segment)
        {
            if (segment == null)
            {
                throw Error.ArgumentNull("segment");
            }

            return segment.NavigationProperty.Name;
        }

        public static string ToUriLiteral(this NavigationPropertyLinkSegment segment)
        {
            if (segment == null)
            {
                throw Error.ArgumentNull("segment");
            }

            return segment.NavigationProperty.Name + "/$ref";
        }

        public static string ToUriLiteral(this KeySegment segment)
        {
            if (segment == null)
            {
                throw Error.ArgumentNull("segment");
            }

            return ConvertKeysToUriLiteral(segment.Keys, segment.EdmType);
        }

        public static string ToUriLiteral(this EntitySetSegment segment)
        {
            if (segment == null)
            {
                throw Error.ArgumentNull("segment");
            }

            return segment.EntitySet.Name;
        }

        public static string ToUriLiteral(this OperationSegment segment)
        {
            if (segment == null)
            {
                throw Error.ArgumentNull("segment");
            }

            IEdmAction action = segment.Operations.Single() as IEdmAction;

            if (action != null)
            {
                return action.FullName();
            }
            else
            {
                // Translate the nodes in ODL path to string literals as parameter of BoundFunctionPathSegment.
                Dictionary<string, string> parameterValues = segment.Parameters.ToDictionary(
                    parameterValue => parameterValue.Name,
                    parameterValue => TranslateNode(parameterValue.Value));

                IEdmFunction function = (IEdmFunction)segment.Operations.Single();

                IEnumerable<string> parameters = parameterValues.Select(v => String.Format(CultureInfo.InvariantCulture, "{0}={1}", v.Key, v.Value));
                return String.Format(CultureInfo.InvariantCulture, "{0}({1})", function.FullName(), String.Join(",", parameters));
            }
        }

        public static string ToUriLiteral(this OperationImportSegment segment)
        {
            if (segment == null)
            {
                throw Error.ArgumentNull("segment");
            }

            IEdmActionImport action = segment.OperationImports.Single() as IEdmActionImport;

            if (action != null)
            {
                return action.Name;
            }
            else
            {
                // Translate the nodes in ODL path to string literals as parameter of BoundFunctionPathSegment.
                Dictionary<string, string> parameterValues = segment.Parameters.ToDictionary(
                    parameterValue => parameterValue.Name,
                    parameterValue => TranslateNode(parameterValue.Value));

                // TODO: refactor the function literal for parameter alias
                IEdmFunctionImport function = (IEdmFunctionImport)segment.OperationImports.Single();

                IEnumerable<string> parameters = parameterValues.Select(v => String.Format(CultureInfo.InvariantCulture, "{0}={1}", v.Key, v.Value));
                return String.Format(CultureInfo.InvariantCulture, "{0}({1})", function.Name, String.Join(",", parameters));
            }
        }

        public static string ToUriLiteral(this UnresolvedPathSegment segment)
        {
            if (segment == null)
            {
                throw Error.ArgumentNull("segment");
            }

            return segment.SegmentValue;
        }

        private static string ConvertKeysToUriLiteral(IEnumerable<KeyValuePair<string, object>> keys, IEdmType edmType)
        {
            Contract.Assert(keys != null);

            IEdmEntityType entityType = edmType as IEdmEntityType;
            Contract.Assert(entityType != null);

            if (keys.Count() < 2)
            {
                var keyValue = keys.First();
                bool isDeclaredKey = entityType.Key().Any(k => k.Name == keyValue.Key);

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

        // Translate the object of key in ODL path to string literal.
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

        private static string TranslateNode(object node)
        {
            if (node == null)
            {
                throw Error.ArgumentNull("node");
            }

            ConstantNode constantNode = node as ConstantNode;
            if (constantNode != null)
            {
                UriTemplateExpression uriTemplateExpression = constantNode.Value as UriTemplateExpression;
                if (uriTemplateExpression != null)
                {
                    return uriTemplateExpression.LiteralText;
                }

                // Make the enum prefix free to work.
                ODataEnumValue enumValue = constantNode.Value as ODataEnumValue;
                if (enumValue != null)
                {
                    return ODataUriUtils.ConvertToUriLiteral(enumValue, ODataVersion.V4);
                }

                return constantNode.LiteralText;
            }

            ConvertNode convertNode = node as ConvertNode;
            if (convertNode != null)
            {
                return TranslateNode(convertNode.Source);
            }

            throw Error.NotSupported(
                SRResources.CannotRecognizeNodeType,
                typeof(ODataPathSegmentTranslator),
                node.GetType().FullName);
        }

        #endregion
    }
}
