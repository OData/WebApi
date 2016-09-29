// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData.UriParser;
using System;
using System.Globalization;
using Microsoft.AspNetCore.OData.Common;

namespace Microsoft.AspNetCore.OData.Query
{
    /// <summary>
    /// Represents a single order by expression in the $orderby clause.
    /// </summary>
    public abstract class OrderByNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderByNode"/> class.
        /// </summary>
        /// <param name="direction">The direction of the sort order.</param>
        protected OrderByNode(OrderByDirection direction)
        {
            Direction = direction;
            PropertyPath = String.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderByNode"/> class.
        /// </summary>
        /// <param name="orderByClause">The clause of the sort order.</param>
        protected OrderByNode(OrderByClause orderByClause)
        {
            if (orderByClause == null)
            {
                throw Error.ArgumentNull("orderByClause");
            }

            Direction = orderByClause.Direction;
            PropertyPath = RestorePropertyPath(orderByClause.Expression);
        }

        internal OrderByNode()
        {
        }

        /// <summary>
        /// Gets the <see cref="OrderByDirection"/> for the current node.
        /// </summary>
        public OrderByDirection Direction { get; internal set; }

        internal string PropertyPath { get; set; }

        /// <summary>
        /// Creates a list of <see cref="OrderByNode"/> instances from a linked list of <see cref="OrderByClause"/> instances.
        /// </summary>
        /// <param name="orderByClause">The head of the <see cref="OrderByClause"/> linked list.</param>
        /// <returns>The list of new <see cref="OrderByPropertyNode"/> instances.</returns>
        public static IList<OrderByNode> CreateCollection(OrderByClause orderByClause)
        {
            List<OrderByNode> result = new List<OrderByNode>();
            for (OrderByClause clause = orderByClause; clause != null; clause = clause.ThenBy)
            {
                if (clause.Expression is NonResourceRangeVariableReferenceNode ||
                    clause.Expression is ResourceRangeVariableReferenceNode)
                {
                    result.Add(new OrderByItNode(clause.Direction));
                    continue;
                }

                if (clause.Expression is SingleValueOpenPropertyAccessNode)
                {
                    result.Add(new OrderByOpenPropertyNode(clause));
                }
                else
                {
                    result.Add(new OrderByPropertyNode(clause));
                }
            }

            return result;
        }

        internal static string RestorePropertyPath(SingleValueNode expression)
        {
            if (expression == null)
            {
                return String.Empty;
            }

            string propertyName = String.Empty;
            SingleValueNode source = null;

            var accessNode = expression as SingleValuePropertyAccessNode;
            if (accessNode != null)
            {
                propertyName = accessNode.Property.Name;
                source = accessNode.Source;
            }
            else
            {
                var complexNode = expression as SingleComplexNode;
                if (complexNode != null)
                {
                    propertyName = complexNode.Property.Name;
                    source = complexNode.Source;
                }
            }

            var parentPath = RestorePropertyPath(source);
            if (String.IsNullOrEmpty(parentPath))
            {
                return propertyName;
            }
            else
            {
                return String.Format(CultureInfo.CurrentCulture, "{0}/{1}", parentPath, propertyName);
            }
        }
    }
}
