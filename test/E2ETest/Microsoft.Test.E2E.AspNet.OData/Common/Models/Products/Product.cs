//-----------------------------------------------------------------------------
// <copyright file="Product.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Microsoft.OData.Edm;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Models.Products
{
    public class Product
    {
        public Product()
        {
        }

        public int ID { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTimeOffset PublishDate { get; set; }

        public DateTimeOffset? ReleaseDate { get; set; }

        public DateTimeOffset? DiscontinuedDate { get; set; }

        public DateTimeOffset? DateTimeOffset { get; set; }

        public Date Date { get; set; }

        public Date? NullableDate { get; set; }

        public TimeOfDay TimeOfDay { get; set; }

        public TimeOfDay? NullableTimeOfDay { get; set; }

        public int Rating { get; set; }

        public decimal? Price { get; set; }

        public Guid Guid { get; set; }

        public byte[] Binary { get; set; }

        [IgnoreDataMember]
        [XmlIgnore]
        public virtual Category Category { get; set; }

        public virtual Supplier Supplier { get; set; }

        [IgnoreDataMember]
        [XmlIgnore]
        public virtual Product RelatedProduct { get; set; }

        public bool? Taxable { get; set; }
    }
}
