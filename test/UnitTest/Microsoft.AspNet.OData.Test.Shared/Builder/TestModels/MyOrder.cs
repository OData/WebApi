//-----------------------------------------------------------------------------
// <copyright file="MyOrder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.OData.Builder;

namespace Microsoft.AspNet.OData.Test.Builder.TestModels
{
    public class MyOrder
    {
        public int ID { get; set; }

        public string Name { get; set; }
        
        [Contained]
        public virtual ICollection<OrderLine> OrderLines { get; set; }

        [Contained]
        public virtual ICollection<OrderTest> OrderTests { get; set; }

        [Required]
        [Contained]
        public virtual OrderHeader OrderHeader { get; set; }

        [Contained]
        public virtual OrderCancellation OrderCancellation { get; set; }
    }
}
