using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebStack.QA.Test.OData.ETags
{
    public class ETagsCustomer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IList<string> Notes { get; set; }
        public bool BoolProperty { get; set; }
        public byte ByteProperty { get; set; }
        public char CharProperty { get; set; }
        public decimal DecimalProperty { get; set; }
        public double DoubleProperty { get; set; }
        public short ShortProperty { get; set; }
        public long LongProperty { get; set; }
        public sbyte SbyteProperty { get; set; }
        public float FloatProperty { get; set; }
        public ushort UshortProperty { get; set; }
        public uint UintProperty { get; set; }
        public ulong UlongProperty { get; set; }
        public Guid GuidProperty { get; set; }
        public DateTimeOffset DateTimeOffsetProperty { get; set; }
        [ConcurrencyCheck]
        public string StringWithConcurrencyCheckAttributeProperty { get; set; }
        public ETagsCustomer NestedCustomer { get; set; }
    }
}
