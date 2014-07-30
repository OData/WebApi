// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Xml.Linq;
using Microsoft.TestCommon.Types;

namespace System.Web.OData.Query.Expressions
{
    public class Product
    {
        public int ProductID { get; set; }

        public string ProductName { get; set; }
        public int SupplierID { get; set; }
        public int CategoryID { get; set; }
        public string QuantityPerUnit { get; set; }
        public decimal? UnitPrice { get; set; }
        public double? Weight { get; set; }
        public float? Width { get; set; }
        public short? UnitsInStock { get; set; }
        public short? UnitsOnOrder { get; set; }

        public short? ReorderLevel { get; set; }
        public bool? Discontinued { get; set; }
        public DateTimeOffset? DiscontinuedDate { get; set; }
        public DateTimeOffset NonNullableDiscontinuedDate { get; set; }

        public DateTimeOffset DiscontinuedOffset { get; set; }
        public TimeSpan DiscontinuedSince { get; set; }

        public ushort? UnsignedReorderLevel { get; set; }

        public Category Category { get; set; }

        public Address SupplierAddress { get; set; }

        public int[] AlternateIDs { get; set; }
        public Address[] AlternateAddresses { get; set; }
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
        public DateTimeOffset DateTimeProp { get; set; }
        public DateTimeOffset DateTimeOffsetProp { get; set; }
        public byte[] ByteArrayProp { get; set; }
        public byte[] ByteArrayPropWithNullValue { get; set; }
        public TimeSpan TimeSpanProp { get; set; }
        public decimal DecimalProp { get; set; }
        public double DoubleProp { get; set; }
        public float FloatProp { get; set; }
        public Single SingleProp { get; set; }
        public long LongProp { get; set; }
        public int IntProp { get; set; }
        public string StringProp { get; set; }
        public bool BoolProp { get; set; }

        public ushort UShortProp { get; set; }
        public uint UIntProp { get; set; }
        public ulong ULongProp { get; set; }
        public char CharProp { get; set; }
        public byte ByteProp { get; set; }

        public short? NullableShortProp { get; set; }
        public int? NullableIntProp { get; set; }
        public long? NullableLongProp { get; set; }
        public Single? NullableSingleProp { get; set; }
        public double? NullableDoubleProp { get; set; }
        public decimal? NullableDecimalProp { get; set; }
        public bool? NullableBoolProp { get; set; }
        public byte? NullableByteProp { get; set; }
        public Guid? NullableGuidProp { get; set; }
        public DateTimeOffset? NullableDateTimeOffsetProp { get; set; }
        public TimeSpan? NullableTimeSpanProp { get; set; }

        public ushort? NullableUShortProp { get; set; }
        public uint? NullableUIntProp { get; set; }
        public ulong? NullableULongProp { get; set; }
        public char? NullableCharProp { get; set; }

        public char[] CharArrayProp { get; set; }
        public Binary BinaryProp { get; set; }
        public XElement XElementProp { get; set; }

        public SimpleEnum SimpleEnumProp { get; set; }
        public FlagsEnum FlagsEnumProp { get; set; }
        public LongEnum LongEnumProp { get; set; }
        public SimpleEnum? NullableSimpleEnumProp { get; set; }

        public Product EntityProp { get;set; }
        public Address ComplexProp { get; set; }

        public string Inaccessible() { return String.Empty; }
    }

    public class DerivedProduct : Product
    {
        public string DerivedProductName { get; set; }
    }

    public class DerivedCategory : Category
    {
        public string DerivedCategoryName { get; set; }
    }
}
