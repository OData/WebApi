// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace System.Web.Http.OData.Query.Expressions
{
    public class Product
    {
        public int ProductID { get; set; }

        public string ProductName { get; set; }
        public int SupplierID { get; set; }
        public int CategoryID { get; set; }
        public string QuantityPerUnit { get; set; }
        public decimal? UnitPrice { get; set; }
        public short? UnitsInStock { get; set; }
        public short? UnitsOnOrder { get; set; }

        public short? ReorderLevel { get; set; }
        public bool? Discontinued { get; set; }
        public DateTime? DiscontinuedDate { get; set; }
        public DateTime NonNullableDiscontinuedDate { get; set; }

        public Category Category { get; set; }

        public Address SupplierAddress { get; set; }
    }

    public class Category
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }

        public Product Product { get; set; }

        public IEnumerable<Product> Products { get; set; }

        public IEnumerable<Product> EnumerableProducts { get; set; }
        public IQueryable<Product> QueryableProducts { get; set; }
    }

    public class Address
    {
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
    }

    public class DataTypes
    {
        public int Id { get; set; }
        public Guid GuidProp { get; set; }
        public DateTime DateTimeProp { get; set; }
        public DateTimeOffset DateTimeOffsetProp { get; set; }
        public byte[] ByteArrayProp { get; set; }
        public TimeSpan TimeSpanProp { get; set; }
        public decimal DecimalProp { get; set; }
        public double DoubleProp { get; set; }
        public float FloatProp { get; set; }
        public long LongProp { get; set; }
        public int IntProp { get; set; }

        public string Inaccessable() { return String.Empty; }
    }
}
