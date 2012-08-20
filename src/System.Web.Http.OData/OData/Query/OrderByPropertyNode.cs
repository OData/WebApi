// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Query;
using Microsoft.Data.OData.Query.SemanticAst;

namespace System.Web.Http.OData.Query
{
    public class OrderByPropertyNode
    {
        public OrderByPropertyNode(Stack<OrderByQueryNode> nodes)
        {
            if (nodes == null)
            {
                throw Error.ArgumentNull("nodes");
            }

            if (nodes.Count == 0)
            {
                throw new ODataException(SRResources.OrderByNodeNotFound);
            }

            OrderByQueryNode currentNode = nodes.Pop();
            PropertyAccessQueryNode property = currentNode.Expression as PropertyAccessQueryNode;

            if (property == null)
            {
                throw new ODataException(SRResources.OrderByPropertyNotFound);
            }

            Property = property.Property;
            Direction = currentNode.Direction;

            if (nodes.Count > 0)
            {
                ThenBy = new OrderByPropertyNode(nodes);
            }
        }

        public IEdmProperty Property { get; private set; }

        public OrderByDirection Direction { get; private set; }

        public OrderByPropertyNode ThenBy { get; private set; }

        public static OrderByPropertyNode Create(OrderByQueryNode node)
        {
            Stack<OrderByQueryNode> nodes = new Stack<OrderByQueryNode>();
            OrderByQueryNode currentNode = node;
            while (currentNode != null)
            {
                nodes.Push(currentNode);
                currentNode = currentNode.Collection as OrderByQueryNode;
            }
            return new OrderByPropertyNode(nodes);
        }
    }
}
