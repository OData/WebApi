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
        /// Visit an AllNode.
        /// </summary>
        /// <param name="nodeIn">The node to be visited.</param>
        /// <returns>The visited node.</returns>
        public override QueryNode Visit(AllNode nodeIn)
        {
            return new AllNode(nodeIn.RangeVariables, nodeIn.CurrentRangeVariable)
            {
                Source = (CollectionNode)nodeIn.Source.Accept(this),
                Body = (SingleValueNode)nodeIn.Body.Accept(this)
            };
        }

        /// <summary>
        /// Visit an AnyNode.
        /// </summary>
        /// <param name="nodeIn">The node to be visited.</param>
        /// <returns>The visited node.</returns>
        public override QueryNode Visit(AnyNode nodeIn)
        {
            return new AnyNode(nodeIn.RangeVariables, nodeIn.CurrentRangeVariable)
            {
                Source = (CollectionNode)nodeIn.Source.Accept(this),
                Body = (SingleValueNode)nodeIn.Body.Accept(this)
            };
        }

        /// <summary>
        /// Visit a BinaryOperatorNode.
        /// </summary>
        /// <param name="nodeIn">The node to be visited.</param>
        /// <returns>The visited node.</returns>
        public override QueryNode Visit(BinaryOperatorNode nodeIn)
        {
            return new BinaryOperatorNode(
                nodeIn.OperatorKind,
                (SingleValueNode)nodeIn.Left.Accept(this),
                (SingleValueNode)nodeIn.Right.Accept(this));
        }

        /// <summary>
        /// Visit a CollectionFunctionCallNode.
        /// </summary>
        /// <param name="nodeIn">The node to be visited.</param>
        /// <returns>The visited node.</returns>
        public override QueryNode Visit(CollectionFunctionCallNode nodeIn)
        {
            return new CollectionFunctionCallNode(
                nodeIn.Name,
                nodeIn.Functions,
                nodeIn.Parameters.Select(p => p.Accept(this)),
                nodeIn.CollectionType,
                nodeIn.Source);
        }

        /// <summary>
        /// Visit a CollectionNavigationNode.
        /// </summary>
        /// <param name="nodeIn">The node to be visited.</param>
        /// <returns>The visited node.</returns>
        public override QueryNode Visit(CollectionNavigationNode nodeIn)
        {
            return nodeIn;
        }

        /// <summary>
        /// Visit a CollectionOpenPropertyAccessNode.
        /// </summary>
        /// <param name="nodeIn">The node to be visited.</param>
        /// <returns>The visited node.</returns>
        public override QueryNode Visit(CollectionOpenPropertyAccessNode nodeIn)
        {
            return nodeIn;
        }

        /// <summary>
        /// Visit a CollectionPropertyAccessNode.
        /// </summary>
        /// <param name="nodeIn">The node to be visited.</param>
        /// <returns>The visited node.</returns>
        public override QueryNode Visit(CollectionPropertyAccessNode nodeIn)
        {
            return nodeIn;
        }

        /// <summary>
        /// Visit a CollectionPropertyCastNode.
        /// </summary>
        /// <param name="nodeIn">The node to be visited.</param>
        /// <returns>The visited node.</returns>
        public override QueryNode Visit(CollectionPropertyCastNode nodeIn)
        {
            return nodeIn;
        }

        /// <summary>
        /// Visit a ConstantNode.
        /// </summary>
        /// <param name="nodeIn">The node to be visited.</param>
        /// <returns>The visited node.</returns>
        public override QueryNode Visit(ConstantNode nodeIn)
        {
            return nodeIn;
        }

        /// <summary>
        /// Visit a ConvertNode.
        /// </summary>
        /// <param name="nodeIn">The node to be visited.</param>
        /// <returns>The visited node.</returns>
        public override QueryNode Visit(ConvertNode nodeIn)
        {
            return new ConvertNode((SingleValueNode)nodeIn.Source.Accept(this), nodeIn.TypeReference);
        }

        /// <summary>
        /// Visit an EntityCollectionCastNode.
        /// </summary>
        /// <param name="nodeIn">The node to be visited.</param>
        /// <returns>The visited node.</returns>
        public override QueryNode Visit(EntityCollectionCastNode nodeIn)
        {
            return nodeIn;
        }

        /// <summary>
        /// Visit an EntityCollectionFunctionCallNode.
        /// </summary>
        /// <param name="nodeIn">The node to be visited.</param>
        /// <returns>The visited node.</returns>
        public override QueryNode Visit(EntityCollectionFunctionCallNode nodeIn)
        {
            return new EntityCollectionFunctionCallNode(
                nodeIn.Name,
                nodeIn.Functions,
                nodeIn.Parameters.Select(p => p.Accept(this)),
                nodeIn.CollectionType,
                (IEdmEntitySetBase)nodeIn.NavigationSource,
                nodeIn.Source);
        }

        /// <summary>
        /// Visit an EntityRangeVariableReferenceNode.
        /// </summary>
        /// <param name="nodeIn">The node to be visited.</param>
        /// <returns>The visited node.</returns>
        public override QueryNode Visit(EntityRangeVariableReferenceNode nodeIn)
        {
            return nodeIn;
        }

        /// <summary>
        /// Visit a NamedFunctionParameterNode.
        /// </summary>
        /// <param name="nodeIn">The node to be visited.</param>
        /// <returns>The visited node.</returns>
        public override QueryNode Visit(NamedFunctionParameterNode nodeIn)
        {
            return new NamedFunctionParameterNode(nodeIn.Name, nodeIn.Value.Accept(this));
        }

        /// <summary>
        /// Visit a NonentityRangeVariableReferenceNode.
        /// </summary>
        /// <param name="nodeIn">The node to be visited.</param>
        /// <returns>The visited node.</returns>
        public override QueryNode Visit(NonentityRangeVariableReferenceNode nodeIn)
        {
            return nodeIn;
        }

        /// <summary>
        /// Visit a ParameterAliasNode.
        /// </summary>
        /// <param name="nodeIn">The node to be visited.</param>
        /// <returns>The visited node.</returns>
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
        /// Visit a SearchTermNode.
        /// </summary>
        /// <param name="nodeIn">The node to be visited.</param>
        /// <returns>The visited node.</returns>
        public override QueryNode Visit(SearchTermNode nodeIn)
        {
            return nodeIn;
        }

        /// <summary>
        /// Visit a SingleEntityCastNode.
        /// </summary>
        /// <param name="nodeIn">The node to be visited.</param>
        /// <returns>The visited node.</returns>
        public override QueryNode Visit(SingleEntityCastNode nodeIn)
        {
            return nodeIn;
        }

        /// <summary>
        /// Visit a SingleEntityFunctionCallNode.
        /// </summary>
        /// <param name="nodeIn">The node to be visited.</param>
        /// <returns>The visited node.</returns>
        public override QueryNode Visit(SingleEntityFunctionCallNode nodeIn)
        {
            return new SingleEntityFunctionCallNode(
                nodeIn.Name,
                nodeIn.Functions,
                nodeIn.Parameters.Select(p => p.Accept(this)),
                nodeIn.EntityTypeReference,
                nodeIn.NavigationSource,
                nodeIn.Source);
        }

        /// <summary>
        /// Visit a SingleNavigationNode.
        /// </summary>
        /// <param name="nodeIn">The node to be visited.</param>
        /// <returns>The visited node.</returns>
        public override QueryNode Visit(SingleNavigationNode nodeIn)
        {
            return nodeIn;
        }

        /// <summary>
        /// Visit a SingleValueCastNode.
        /// </summary>
        /// <param name="nodeIn">The node to be visited.</param>
        /// <returns>The visited node.</returns>
        public override QueryNode Visit(SingleValueCastNode nodeIn)
        {
            return nodeIn;
        }

        /// <summary>
        /// Visit a SingleValueFunctionCallNode.
        /// </summary>
        /// <param name="nodeIn">The node to be visited.</param>
        /// <returns>The visited node.</returns>
        public override QueryNode Visit(SingleValueFunctionCallNode nodeIn)
        {
            return new SingleValueFunctionCallNode(
                nodeIn.Name,
                nodeIn.Functions,
                nodeIn.Parameters.Select(p => p.Accept(this)),
                nodeIn.TypeReference,
                nodeIn.Source);
        }

        /// <summary>
        /// Visit a SingleValueOpenPropertyAccessNode.
        /// </summary>
        /// <param name="nodeIn">The node to be visited.</param>
        /// <returns>The visited node.</returns>
        public override QueryNode Visit(SingleValueOpenPropertyAccessNode nodeIn)
        {
            return nodeIn;
        }

        /// <summary>
        /// Visit a SingleValuePropertyAccessNode.
        /// </summary>
        /// <param name="nodeIn">The node to be visited.</param>
        /// <returns>The visited node.</returns>
        public override QueryNode Visit(SingleValuePropertyAccessNode nodeIn)
        {
            return nodeIn;
        }

        /// <summary>
        /// Visit an UnaryOperatorNode.
        /// </summary>
        /// <param name="nodeIn">The node to be visited.</param>
        /// <returns>The visited node.</returns>
        public override QueryNode Visit(UnaryOperatorNode nodeIn)
        {
            return new UnaryOperatorNode(nodeIn.OperatorKind, (SingleValueNode)nodeIn.Operand.Accept(this));
        }
    }
}
