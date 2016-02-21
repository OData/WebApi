// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Properties;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Core.UriParser.Visitors;
using Microsoft.OData.Edm;
using Semantic = Microsoft.OData.Core.UriParser.Semantic;
using System.Linq;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    /// Translator to convert an OData path segment to WebAPI path segment.
    /// </summary>
    internal class ODataPathSegmentTranslator : PathSegmentTranslator<IEnumerable<ODataPathSegment>>
    {
        private readonly IEdmModel _model;
        private readonly bool _enableUriTemplateParsing;
        private IDictionary<string, Semantic.SingleValueNode> _parameterAliasValueNodes;

        /// <summary>
        /// Translates an ODL path to Web API path.
        /// </summary>
        /// <param name="path">The ODL path to be translated.</param>
        /// <param name="model">The model used to translate.</param>
        /// <param name="unresolvedPathSegment">Unresolved path segment.</param>
        /// <param name="id">The key segment from $id.</param>
        /// <param name="enableUriTemplateParsing">Specifies the ODL path is template or not.</param>
        /// <param name="parameterAliasNodes">The parameter alias nodes info.</param>
        /// <returns>The translated Web API path.</returns>
        public static ODataPath TranslateODLPathToWebAPIPath(
            Semantic.ODataPath path,
            IEdmModel model,
            UnresolvedPathSegment unresolvedPathSegment,
            Semantic.KeySegment id,
            bool enableUriTemplateParsing,
            IDictionary<string, Semantic.SingleValueNode> parameterAliasNodes)
        {
            if (path == null)
            {
                throw Error.ArgumentNull("path");
            }
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }
            if (parameterAliasNodes == null)
            {
                throw Error.ArgumentNull("parameterAliasNodes");
            }

            IList<ODataPathSegment> segments = path.WalkWith(
                new ODataPathSegmentTranslator(model, enableUriTemplateParsing, parameterAliasNodes))
                .SelectMany(s => s).ToList();

            if (unresolvedPathSegment != null)
            {
                segments.Add(unresolvedPathSegment);
            }

            if (!enableUriTemplateParsing)
            {
                AppendIdForRef(segments, id);
            }

            ReverseRefPathSegmentAndKeyValuePathSegment(segments);
            ODataPath odataPath = new ODataPath(segments);
            odataPath.ODLPath = path;

            return odataPath;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataPathSegmentTranslator" /> class.
        /// </summary>
        /// <param name="model">The model used to parse the path.</param>
        /// <param name="enableUriTemplateParsing">Specifies parsing path template or not.</param>
        /// <param name="parameterAliasNodes">The parameter alias nodes info.</param>
        public ODataPathSegmentTranslator(
            IEdmModel model,
            bool enableUriTemplateParsing,
            IDictionary<string, Semantic.SingleValueNode> parameterAliasNodes)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            if (parameterAliasNodes == null)
            {
                throw Error.ArgumentNull("parameterAliasNodes");
            }

            _model = model;
            _enableUriTemplateParsing = enableUriTemplateParsing;
            _parameterAliasValueNodes = parameterAliasNodes;
        }

        /// <summary>
        /// Translate parameter alias node to corresponding single value node.
        /// </summary>
        /// <param name="node">The node to be translated.</param>
        /// <param name="parameterAliasNodes">The parameter alias node mapping.</param>
        /// <returns>The translated node.</returns>
        public static Semantic.SingleValueNode TranslateParameterAlias(
            Semantic.SingleValueNode node,
            IDictionary<string, Semantic.SingleValueNode> parameterAliasNodes)
        {
            if (node == null)
            {
                throw Error.ArgumentNull("node");
            }

            if (parameterAliasNodes == null)
            {
                throw Error.ArgumentNull("parameterAliasNodes");
            }

            Semantic.ParameterAliasNode parameterAliasNode = node as Semantic.ParameterAliasNode;

            if (parameterAliasNode == null)
            {
                return node;
            }

            Semantic.SingleValueNode singleValueNode;

            if (parameterAliasNodes.TryGetValue(parameterAliasNode.Alias, out singleValueNode) &&
                singleValueNode != null)
            {
                if (singleValueNode is Semantic.ParameterAliasNode)
                {
                    singleValueNode = TranslateParameterAlias(singleValueNode, parameterAliasNodes);
                }

                return singleValueNode;
            }

            // Parameter alias value is assumed to be null if it is not found.
            // Do not need to translate the parameter alias node from the query string
            // because this method only deals with the parameter alias node mapping from ODL parser.
            return null;
        }

        /// <summary>
        /// Translate a TypeSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated WebApi path segment</returns>
        public override IEnumerable<ODataPathSegment> Translate(Semantic.TypeSegment segment)
        {
            IEdmType elementType = segment.EdmType;
            if (segment.EdmType.TypeKind == EdmTypeKind.Collection)
            {
                elementType = ((IEdmCollectionType)segment.EdmType).ElementType.Definition;
            }

            if (elementType.TypeKind == EdmTypeKind.Entity)
            {
                yield return new CastPathSegment((IEdmEntityType)elementType);
            }
            else if (elementType.TypeKind == EdmTypeKind.Complex)
            {
                yield return new ComplexCastPathSegment((IEdmComplexType)elementType);
            }
        }

        /// <summary>
        /// Translate a NavigationPropertySegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated WebApi path segment.</returns>
        public override IEnumerable<ODataPathSegment> Translate(Semantic.NavigationPropertySegment segment)
        {
            yield return new NavigationPathSegment(segment.NavigationProperty);
        }

        /// <summary>
        /// Translate an EntitySetSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated WebApi path segment.</returns>
        public override IEnumerable<ODataPathSegment> Translate(Semantic.EntitySetSegment segment)
        {
            yield return new EntitySetPathSegment(segment.EntitySet);
        }

        /// <summary>
        /// Translate an SingletonSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated WebApi path segment.</returns>
        public override IEnumerable<ODataPathSegment> Translate(Semantic.SingletonSegment segment)
        {
            yield return new SingletonPathSegment(segment.Singleton);
        }

        /// <summary>
        /// Translate a KeySegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated WebApi path segment.</returns>
        public override IEnumerable<ODataPathSegment> Translate(Semantic.KeySegment segment)
        {
            yield return new KeyValuePathSegment(ConvertKeysToString(segment.Keys, _enableUriTemplateParsing));
        }

        /// <summary>
        /// Translate a PropertySegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated WebApi path segment.</returns>
        public override IEnumerable<ODataPathSegment> Translate(Semantic.PropertySegment segment)
        {
            yield return new PropertyAccessPathSegment(segment.Property);
        }

        /// <summary>
        /// Translate a OperationImportSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated WebApi path segment.</returns>
        public override IEnumerable<ODataPathSegment> Translate(Semantic.OperationImportSegment segment)
        {
            IEdmActionImport actionImport = segment.OperationImports.Single() as IEdmActionImport;

            if (actionImport != null)
            {
                yield return new UnboundActionPathSegment(actionImport);
            }
            else
            {
                // Translate the nodes in ODL path to string literals as parameter of UnboundFunctionPathSegment.
                Dictionary<string, string> parameterValues = segment.Parameters.ToDictionary(
                    parameterValue => parameterValue.Name,
                    parameterValue => TranslateNode(parameterValue.Value));

                yield return new UnboundFunctionPathSegment(
                    (IEdmFunctionImport)segment.OperationImports.Single(),
                    _model,
                    parameterValues);
            }
        }

        /// <summary>
        /// Translate a OperationSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated WebApi path segment.</returns>
        public override IEnumerable<ODataPathSegment> Translate(Semantic.OperationSegment segment)
        {
            IEdmAction action = segment.Operations.Single() as IEdmAction;

            if (action != null)
            {
                yield return new BoundActionPathSegment(action, _model);
            }
            else
            {
                // Translate the nodes in ODL path to string literals as parameter of BoundFunctionPathSegment.
                Dictionary<string, string> parameterValues = segment.Parameters.ToDictionary(
                    parameterValue => parameterValue.Name,
                    parameterValue => TranslateNode(parameterValue.Value));
                IEdmFunction function = (IEdmFunction)segment.Operations.Single();

                yield return new BoundFunctionPathSegment(function, _model, parameterValues);
            }
        }

        /// <summary>
        /// Translate an OpenPropertySegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated WebApi path segment.</returns>
        public override IEnumerable<ODataPathSegment> Translate(Semantic.OpenPropertySegment segment)
        {
            yield return new DynamicPropertyPathSegment(segment.PropertyName);
        }

        /// <summary>
        /// Translate a CountSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated WebApi path segment.</returns>
        public override IEnumerable<ODataPathSegment> Translate(Semantic.CountSegment segment)
        {
            yield return new CountPathSegment();
        }

        /// <summary>
        /// Visit a NavigationPropertyLinkSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated WebApi path segment.</returns>
        public override IEnumerable<ODataPathSegment> Translate(Semantic.NavigationPropertyLinkSegment segment)
        {
            yield return new NavigationPathSegment(segment.NavigationProperty);
            yield return new RefPathSegment();
        }

        /// <summary>
        /// Translate a ValueSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated WebApi path segment.</returns>
        public override IEnumerable<ODataPathSegment> Translate(Semantic.ValueSegment segment)
        {
            yield return new ValuePathSegment();
        }

        /// <summary>
        /// Translate a BatchSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated WebApi path segment.</returns>
        public override IEnumerable<ODataPathSegment> Translate(Semantic.BatchSegment segment)
        {
            yield return new BatchPathSegment();
        }

        /// <summary>
        /// Translate a BatchReferenceSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated WebApi path segment.</returns>
        public override IEnumerable<ODataPathSegment> Translate(Semantic.BatchReferenceSegment segment)
        {
            throw new ODataException(Error.Format(
                SRResources.TargetKindNotImplemented,
                typeof(ODataPathSegment).Name,
                typeof(Semantic.BatchReferenceSegment).Name));
        }

        /// <summary>
        /// Translate a MetadataSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated WebApi path segment.</returns>
        public override IEnumerable<ODataPathSegment> Translate(Semantic.MetadataSegment segment)
        {
            yield return new MetadataPathSegment();
        }

        /// <summary>
        /// Translate a PathTemplateSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated WebApi path segment.</returns>
        public override IEnumerable<ODataPathSegment> Translate(Semantic.PathTemplateSegment segment)
        {
            string value = String.Empty;
            switch (TranslatePathTemplateSegment(segment.LiteralText, out value))
            {
                case ODataSegmentKinds._DynamicProperty:
                    yield return new DynamicPropertyPathSegment(value);
                    break;
                default:
                    throw new ODataException(Error.Format(
                        SRResources.InvalidAttributeRoutingTemplateSegment,
                        segment.LiteralText));
            }
        }

        // We need to append the key value path segment from $id.
        private static void AppendIdForRef(IList<ODataPathSegment> segments, Semantic.KeySegment id)
        {
            if (id == null || !(segments.Last() is RefPathSegment))
            {
                return;
            }

            segments.Add(new KeyValuePathSegment(ConvertKeysToString(id.Keys, enableUriTemplateParsing: false)));
        }

        // We need to reverse the order of RefPathSegment and KeyValuePathSegment.
        // For uri ~/Customers(5)/Orders(10)/$ref,
        // the parsed result of ODataUriParser has NavigationPropertyLinkSegment followed by KeyValuePathSegment,
        // the corresponding order in WebAPI is NavigationPathSegment/KeyValuePathSegment/RefPathSegment.
        private static void ReverseRefPathSegmentAndKeyValuePathSegment(IList<ODataPathSegment> segments)
        {
            Contract.Assert(segments != null);

            if (segments.Count >= 2 &&
                segments[segments.Count - 2] is RefPathSegment &&
                segments[segments.Count - 1] is KeyValuePathSegment)
            {
                ODataPathSegment segment = segments[segments.Count - 2];
                segments[segments.Count - 2] = segments[segments.Count - 1];
                segments[segments.Count - 1] = segment;
            }
        }

        // Convert the objects of keys in ODL path to string literals.
        private static string ConvertKeysToString(
            IEnumerable<KeyValuePair<string, object>> keys,
            bool enableUriTemplateParsing)
        {
            Contract.Assert(keys != null);

            string value;
            if (keys.Count() < 2)
            {
                value = String.Join(
                    ",",
                    keys.Select(keyValuePair =>
                        TranslateKeySegmentValue(keyValuePair.Value, enableUriTemplateParsing)).ToArray());
            }
            else
            {
                value = String.Join(
                    ",",
                    keys.Select(keyValuePair =>
                        (keyValuePair.Key +
                        "=" +
                        TranslateKeySegmentValue(keyValuePair.Value, enableUriTemplateParsing))).ToArray());
            }

            return value;
        }

        // Translate the object of key in ODL path to string literal.
        private static string TranslateKeySegmentValue(object value, bool enableUriTemplateParsing)
        {
            if (value == null)
            {
                throw Error.ArgumentNull("value");
            }

            if (enableUriTemplateParsing)
            {
                Semantic.UriTemplateExpression uriTemplateExpression = value as Semantic.UriTemplateExpression;
                if (uriTemplateExpression != null)
                {
                    return uriTemplateExpression.LiteralText;
                }
            }

            Semantic.ConstantNode constantNode = value as Semantic.ConstantNode;
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

        // Translate literal test of pathTemplateSegment to segment type and segment name.
        private static string TranslatePathTemplateSegment(string pathTemplateSegmentLiteralText, out string value)
        {
            if (pathTemplateSegmentLiteralText == null)
            {
                throw Error.ArgumentNull("pathTemplateSegmentLiteralText");
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

        // Translate the node in ODL path to string literal.
        private string TranslateNode(object node)
        {
            if (node == null)
            {
                throw Error.ArgumentNull("node");
            }

            Semantic.ConstantNode constantNode = node as Semantic.ConstantNode;
            if (constantNode != null)
            {
                if (_enableUriTemplateParsing)
                {
                    Semantic.UriTemplateExpression uriTemplateExpression = constantNode.Value as Semantic.UriTemplateExpression;
                    if (uriTemplateExpression != null)
                    {
                        return uriTemplateExpression.LiteralText;
                    }
                }

                // Make the enum prefix free to work.
                ODataEnumValue enumValue = constantNode.Value as ODataEnumValue;
                if (enumValue != null)
                {
                    return ODataUriUtils.ConvertToUriLiteral(enumValue, ODataVersion.V4);
                }

                return constantNode.LiteralText;
            }

            Semantic.ConvertNode convertNode = node as Semantic.ConvertNode;
            if (convertNode != null)
            {
                return TranslateNode(convertNode.Source);
            }

            Semantic.ParameterAliasNode parameterAliasNode = node as Semantic.ParameterAliasNode;
            if (parameterAliasNode != null)
            {
                return TranslateParameterAlias(parameterAliasNode.Alias);
            }

            throw Error.NotSupported(
                SRResources.CannotRecognizeNodeType,
                typeof(ODataPathSegmentTranslator),
                node.GetType().FullName);
        }

        // Translate parameter alias to string literal by using the parameter alias node mapping from ODL parser.
        private string TranslateParameterAlias(string alias)
        {
            if (alias == null)
            {
                throw Error.ArgumentNull("alias");
            }

            Semantic.SingleValueNode singleValueNode;

            if (_parameterAliasValueNodes.TryGetValue(alias, out singleValueNode) && singleValueNode != null)
            {
                return TranslateNode(singleValueNode);
            }

            return null;
        }
    }
}