//-----------------------------------------------------------------------------
// <copyright file="Supplier.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Models.ProductFamilies
{
    [Flags]
    public enum CountryOrRegion
    {
        USA = 1,
        China = 2,
        Japen = 4,
        Canada = 8,
        India = 16
    }

    public class Supplier
    {
        public Supplier()
        {
            ProductFamilies = new List<ProductFamily>();
            Addresses = new List<Address>();
            Tags = new List<string>();
        }

        public int ID { get; set; }

        public string Name { get; set; }

        public ICollection<Address> Addresses { get; set; }

        public Address MainAddress { get; set; }

        public ICollection<string> Tags { get; set; }

        public virtual ICollection<ProductFamily> ProductFamilies { get; set; }

        public CountryOrRegion CountryOrRegion { get; set; }
    }

    public class ToiletPaperSupplier : Supplier
    {
        public int price { get; set; }
    }
}
