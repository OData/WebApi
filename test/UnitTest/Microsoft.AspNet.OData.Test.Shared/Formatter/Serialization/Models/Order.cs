//-----------------------------------------------------------------------------
// <copyright file="Order.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.AspNet.OData.Test.Formatter.Serialization.Models
{
    public class Order
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public Customer Customer { get; set; }
        public IDictionary<string, object> OrderProperties { get; set; }
    }
}
