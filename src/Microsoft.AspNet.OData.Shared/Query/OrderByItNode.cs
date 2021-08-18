//-----------------------------------------------------------------------------
// <copyright file="OrderByItNode.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Query
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
