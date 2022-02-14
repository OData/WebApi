//-----------------------------------------------------------------------------
// <copyright file="ODataPathSegmentTranslator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Semantic = Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Routing
{
    /// <summary>
    /// Translator the parameter alias, convert node, returned entity set into OData path segment.
    /// </summary>
    public class ODataPathSegmentTranslator : PathSegmentTranslator<ODataPathSegment>
    {
        /// <summary>
        /// Translate the parameter alias, convert node, returned entity set into OData path segment.
        /// </summary>
        /// <param name="model">The EDM model</param>
        /// <param name="path">The odata path segments</param>
        /// <param name="parameterAliasNodes">The parameter alias</param>
        /// <returns>The translated odata path segments.</returns>
        public static IEnumerable<ODataPathSegment> Translate(IEdmModel model, Semantic.ODataPath path,
            IDictionary<string, SingleValueNode> parameterAliasNodes)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            if (path == null)
            {
                throw Error.ArgumentNull("path");
            }

            var translator = new ODataPathSegmentTranslator(model, parameterAliasNodes);
            return path.WalkWith(translator);
        }

        private readonly IDictionary<string, SingleValueNode> _parameterAliasNodes;
        private readonly IEdmModel _model;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataPathSegmentTranslator" /> class.
        /// </summary>
        /// <param name="model">The model used to parse the path.</param>
        /// <param name="parameterAliasNodes">The parameter alias nodes info.</param>
        public ODataPathSegmentTranslator(IEdmModel model, IDictionary<string, SingleValueNode> parameterAliasNodes)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            _model = model;
            _parameterAliasNodes = parameterAliasNodes ?? new Dictionary<string, SingleValueNode>();
        }

        /// <summary>
        /// Translate a TypeSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated odata path segment</returns>
        public override ODataPathSegment Translate(TypeSegment segment)
        {
            return segment;
        }

        /// <summary>
        /// Translate an EntitySetSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated odata path segment.</returns>
        public override ODataPathSegment Translate(EntitySetSegment segment)
        {
            return segment;
        }

        /// <summary>
        /// Translate an SingletonSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated odata path segment.</returns>
        public override ODataPathSegment Translate(SingletonSegment segment)
        {
            return segment;
        }

        /// <summary>
        /// Translate a PropertySegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated odata path segment.</returns>
        public override ODataPathSegment Translate(PropertySegment segment)
        {
            return segment;
        }

        /// <summary>
        /// Translate an OpenPropertySegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated odata path segment.</returns>
        public override ODataPathSegment Translate(DynamicPathSegment segment)
        {
            return segment;
        }

        /// <summary>
        /// Translate a CountSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated odata path segment.</returns>
        public override ODataPathSegment Translate(CountSegment segment)
        {
            return segment;
        }

        /// <summary>
        /// Translate a NavigationPropertySegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated odata path segment.</returns>
        public override ODataPathSegment Translate(NavigationPropertySegment segment)
        {
            return segment;
        }

        /// <summary>
        /// Visit a NavigationPropertyLinkSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated odata path segment.</returns>
        public override ODataPathSegment Translate(NavigationPropertyLinkSegment segment)
        {
            return segment;
        }

        /// <summary>
        /// Translate a ValueSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated odata path segment.</returns>
        public override ODataPathSegment Translate(ValueSegment segment)
        {
            return segment;
        }

        /// <summary>
        /// Translate a BatchSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated odata path segment.</returns>
        public override ODataPathSegment Translate(BatchSegment segment)
        {
            return segment;
        }

        /// <summary>
        /// Translate a MetadataSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated odata path segment.</returns>
        public override ODataPathSegment Translate(MetadataSegment segment)
        {
            return segment;
        }

        /// <summary>
        /// Translate a PathTemplateSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated odata path segment.</returns>
        public override ODataPathSegment Translate(PathTemplateSegment segment)
        {
            return segment;
        }

        /// <summary>
        /// Translate a KeySegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated odata path segment.</returns>
        public override ODataPathSegment Translate(KeySegment segment)
        {
            return segment;

            /*
            KeySegment newKeySegment = segment;
            if (_enableUriTemplate)
            {
                var newKeys =
                    segment.Keys.Select(e => new KeyValuePair<string, object>(e.Key, TranslateKeyValue(e.Value)));
                newKeySegment = new KeySegment(newKeys, (IEdmEntityType)segment.EdmType, segment.NavigationSource);
            }

            return newKeySegment;*/
        }
        /*
        private static object TranslateKeyValue(object value)
        {
            UriTemplateExpression uriTemplateExpression = value as UriTemplateExpression;
            if (uriTemplateExpression != null)
            {
                return uriTemplateExpression.LiteralText;
            }

            return value;
        }*/

        /// <summary>
        /// Translate a OperationImportSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated odata path segment.</returns>
        public override ODataPathSegment Translate(OperationImportSegment segment)
        {
            IEdmActionImport actionImport = segment.OperationImports.Single() as IEdmActionImport;

            if (actionImport != null)
            {
                return segment;
            }

            OperationImportSegment newSegment = segment;
            if (segment.Parameters.Any(p => p.Value is ParameterAliasNode || p.Value is ConvertNode))
            {
                var newParameters =
                    segment.Parameters.Select(e => new OperationSegmentParameter(e.Name, TranslateNode(e.Value)));
                newSegment = new OperationImportSegment(segment.OperationImports, segment.EntitySet, newParameters);
            }

            return newSegment;
        }

        /// <summary>
        /// Translate a OperationSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated odata path segment.</returns>
        public override ODataPathSegment Translate(OperationSegment segment)
        {
            Contract.Assert(segment != null);

            IEdmFunction function = segment.Operations.Single() as IEdmFunction;

            if (function != null)
            {
                OperationSegment newSegment = segment;
                if (segment.Parameters.Any(p => p.Value is ParameterAliasNode || p.Value is ConvertNode))
                {
                    var newParameters =
                        segment.Parameters.Select(e => new OperationSegmentParameter(e.Name, TranslateNode(e.Value)));
                    newSegment = new OperationSegment(segment.Operations, newParameters, segment.EntitySet);
                }

                segment = newSegment;
            }

            // Try to use the entity set annotation to get the target navigation source.
            ReturnedEntitySetAnnotation entitySetAnnotation =
                _model.GetAnnotationValue<ReturnedEntitySetAnnotation>(segment.Operations.Single());

            IEdmEntitySet returnedEntitySet = null;
            if (entitySetAnnotation != null)
            {
                returnedEntitySet = _model.EntityContainer.FindEntitySet(entitySetAnnotation.EntitySetName);
            }

            if (returnedEntitySet != null)
            {
                segment = new OperationSegment(segment.Operations, segment.Parameters, returnedEntitySet);
            }

            return segment;
        }

        private object TranslateNode(object node)
        {
            if (node == null)
            {
                throw Error.ArgumentNull("node");
            }

            ConstantNode constantNode = node as ConstantNode;
            if (constantNode != null)
            {
                return constantNode;
            }

            ConvertNode convertNode = node as ConvertNode;
            if (convertNode != null)
            {
                object value = TranslateNode(convertNode.Source);
                return ConvertNode(value, convertNode.TypeReference);
            }

            ParameterAliasNode parameterAliasNode = node as ParameterAliasNode;
            if (parameterAliasNode != null)
            {
                SingleValueNode singleValueNode;

                if (_parameterAliasNodes.TryGetValue(parameterAliasNode.Alias, out singleValueNode) && singleValueNode != null)
                {
                    return TranslateNode(singleValueNode);
                }

                // if not found the parameter alias, return null
                return null;
            }

            throw Error.NotSupported(SRResources.CannotRecognizeNodeType, typeof(ODataPathSegmentTranslator),
                node.GetType().FullName);
        }

        private object ConvertNode(object node, IEdmTypeReference typeReference)
        {
            // TODO: maybe find a better solution to do the convert.
            if (node == null)
            {
                return null;
            }

            ConstantNode constantNode = node as ConstantNode;
            if (constantNode == null)
            {
                return node;
            }

            if (constantNode.Value is UriTemplateExpression)
            {
                return node;
            }

            if (constantNode.Value is ODataEnumValue)
            {
                return node;
            }

            string literal = constantNode.LiteralText;
            object convertValue = ODataUriUtils.ConvertFromUriLiteral(literal, ODataVersion.V4, _model, typeReference);
            return new ConstantNode(convertValue, literal, typeReference);
        }

        internal static SingleValueNode TranslateParameterAlias(
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
    }
}
