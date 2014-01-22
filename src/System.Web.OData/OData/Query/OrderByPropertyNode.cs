// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http;
using System.Web.OData.Properties;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Edm;

namespace System.Web.OData.Query
{
    /// <summary>
    /// Represents an order by <see cref="IEdmProperty"/> expression.
    /// </summary>
    public class OrderByPropertyNode : OrderByNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderByPropertyNode"/> class.
        /// </summary>
        /// <param name="orderByClause">The orderby clause representing property access.</param>
        public OrderByPropertyNode(OrderByClause orderByClause)
        {
            if (orderByClause == null)
            {
                throw Error.ArgumentNull("orderByClause");
            }

            OrderByClause = orderByClause;
            Direction = orderByClause.Direction;

            SingleValuePropertyAccessNode propertyExpression = orderByClause.Expression as SingleValuePropertyAccessNode;
            if (propertyExpression == null)
            {
                throw new ODataException(SRResources.OrderByClauseNotSupported);
            }
            Property = propertyExpression.Property;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderByPropertyNode"/> class.
        /// </summary>
        /// <param name="property">The <see cref="IEdmProperty"/> for this node.</param>
        /// <param name="direction">The <see cref="OrderByDirection"/> for this node.</param>
        public OrderByPropertyNode(IEdmProperty property, OrderByDirection direction)
            : base(direction)
        {
            if (property == null)
            {
                throw Error.ArgumentNull("property");
            }

            Property = property;
        }

        /// <summary>
        /// Gets the <see cref="OrderByClause"/> of this node.
        /// </summary>
        public OrderByClause OrderByClause { get; private set; }

        /// <summary>
        /// Gets the <see cref="IEdmProperty"/> for the current node.
        /// </summary>
        public IEdmProperty Property { get; private set; }
    }
}
