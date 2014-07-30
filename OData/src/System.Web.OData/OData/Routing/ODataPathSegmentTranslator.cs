// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http;
using System.Web.OData.Properties;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Core.UriParser.Visitors;
using Microsoft.OData.Edm;
using Semantic = Microsoft.OData.Core.UriParser.Semantic;

namespace System.Web.OData.Routing
{
    /// <summary>
    /// Translator to convert an OData path segment to WebAPI path segment.
    /// </summary>
    internal class ODataPathSegmentTranslator : PathSegmentTranslator<IEnumerable<ODataPathSegment>>
    {
        private readonly IEdmModel _model;
        private readonly bool _enableUriTemplateParsing;
        private IDictionary<string, SingleValueNode> _parameterAliasValueNodes;
        private NameValueCollection _queryString;

        /// <summary>
        /// Translates an ODL path to Web API path.
        /// </summary>
        /// <param name="path">The ODL path to be translated.</param>
        /// <param name="model">The model used to translate.</param>
        /// <param name="unresolvedPathSegment">Unresolved path segment.</param>
        /// <param name="id">The key segment from $id.</param>
        /// <param name="enableUriTemplateParsing">Specifies the ODL path is template or not.</param>
        /// <param name="parameterAliasNodes">The parameter alias nodes info.</param>
        /// <param name="queryString">The query string.</param>
        /// <returns>The translated Web API path.</returns>
        public static ODataPath TranslateODLPathToWebAPIPath(
            Semantic.ODataPath path,
            IEdmModel model,
            UnresolvedPathSegment unresolvedPathSegment,            
            KeySegment id,
            bool enableUriTemplateParsing,
            IDictionary<string, SingleValueNode> parameterAliasNodes,
            NameValueCollection queryString)
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
                new ODataPathSegmentTranslator(model, enableUriTemplateParsing, parameterAliasNodes, queryString))
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
        /// <param name="queryString">The query string.</param>
        public ODataPathSegmentTranslator(
            IEdmModel model,
            bool enableUriTemplateParsing,
            IDictionary<string, SingleValueNode> parameterAliasNodes,
            NameValueCollection queryString)
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
            _queryString = queryString;
        }

        /// <summary>
        /// Translate parameter alias node to corresponding single value node.
        /// </summary>
        /// <param name="node">The node to be translated.</param>
        /// <param name="parameterAliasNodes">The parameter alias node mapping.</param>
        /// <returns>The translated node.</returns>
        public static SingleValueNode TranslateParameterAlias(
            SingleValueNode node,
            IDictionary<string, SingleValueNode> parameterAliasNodes)
        {
            if (node == null)
            {
                throw Error.ArgumentNull("node");
            }

            if (parameterAliasNodes == null)
            {
                throw Error.ArgumentNull("parameterAliasNodes");
            }

            ParameterAliasNode parameterAliasNode = node as ParameterAliasNode;

            if (parameterAliasNode == null)
            {
                return node;
            }

            SingleValueNode singleValueNode;

            if (parameterAliasNodes.TryGetValue(parameterAliasNode.Alias, out singleValueNode) &&
                singleValueNode != null)
            {
                if (singleValueNode is ParameterAliasNode)
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
        public override IEnumerable<ODataPathSegment> Translate(TypeSegment segment)
        {
            if (segment.EdmType.TypeKind == EdmTypeKind.Entity)
            {
                yield return new CastPathSegment((IEdmEntityType)segment.EdmType);
            }
            else
            {
                yield return new CastPathSegment(
                    (IEdmEntityType)((IEdmCollectionType)segment.EdmType).ElementType.Definition);
            }
        }

        /// <summary>
        /// Translate a NavigationPropertySegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated WebApi path segment.</returns>
        public override IEnumerable<ODataPathSegment> Translate(NavigationPropertySegment segment)
        {
            yield return new NavigationPathSegment(segment.NavigationProperty);
        }

        /// <summary>
        /// Translate an EntitySetSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated WebApi path segment.</returns>
        public override IEnumerable<ODataPathSegment> Translate(EntitySetSegment segment)
        {
            yield return new EntitySetPathSegment(segment.EntitySet);
        }

        /// <summary>
        /// Translate an SingletonSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated WebApi path segment.</returns>
        public override IEnumerable<ODataPathSegment> Translate(SingletonSegment segment)
        {
            yield return new SingletonPathSegment(segment.Singleton);
        }

        /// <summary>
        /// Translate a KeySegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated WebApi path segment.</returns>
        public override IEnumerable<ODataPathSegment> Translate(KeySegment segment)
        {
            yield return new KeyValuePathSegment(ConvertKeysToString(segment.Keys, _enableUriTemplateParsing));
        }

        /// <summary>
        /// Translate a PropertySegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated WebApi path segment.</returns>
        public override IEnumerable<ODataPathSegment> Translate(PropertySegment segment)
        {
            yield return new PropertyAccessPathSegment(segment.Property);
        }

        /// <summary>
        /// Translate a OperationImportSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated WebApi path segment.</returns>
        public override IEnumerable<ODataPathSegment> Translate(OperationImportSegment segment)
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
        public override IEnumerable<ODataPathSegment> Translate(OperationSegment segment)
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
        public override IEnumerable<ODataPathSegment> Translate(OpenPropertySegment segment)
        {
            throw new ODataException(Error.Format(
                SRResources.TargetKindNotImplemented,
                typeof(Semantic.ODataPathSegment).Name,
                typeof(OpenPropertySegment).Name));
        }

        /// <summary>
        /// Translate a CountSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated WebApi path segment.</returns>
        public override IEnumerable<ODataPathSegment> Translate(CountSegment segment)
        {
            throw new ODataException(Error.Format(
                SRResources.TargetKindNotImplemented,
                typeof(Semantic.ODataPathSegment).Name, 
                typeof(CountSegment).Name));
        }

        /// <summary>
        /// Visit a NavigationPropertyLinkSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated WebApi path segment.</returns>
        public override IEnumerable<ODataPathSegment> Translate(NavigationPropertyLinkSegment segment)
        {
            yield return new NavigationPathSegment(segment.NavigationProperty);
            yield return new RefPathSegment();
        }

        /// <summary>
        /// Translate a ValueSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated WebApi path segment.</returns>
        public override IEnumerable<ODataPathSegment> Translate(ValueSegment segment)
        {
            yield return new ValuePathSegment();
        }

        /// <summary>
        /// Translate a BatchSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated WebApi path segment.</returns>
        public override IEnumerable<ODataPathSegment> Translate(BatchSegment segment)
        {
            yield return new BatchPathSegment();
        }

        /// <summary>
        /// Translate a BatchReferenceSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated WebApi path segment.</returns>
        public override IEnumerable<ODataPathSegment> Translate(BatchReferenceSegment segment)
        {
            throw new ODataException(Error.Format(
                SRResources.TargetKindNotImplemented,
                typeof(Semantic.ODataPathSegment).Name, 
                typeof(BatchReferenceSegment).Name));
        }

        /// <summary>
        /// Translate a MetadataSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated WebApi path segment.</returns>
        public override IEnumerable<ODataPathSegment> Translate(MetadataSegment segment)
        {
            yield return new MetadataPathSegment();
        }

        // We need to append the key value path segment from $id.
        private static void AppendIdForRef(IList<ODataPathSegment> segments, KeySegment id)
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
                UriTemplateExpression uriTemplateExpression = value as UriTemplateExpression;
                if (uriTemplateExpression != null)
                {
                    return uriTemplateExpression.LiteralText;
                }
            }

            return ODataUriUtils.ConvertToUriLiteral(value, ODataVersion.V4);
        }

        // Translate the node in ODL path to string literal.
        private string TranslateNode(object node)
        {
            if (node == null)
            {
                throw Error.ArgumentNull("node");
            }

            ConstantNode constantNode = node as ConstantNode;
            if (constantNode != null)
            {
                if (_enableUriTemplateParsing)
                {
                    UriTemplateExpression uriTemplateExpression = constantNode.Value as UriTemplateExpression;
                    if (uriTemplateExpression != null)
                    {
                        return uriTemplateExpression.LiteralText;
                    }
                }

                return constantNode.LiteralText;
            }

            ConvertNode convertNode = node as ConvertNode;
            if (convertNode != null)
            {
                return TranslateNode(convertNode.Source);
            }

            ParameterAliasNode parameterAliasNode = node as ParameterAliasNode;
            if (parameterAliasNode != null)
            {
                return TranslateParameterAlias(parameterAliasNode.Alias);
            }

            throw Error.NotSupported(
                SRResources.CannotRecognizeNodeType, 
                typeof(ODataPathSegmentTranslator),
                node.GetType().FullName);
        }

        // Translate parameter alias to string literal.
        // Use the query string to do the translataion if it is not null,
        // otherwise use the parameter alias node mapping from ODL parser.
        // The query string is not null if and only if there is some unresolved path segment.
        private string TranslateParameterAlias(string alias)
        {
            if (alias == null)
            {
                throw Error.ArgumentNull("alias");
            }

            if (_queryString != null)
            {
                string value = _queryString.Get(alias);

                if (value == null)
                {
                    return null;
                }

                value = value.Trim();

                if (value.StartsWith("@", StringComparison.Ordinal))
                {
                    return TranslateParameterAlias(value);
                }
                else
                {
                    return value;
                }
            }

            SingleValueNode singleValueNode;

            if (_parameterAliasValueNodes.TryGetValue(alias, out singleValueNode) && singleValueNode != null)
            {
                return TranslateNode(singleValueNode);
            }

            return null;
        }
    }
}