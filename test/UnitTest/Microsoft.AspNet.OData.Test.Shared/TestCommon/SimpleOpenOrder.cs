//-----------------------------------------------------------------------------
// <copyright file="SimpleOpenOrder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.AspNet.OData.Test.Common
{
    public class SimpleOpenOrder
    {
        [Key]
        public int OrderId { get; set; }
        public decimal Cost { get; set; }
        public decimal Price { get; set; }
        public SimpleOpenCustomer Customer { get; set; }
        public DateTimeOffset OrderDate { get; set; }
        public SimpleOpenAddress Address { get; set; }
        public IDictionary<string, object> OrderProperties { get; set; }
    }
}
