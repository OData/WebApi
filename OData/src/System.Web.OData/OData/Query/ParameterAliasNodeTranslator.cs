// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.OData.Routing;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Core.UriParser.Visitors;
using Microsoft.OData.Edm;

namespace System.Web.OData.Query
{
    /// <summary>
    /// This defines a translator to tranlate parameter alias nodes.
    /// </summary>
    public class ParameterAliasNodeTranslator : QueryNodeVisitor<QueryNode>
    {
        private IDictionary<string, SingleValueNode> _parameterAliasNode;

        /// <summary>
        /// Initialize a new instance of <see cref="ParameterAliasNodeTranslator"/>.
        /// </summary>
        /// <param name="parameterAliasNodes">Parameter alias nodes mapping.</param>
        public ParameterAliasNodeTranslator(IDictionary<string, SingleValueNode> parameterAliasNodes)
        {
            if (parameterAliasNodes == null)
            {
                throw Error.ArgumentNull("parameterAliasNodes");
            }

            _parameterAliasNode = parameterAliasNodes;
        }

        /// <summary>
        /// Translate an AllNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(AllNode nodeIn)
        {
            AllNode allNode = new AllNode(nodeIn.RangeVariables, nodeIn.CurrentRangeVariable);

            if (nodeIn.Source != null)
            {
                allNode.Source = (CollectionNode)nodeIn.Source.Accept(this);
            }

            if (nodeIn.Body != null)
            {
                allNode.Body = (SingleValueNode)nodeIn.Body.Accept(this);
            }

            return allNode;
        }

        /// <summary>
        /// Translate an AnyNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(AnyNode nodeIn)
        {
            AnyNode anyNode = new AnyNode(nodeIn.RangeVariables, nodeIn.CurrentRangeVariable);

            if (nodeIn.Source != null)
            {
                anyNode.Source = (CollectionNode)nodeIn.Source.Accept(this);
            }

            if (nodeIn.Body != null)
            {
                anyNode.Body = (SingleValueNode)nodeIn.Body.Accept(this);
            }

            return anyNode;
        }

        /// <summary>
        /// Translate a BinaryOperatorNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(BinaryOperatorNode nodeIn)
        {
            return new BinaryOperatorNode(
                nodeIn.OperatorKind,
                (SingleValueNode)nodeIn.Left.Accept(this),
                (SingleValueNode)nodeIn.Right.Accept(this));
        }

        /// <summary>
        /// Translate a CollectionFunctionCallNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(CollectionFunctionCallNode nodeIn)
        {
            return new CollectionFunctionCallNode(
                nodeIn.Name,
                nodeIn.Functions,
                nodeIn.Parameters.Select(p => p.Accept(this)),
                nodeIn.CollectionType,
                nodeIn.Source == null ? null : nodeIn.Source.Accept(this));
        }

        /// <summary>
        /// Translate a CollectionNavigationNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(CollectionNavigationNode nodeIn)
        {
            return nodeIn.Source == null ?
                nodeIn :
                new CollectionNavigationNode(
                    nodeIn.NavigationProperty,
                    (SingleEntityNode)nodeIn.Source.Accept(this));
        }

        /// <summary>
        /// Translate a CollectionOpenPropertyAccessNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(CollectionOpenPropertyAccessNode nodeIn)
        {
            return new CollectionOpenPropertyAccessNode(
                (SingleValueNode)nodeIn.Source.Accept(this),
                nodeIn.Name);
        }

        /// <summary>
        /// Translate a CollectionPropertyAccessNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(CollectionPropertyAccessNode nodeIn)
        {
            return new CollectionPropertyAccessNode(
                (SingleValueNode)nodeIn.Source.Accept(this),
                nodeIn.Property);
        }

        /// <summary>
        /// Translate a CollectionPropertyCastNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(CollectionPropertyCastNode nodeIn)
        {
            return new CollectionPropertyCastNode(
                (CollectionPropertyAccessNode)nodeIn.Source.Accept(this),
                (IEdmComplexType)nodeIn.ItemType.Definition);
        }

        /// <summary>
        /// Translate a ConstantNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The original node.</returns>
        public override QueryNode Visit(ConstantNode nodeIn)
        {
            return nodeIn;
        }

        /// <summary>
        /// Translate a ConvertNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(ConvertNode nodeIn)
        {
            return new ConvertNode((SingleValueNode)nodeIn.Source.Accept(this), nodeIn.TypeReference);
        }

        /// <summary>
        /// Translate an EntityCollectionCastNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(EntityCollectionCastNode nodeIn)
        {
            return new EntityCollectionCastNode(
                (EntityCollectionNode)nodeIn.Source.Accept(this),
                (IEdmEntityType)nodeIn.ItemType.Definition);
        }

