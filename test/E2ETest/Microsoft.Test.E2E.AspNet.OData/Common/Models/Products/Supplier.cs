//-----------------------------------------------------------------------------
// <copyright file="Supplier.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Models.Products
{
    public class Supplier
    {
        public Supplier()
        {
            Products = new List<Product>();
            Address = new Address();
        }

        public int ID { get; set; }

        public string Name { get; set; }

        public Address Address { get; set; }

        public int Concurrency { get; set; }

        [IgnoreDataMember]
        [XmlIgnore]
        public virtual ICollection<Product> Products { get; set; }
    }
}
