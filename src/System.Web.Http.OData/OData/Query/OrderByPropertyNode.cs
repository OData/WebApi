// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Query;
using Microsoft.Data.OData.Query.SemanticAst;

namespace System.Web.Http.OData.Query
{
    /// <summary>
    /// Class describing the <see cref="IEdmProperty"/> and 
    /// <see cref="OrderByDirection"/> for a single property
    /// in an OrderBy expression.
    /// </summary>
    public class OrderByPropertyNode
    {
        /// <summary>
        /// Instantiates a new instance of the <see cref="OrderByPropertyNode"/> class.
        /// </summary>
        /// <param name="property">The <see cref="IEdmProperty"/> for this node.</param>
        /// <param name="direction">The <see cref="OrderByDirection"/> for this node.</param>
        public OrderByPropertyNode(IEdmProperty property, OrderByDirection direction)
        {
            if (property == null)
            {
                throw Error.ArgumentNull("property");
            }

            Property = property;
            Direction = direction;
        }

        /// <summary>
        /// Gets the <see cref="IEdmProperty"/> for the current node.
        /// </summary>
        public IEdmProperty Property { get; private set; }

        /// <summary>
        /// Gets the <see cref="OrderByDirection"/> for the current node.
        /// </summary>
        public OrderByDirection Direction { get; private set; }

        /// <summary>
        /// Creates a collection of <see cref="OrderByPropertyNode"/>
        /// instances from a linked list of <see cref="OrderByQueryNode"/>
        /// instances.
        /// </summary>
        /// <remarks>The order of the items in the <see cref="OrderByQueryNode"/>
        /// linked list will be reversed in the <see cref="OrderByPropertyNode"/>
        /// collection.</remarks>
        /// <param name="node">The head of the <see cref="OrderByQueryNode"/>
        /// linked list.</param>
        /// <returns>The collection of new <see cref="OrderByPropertyNode"/> instances.</returns>
        public static ICollection<OrderByPropertyNode> CreateCollection(OrderByQueryNode node)
        {
            LinkedList<OrderByPropertyNode> result = new LinkedList<OrderByPropertyNode>();
            for (OrderByQueryNode currentNode = node; 
                 currentNode != null; 
                 currentNode = currentNode.Collection as OrderByQueryNode)
            {
                PropertyAccessQueryNode property = currentNode.Expression as PropertyAccessQueryNode;

                if (property == null)
                {
                    throw new ODataException(SRResources.OrderByPropertyNotFound);
                }
                result.AddFirst(new OrderByPropertyNode(property.Property, currentNode.Direction));
            }

            return result;
        }
    }
}
