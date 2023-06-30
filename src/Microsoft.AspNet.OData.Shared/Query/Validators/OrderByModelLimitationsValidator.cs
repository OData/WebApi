//-----------------------------------------------------------------------------
// <copyright file="OrderByModelLimitationsValidator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Query.Validators
{
    internal class OrderByModelLimitationsValidator : QueryNodeVisitor<SingleValueNode>
    {
        private readonly IEdmModel _model;
        private readonly bool _enableOrderBy;
        private IEdmProperty _property;
        private IEdmStructuredType _structuredType;

        public OrderByModelLimitationsValidator(ODataQueryContext context, bool enableOrderBy)
        {
            _model = context.Model;
            _enableOrderBy = enableOrderBy;

            if (context.Path != null)
            {
                _property = context.TargetProperty;
                _structuredType = context.TargetStructuredType;
            }
        }

        public bool TryValidate(IEdmProperty property, IEdmStructuredType structuredType, OrderByClause orderByClause,
            bool explicitPropertiesDefined)
        {
            _property = property;
            _structuredType = structuredType;
            return TryValidate(orderByClause, explicitPropertiesDefined);
        }

        // Visits the expression to find the first node if any, that is not sortable and throws
        // an exception only if no explicit properties have been defined in AllowedOrderByProperties
        // on the ODataValidationSettings instance associated with this OrderByValidator.
        public bool TryValidate(OrderByClause orderByClause, bool explicitPropertiesDefined)
        {
            SingleValueNode invalidNode = orderByClause.Expression.Accept(this);
            if (invalidNode != null && !explicitPropertiesDefined)
            {
                throw new ODataException(Error.Format(SRResources.NotSortablePropertyUsedInOrderBy,
                    GetPropertyName(invalidNode)));
            }
            return invalidNode == null;
        }

        public override SingleValueNode Visit(SingleValuePropertyAccessNode nodeIn)
        {
            if (nodeIn.Source != null)
            {
                if (nodeIn.Source.Kind == QueryNodeKind.SingleNavigationNode)
                {
                    SingleNavigationNode singleNavigationNode = nodeIn.Source as SingleNavigationNode;
                    if (EdmLibHelpers.IsNotSortable(nodeIn.Property, singleNavigationNode.NavigationProperty,
                        singleNavigationNode.NavigationProperty.ToEntityType(), _model, _enableOrderBy))
                    {
                        return nodeIn;
                    }
                }
                else if (nodeIn.Source.Kind == QueryNodeKind.SingleComplexNode)
                {
                    SingleComplexNode singleComplexNode = nodeIn.Source as SingleComplexNode;
                    if (EdmLibHelpers.IsNotSortable(nodeIn.Property, singleComplexNode.Property,
                        nodeIn.Property.DeclaringType, _model, _enableOrderBy))
                    {
                        return nodeIn;
                    }
                }
                else if (EdmLibHelpers.IsNotSortable(nodeIn.Property, _property, _structuredType, _model, _enableOrderBy))
                {
                    return nodeIn;
                }
            }

            if (nodeIn.Source != null)
            {
                return nodeIn.Source.Accept(this);
            }

            return null;
        }

        public override SingleValueNode Visit(SingleComplexNode nodeIn)
        {
            if (EdmLibHelpers.IsNotSortable(nodeIn.Property, _property, _structuredType, _model, _enableOrderBy))
            {
                return nodeIn;
            }

            if (nodeIn.Source != null)
            {
                return nodeIn.Source.Accept(this);
            }

            return null;
        }

        public override SingleValueNode Visit(SingleNavigationNode nodeIn)
        {
            if (EdmLibHelpers.IsNotSortable(nodeIn.NavigationProperty, _property, _structuredType, _model,
                _enableOrderBy))
            {
                return nodeIn;
            }

            if (nodeIn.Source != null)
            {
                return nodeIn.Source.Accept(this);
            }

            return null;
        }

        public override SingleValueNode Visit(ResourceRangeVariableReferenceNode nodeIn)
        {
            return null;
        }

        public override SingleValueNode Visit(NonResourceRangeVariableReferenceNode nodeIn)
        {
            return null;
        }

        public override SingleValueNode Visit(SingleResourceCastNode nodeIn)
        {
            if (nodeIn.Source != null)
            {
                if (nodeIn.Source.Kind == QueryNodeKind.SingleComplexNode)
                {
                    SingleComplexNode singleComplexNode = nodeIn.Source as SingleComplexNode;

                    if (singleComplexNode != null)
                    {
                        return Visit(singleComplexNode);
                    }
                }
            }

            return null;
        }

        private static string GetPropertyName(SingleValueNode node)
        {
            if (node.Kind == QueryNodeKind.SingleNavigationNode)
            {
                return ((SingleNavigationNode)node).NavigationProperty.Name;
            }
            else if (node.Kind == QueryNodeKind.SingleValuePropertyAccess)
            {
                return ((SingleValuePropertyAccessNode)node).Property.Name;
            }
            else if (node.Kind == QueryNodeKind.SingleComplexNode)
            {
                return ((SingleComplexNode)node).Property.Name;
            }
            return null;
        }
    }
}
