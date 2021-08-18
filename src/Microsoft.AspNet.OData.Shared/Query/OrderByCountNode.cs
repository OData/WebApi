//-----------------------------------------------------------------------------
// <copyright file="OrderByCountNode.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Common;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Query
{
    /// <summary>
    /// Represents an order by <see cref="IEdmProperty"/> expression.
    /// </summary>
    public class OrderByCountNode : OrderByNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderByCountNode"/> class.
        /// </summary>
        /// <param name="orderByClause">The orderby clause representing property access.</param>
        public OrderByCountNode(OrderByClause orderByClause)
        {
            if (orderByClause == null)
            {
                throw Error.ArgumentNull("orderByClause");
            }

            OrderByClause = orderByClause;
            Direction = orderByClause.Direction;
        }

        /// <summary>
        /// Gets the <see cref="OrderByClause"/> of this node.
        /// </summary>
        public OrderByClause OrderByClause { get; private set; }
    }
}
