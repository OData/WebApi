//-----------------------------------------------------------------------------
// <copyright file="OrderItem.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNet.OData.Builder;

namespace Microsoft.AspNet.OData.Test.Builder.TestModels
{
    public class OrderItem
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public int OrderId { get; set; }

        [Contained]
        public IList<OrderItemDetail> OrderItemDetails { get; set; }
    }
}
