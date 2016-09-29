// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.OData.Edm;
using Microsoft.OData;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Query.Validators
{
    /// <summary>
    /// Represents a validator used to validate an <see cref="OrderByQueryOption"/> based on the <see cref="ODataValidationSettings"/>.
    /// </summary>
    public class OrderByQueryValidator
    {
        /// <summary>
        /// Validates an <see cref="OrderByQueryOption" />.
        /// </summary>
        /// <param name="orderByOption">The $orderby query.</param>
        /// <param name="validationSettings">The validation settings.</param>
        public virtual void Validate(OrderByQueryOption orderByOption, ODataValidationSettings validationSettings)
        {
            if (orderByOption == null)
            {
                throw Error.ArgumentNull("orderByOption");
            }

            if (validationSettings == null)
            {
                throw Error.ArgumentNull("validationSettings");
            }

            int nodeCount = 0;
            for (OrderByClause clause = orderByOption.OrderByClause; clause != null; clause = clause.ThenBy)
            {
                nodeCount++;
                if (nodeCount > validationSettings.MaxOrderByNodeCount)
                {
                    throw new ODataException(Error.Format(SRResources.OrderByNodeCountExceeded,
                        validationSettings.MaxOrderByNodeCount));
                }
            }

            OrderByModelLimitationsValidator validator = new OrderByModelLimitationsValidator(orderByOption.Context.Model);
            bool explicitAllowedProperties = validationSettings.AllowedOrderByProperties.Count > 0;

            foreach (OrderByNode node in orderByOption.OrderByNodes)
            {
                string propertyName = null;
                OrderByPropertyNode propertyNode = node as OrderByPropertyNode;
                if (propertyNode != null)
                {
                    propertyName = propertyNode.Property.Name;
                    bool isValidPath = !validator.TryValidate(propertyNode.OrderByClause, explicitAllowedProperties);
                    if (propertyName != null && isValidPath && explicitAllowedProperties)
                    {
                        // Explicit allowed properties were specified, but this one isn't within the list of allowed 
                        // properties.
                        if (!IsAllowed(validationSettings, propertyName))
                        {
                            throw new ODataException(Error.Format(SRResources.NotAllowedOrderByProperty, propertyName,
                                "AllowedOrderByProperties"));
                        }
                    }
                    else if (propertyName != null)
                    {
                        // The property wasn't limited but it wasn't contained in the set of explicitly allowed 
                        // properties.
                        if (!IsAllowed(validationSettings, propertyName))
                        {
                            throw new ODataException(Error.Format(SRResources.NotAllowedOrderByProperty, propertyName,
                                "AllowedOrderByProperties"));
                        }
                    }
                }
                else
                {
                    propertyName = "$it";
                    if (!IsAllowed(validationSettings, propertyName))
                    {
                        throw new ODataException(Error.Format(SRResources.NotAllowedOrderByProperty, propertyName,
                            "AllowedOrderByProperties"));
                    }
                }
            }
        }

        private static bool IsAllowed(ODataValidationSettings validationSettings, string propertyName)
        {
            return validationSettings.AllowedOrderByProperties.Count == 0 ||
                   validationSettings.AllowedOrderByProperties.Contains(propertyName);
        }

        private class OrderByModelLimitationsValidator : QueryNodeVisitor<SingleValueNode>
        {
            private readonly IEdmModel _model;

            public OrderByModelLimitationsValidator(IEdmModel model)
            {
                _model = model;
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
                if (EdmLibHelpers.IsNotSortable(nodeIn.Property, _model))
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
                if (EdmLibHelpers.IsNotSortable(nodeIn.NavigationProperty, _model))
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
                return null;
            }
        }
    }
}
