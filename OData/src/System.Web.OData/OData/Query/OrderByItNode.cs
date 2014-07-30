// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.OData.Core.UriParser;

namespace System.Web.OData.Query
{
    /// <summary>
    /// Represents the order by expression '$it' in the $orderby clause.
    /// </summary>
    public class OrderByItNode : OrderByNode
    {
        /// <summary>
        /// Instantiates a new instance of <see cref="OrderByItNode"/> class.
        /// </summary>
        /// <param name="direction">The <see cref="OrderByDirection"/> for this node.</param>
        public OrderByItNode(OrderByDirection direction)
            : base(direction)
        {
        }
    }
}