        /// <summary>
        /// Translate an EntityCollectionFunctionCallNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(EntityCollectionFunctionCallNode nodeIn)
        {
            return new EntityCollectionFunctionCallNode(
                nodeIn.Name,
                nodeIn.Functions,
                nodeIn.Parameters.Select(p => p.Accept(this)),
                nodeIn.CollectionType,
                (IEdmEntitySetBase)nodeIn.NavigationSource,
                nodeIn.Source == null ? null : nodeIn.Source.Accept(this));
        }

        /// <summary>
        /// Translate an EntityRangeVariableReferenceNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The original node.</returns>
        public override QueryNode Visit(EntityRangeVariableReferenceNode nodeIn)
        {
            return nodeIn;
        }

        /// <summary>
        /// Translate a NamedFunctionParameterNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(NamedFunctionParameterNode nodeIn)
        {
            return new NamedFunctionParameterNode(
                nodeIn.Name,
                nodeIn.Value == null ? null : nodeIn.Value.Accept(this));
        }

        /// <summary>
        /// Translate a NonentityRangeVariableReferenceNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The original node.</returns>
        public override QueryNode Visit(NonentityRangeVariableReferenceNode nodeIn)
        {
            return nodeIn;
        }

        /// <summary>
        /// Translate a ParameterAliasNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(ParameterAliasNode nodeIn)
        {
            SingleValueNode node = ODataPathSegmentTranslator.TranslateParameterAlias(nodeIn, _parameterAliasNode);

            if (node == null)
            {
                return new ConstantNode(null);
            }
            else
            {
                return node.Accept(this);
            }
        }

        /// <summary>
        /// Translate a SearchTermNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The original node.</returns>
        public override QueryNode Visit(SearchTermNode nodeIn)
        {
            return nodeIn;
        }

        /// <summary>
        /// Translate a SingleEntityCastNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(SingleEntityCastNode nodeIn)
        {
            return nodeIn.Source == null ?
                nodeIn :
                new SingleEntityCastNode(
                    (SingleEntityNode)nodeIn.Source.Accept(this),
                    (IEdmEntityType)nodeIn.TypeReference.Definition);
        }

        /// <summary>
        /// Translate a SingleEntityFunctionCallNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(SingleEntityFunctionCallNode nodeIn)
        {
            return new SingleEntityFunctionCallNode(
                nodeIn.Name,
                nodeIn.Functions,
                nodeIn.Parameters.Select(p => p.Accept(this)),
                nodeIn.EntityTypeReference,
                nodeIn.NavigationSource,
                nodeIn.Source == null ? null : nodeIn.Source.Accept(this));
        }

        /// <summary>
        /// Translate a SingleNavigationNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(SingleNavigationNode nodeIn)
        {
            return nodeIn.Source == null ?
                nodeIn :
                new SingleNavigationNode(
                    nodeIn.NavigationProperty,
                    (SingleEntityNode)nodeIn.Source.Accept(this));
        }

        /// <summary>
        /// Translate a SingleValueCastNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(SingleValueCastNode nodeIn)
        {
            return nodeIn.Source == null ?
                nodeIn :
                new SingleValueCastNode(
                    (SingleValueNode)nodeIn.Source.Accept(this),
                    (IEdmComplexType)nodeIn.TypeReference.Definition);
        }

        /// <summary>
        /// Translate a SingleValueFunctionCallNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(SingleValueFunctionCallNode nodeIn)
        {
            return new SingleValueFunctionCallNode(
                nodeIn.Name,
                nodeIn.Functions,
                nodeIn.Parameters.Select(p => p.Accept(this)),
                nodeIn.TypeReference,
                nodeIn.Source == null ? null : nodeIn.Source.Accept(this));
        }

        /// <summary>
        /// Translate a SingleValueOpenPropertyAccessNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(SingleValueOpenPropertyAccessNode nodeIn)
        {
            return new SingleValueOpenPropertyAccessNode(
                (SingleValueNode)nodeIn.Source.Accept(this),
                nodeIn.Name);
        }

        /// <summary>
        /// Translate a SingleValuePropertyAccessNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(SingleValuePropertyAccessNode nodeIn)
        {
            return new SingleValuePropertyAccessNode(
                (SingleValueNode)nodeIn.Source.Accept(this),
                nodeIn.Property);
        }

        /// <summary>
        /// Translate an UnaryOperatorNode.
        /// </summary>
        /// <param name="nodeIn">The node to be translated.</param>
        /// <returns>The translated node.</returns>
        public override QueryNode Visit(UnaryOperatorNode nodeIn)
        {
            return new UnaryOperatorNode(nodeIn.OperatorKind, (SingleValueNode)nodeIn.Operand.Accept(this));
        }
    }
}
