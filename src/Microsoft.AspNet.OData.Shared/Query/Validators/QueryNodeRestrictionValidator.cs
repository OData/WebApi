//-----------------------------------------------------------------------------
// <copyright file="QueryNodeRestrictionValidator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Query.Validators
{
    // Reuse $filter's operator/function checks and complexity bookkeeping, but walk properties
    // using explicit restrictions only, independently of the global filter/select switches.
    internal sealed class QueryNodeRestrictionValidator : FilterQueryValidator
    {
        private readonly ODataQueryContext _context;

        private QueryNodeRestrictionValidator(ODataQueryContext context)
            : base(new DefaultQuerySettings())
        {
            _context = context;
        }

        internal static void Validate(QueryNode node, ODataQueryContext context, ODataValidationSettings settings)
        {
            // Limits apply separately to each expression, just as they do to a $filter clause.
            new QueryNodeRestrictionValidator(context).ValidateQueryNode(node, settings);
        }

        public override void ValidateQueryNode(QueryNode node, ODataValidationSettings settings)
        {
            if (node == null)
            {
                return;
            }

            RuntimeHelpers.EnsureSufficientExecutionStack();
            IncrementNodeCount(settings);

            switch (node.Kind)
            {
                case QueryNodeKind.BinaryOperator:
                    ValidateBinaryOperatorNode((BinaryOperatorNode)node, settings);
                    break;

                case QueryNodeKind.UnaryOperator:
                    UnaryOperatorNode unary = (UnaryOperatorNode)node;
                    ValidateUnaryOperatorAllowed(unary, settings);
                    ValidateQueryNode(unary.Operand, settings);
                    break;

                case QueryNodeKind.Convert:
                    ValidateQueryNode(((ConvertNode)node).Source, settings);
                    break;

                case QueryNodeKind.SingleValuePropertyAccess:
                    SingleValuePropertyAccessNode singleProperty = (SingleValuePropertyAccessNode)node;
                    CheckProperty(singleProperty.Property, checkSelectable: true);
                    ValidateQueryNode(singleProperty.Source, settings);
                    break;

                case QueryNodeKind.CollectionPropertyAccess:
                    CollectionPropertyAccessNode collectionProperty = (CollectionPropertyAccessNode)node;
                    CheckProperty(collectionProperty.Property, checkSelectable: true);
                    ValidateQueryNode(collectionProperty.Source, settings);
                    break;

                case QueryNodeKind.SingleComplexNode:
                    SingleComplexNode singleComplex = (SingleComplexNode)node;
                    CheckProperty(singleComplex.Property, checkSelectable: true);
                    ValidateQueryNode(singleComplex.Source, settings);
                    break;

                case QueryNodeKind.CollectionComplexNode:
                    CollectionComplexNode collectionComplex = (CollectionComplexNode)node;
                    CheckProperty(collectionComplex.Property, checkSelectable: true);
                    ValidateQueryNode(collectionComplex.Source, settings);
                    break;

                case QueryNodeKind.AggregatedCollectionPropertyNode:
                    AggregatedCollectionPropertyNode aggregatedProperty = (AggregatedCollectionPropertyNode)node;
                    CheckProperty(aggregatedProperty.Property, checkSelectable: true);
                    ValidateQueryNode(aggregatedProperty.Source, settings);
                    break;

                case QueryNodeKind.SingleNavigationNode:
                    SingleNavigationNode singleNavigation = (SingleNavigationNode)node;
                    CheckProperty(singleNavigation.NavigationProperty, checkSelectable: false);
                    ValidateQueryNode(singleNavigation.Source, settings);
                    break;

                case QueryNodeKind.CollectionNavigationNode:
                    CollectionNavigationNode collectionNavigation = (CollectionNavigationNode)node;
                    CheckProperty(collectionNavigation.NavigationProperty, checkSelectable: false);
                    ValidateQueryNode(collectionNavigation.Source, settings);
                    break;

                case QueryNodeKind.SingleValueFunctionCall:
                    SingleValueFunctionCallNode singleFunction = (SingleValueFunctionCallNode)node;
                    ValidateFunctionCall(singleFunction.Name, singleFunction.Parameters, settings);
                    break;

                case QueryNodeKind.CollectionFunctionCall:
                    CollectionFunctionCallNode collectionFunction = (CollectionFunctionCallNode)node;
                    ValidateFunctionCall(collectionFunction.Name, collectionFunction.Parameters, settings);
                    break;

                case QueryNodeKind.SingleResourceFunctionCall:
                    SingleResourceFunctionCallNode resourceFunction = (SingleResourceFunctionCallNode)node;
                    ValidateFunctionCall(resourceFunction.Name, resourceFunction.Parameters, settings);
                    break;

                case QueryNodeKind.CollectionResourceFunctionCall:
                    CollectionResourceFunctionCallNode resourceCollectionFunction = (CollectionResourceFunctionCallNode)node;
                    ValidateFunctionCall(resourceCollectionFunction.Name, resourceCollectionFunction.Parameters, settings);
                    break;

                case QueryNodeKind.Any:
                    AnyNode any = (AnyNode)node;
                    ValidateLambda("any", any.Source, any.Body, settings);
                    break;

                case QueryNodeKind.All:
                    AllNode all = (AllNode)node;
                    ValidateLambda("all", all.Source, all.Body, settings);
                    break;

                case QueryNodeKind.SingleResourceCast:
                    ValidateQueryNode(((SingleResourceCastNode)node).Source, settings);
                    break;

                case QueryNodeKind.CollectionResourceCast:
                    ValidateQueryNode(((CollectionResourceCastNode)node).Source, settings);
                    break;

                case QueryNodeKind.In:
                    InNode inNode = (InNode)node;
                    ValidateQueryNode(inNode.Left, settings);
                    ValidateQueryNode(inNode.Right, settings);
                    break;

                case QueryNodeKind.Count:
                    // aggregate($count as ...) has the same kind but no underlying property.
                    ValidateCountExpression(node as CountNode, settings);
                    break;
            }
        }

        private void ValidateCountExpression(CountNode count, ODataValidationSettings settings)
        {
            if (count == null)
            {
                return;
            }

            if (count.FilterClause != null)
            {
                ValidateQueryNode(count.FilterClause.Expression, settings);
            }

            if (count.SearchClause != null)
            {
                ValidateQueryNode(count.SearchClause.Expression, settings);
            }

            ValidateQueryNode(count.Source, settings);
        }

        private void ValidateFunctionCall(string functionName, IEnumerable<QueryNode> parameters,
            ODataValidationSettings settings)
        {
            ValidateFunction(functionName, settings);
            EnterFunctionCall(settings);
            try
            {
                foreach (QueryNode parameter in parameters)
                {
                    ValidateQueryNode(parameter, settings);
                }
            }
            finally
            {
                ExitFunctionCall();
            }
        }

        private void ValidateLambda(string functionName, QueryNode source, QueryNode body, ODataValidationSettings settings)
        {
            ValidateFunction(functionName, settings);
            EnterLambda(settings);
            try
            {
                ValidateQueryNode(source, settings);
                ValidateQueryNode(body, settings);
            }
            finally
            {
                ExitLambda();
            }
        }

        private void CheckProperty(IEdmProperty property, bool checkSelectable)
        {
            if (property == null)
            {
                return;
            }

            // Null path arguments deliberately resolve each property's declaring-type restrictions,
            // not configurations scoped to a particular traversal path.
            if (EdmLibHelpers.IsNotFilterable(property, null, null, _context.Model, enableFilter: true))
            {
                throw new ODataException(Error.Format(SRResources.NotFilterablePropertyUsedInFilter, property.Name));
            }

            if (checkSelectable && EdmLibHelpers.IsNotSelectable(property, null, null, _context.Model, enableSelect: true))
            {
                throw new ODataException(Error.Format(SRResources.NotSelectablePropertyUsedInSelect, property.Name));
            }
        }
    }
}
