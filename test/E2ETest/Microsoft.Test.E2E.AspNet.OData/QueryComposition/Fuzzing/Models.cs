//-----------------------------------------------------------------------------
// <copyright file="Models.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace Microsoft.Test.E2E.AspNet.OData.QueryComposition.Fuzzing
{
    public class FuzzingContext : DbContext
    {
        public FuzzingContext()
            : base("FuzzingContext")
        {
        }

        public DbSet<EntityTypeModel1> EntityTypeModel1Set { get; set; }
    }

    public class FuzzingDataInitializer : DropCreateDatabaseAlways<FuzzingContext>
    {
        public static List<EntityTypeModel1> Generate()
        {
            var list = new List<EntityTypeModel1>();

            for (int i = 0; i < 10000; i++)
            {
                var complex1 = new ComplexTypeModel1()
                {
                    BoolProperty = true,
                    ByteArrayProperty = new byte[] { 0x01, 0x02, 0x03 },
                    DateTimeOffsetProperty = new DateTimeOffset(2012, 9, 11, 0, 8, 0, TimeSpan.FromHours(8)),
                    DecimalProperty = 123.123M,
                    GuidProperty = new Guid("9bcaf17a-7414-4e97-89e7-6f84a2f16280"),
                    LongProperty = -9223372036854775808,
                    StringProperty = "stringLiternal",
                    Int32Property = 123,
                    DoubleProperty = 100000.11,
                    FloatProperty = 992.1f,
                };

                var entity2 = new EntityTypeModel2()
                {
                    //ID = i,
                    BoolProperty = true,
                    ByteArrayProperty = new byte[] { 0x01, 0x02, 0x03 },
                    DateTimeOffsetProperty = new DateTimeOffset(2012, 9, 11, 0, 8, 0, TimeSpan.FromHours(8)),
                    DecimalProperty = 123.123M,
                    GuidProperty = new Guid("9bcaf17a-7414-4e97-89e7-6f84a2f16280"),
                    LongProperty = -9223372036854775808,
                    StringProperty = "stringLiternal",
                    Int32Property = 123,
                    DoubleProperty = 100000.11,
                    FloatProperty = 992.1f,
                };

                var entity1 = new EntityTypeModel1()
                {
                    //ID = i,
                    BoolProperty = true,
                    ByteArrayProperty = new byte[] { 0x01, 0x02, 0x03 },
                    DateTimeOffsetProperty = new DateTimeOffset(2012, 9, 11, 0, 8, 0, TimeSpan.FromHours(8)),
                    DecimalProperty = 123.123M,
                    GuidProperty = new Guid("9bcaf17a-7414-4e97-89e7-6f84a2f16280"),
                    LongProperty = -9223372036854775808,
                    StringProperty = "stringLiternal",
                    Int32Property = 123,
                    DoubleProperty = 100000.11,
                    FloatProperty = 992.1f,
                    ComplexTypeProperty = complex1,
                    SingleNavigationProperty = entity2,
                };
                entity2.SingleNavigationProperty = entity1;
                list.Add(entity1);
            }

            return list;
        }

        protected override void Seed(FuzzingContext context)
        {
            context.Configuration.AutoDetectChangesEnabled = false;

            int count = 0;
            var data = Generate();
            foreach (var d in data)
            {
                count++;
                context.Set<EntityTypeModel1>().Add(d);

                if (count % 100 == 0)
                {
                    context.SaveChanges();
                }
            }
            context.SaveChanges();
            base.Seed(context);
        }
    }

    public class EntityTypeModel1
    {
        public int ID { get; set; }
        public string StringProperty { get; set; }
        public DateTimeOffset? DateTimeOffsetProperty { get; set; }
        public Decimal? DecimalProperty { get; set; }
        public long LongProperty { get; set; }
        public bool BoolProperty { get; set; }
        public byte[] ByteArrayProperty { get; set; }
        public Guid GuidProperty { get; set; }
        public int Int32Property { get; set; }
        public UInt16 UInt16Property { get; set; }
        public UInt32 UInt32Property { get; set; }
        public UInt64 UInt64Property { get; set; }
        public char CharProperty { get; set; }
        public char[] CharArrayProperty { get; set; }
        public double? DoubleProperty { get; set; }
        public float? FloatProperty { get; set; }

        public ComplexTypeModel1 ComplexTypeProperty { get; set; }

        public EntityTypeModel2 SingleNavigationProperty { get; set; }
    }

    [ComplexType]
    public class ComplexTypeModel1
    {
        public string StringProperty { get; set; }
        public DateTimeOffset? DateTimeOffsetProperty { get; set; }
        public Decimal? DecimalProperty { get; set; }
        public long LongProperty { get; set; }
        public bool BoolProperty { get; set; }
        public byte[] ByteArrayProperty { get; set; }
        public Guid GuidProperty { get; set; }
        public int Int32Property { get; set; }
        public UInt16 UInt16Property { get; set; }
        public UInt32 UInt32Property { get; set; }
        public UInt64 UInt64Property { get; set; }
        public char CharProperty { get; set; }
        public char[] CharArrayProperty { get; set; }
        public double? DoubleProperty { get; set; }
        public float? FloatProperty { get; set; }
    }

    public class EntityTypeModel2
    {
        [Key, ForeignKey("SingleNavigationProperty")]
        public int ID { get; set; }
        public string StringProperty { get; set; }
        public DateTimeOffset? DateTimeOffsetProperty { get; set; }
        public Decimal? DecimalProperty { get; set; }
        public long LongProperty { get; set; }
        public bool BoolProperty { get; set; }
        public byte[] ByteArrayProperty { get; set; }
        public Guid GuidProperty { get; set; }
        public int Int32Property { get; set; }
        public UInt16 UInt16Property { get; set; }
        public UInt32 UInt32Property { get; set; }
        public UInt64 UInt64Property { get; set; }
        public char CharProperty { get; set; }
        public char[] CharArrayProperty { get; set; }
        public double? DoubleProperty { get; set; }
        public float? FloatProperty { get; set; }

        public virtual EntityTypeModel1 SingleNavigationProperty { get; set; }
    }
}
