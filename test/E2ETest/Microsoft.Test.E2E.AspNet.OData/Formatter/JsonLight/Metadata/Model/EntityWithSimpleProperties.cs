//-----------------------------------------------------------------------------
// <copyright file="EntityWithSimpleProperties.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight.Metadata.Model
{
    public class EntityWithSimpleProperties
    {
        public int Id { get; set; }
        public int? NullableIntProperty { get; set; }
        public byte[] BinaryProperty { get; set; }
        public bool BooleanProperty { get; set; }
        public TimeSpan DurationProperty { get; set; }
        public decimal DecimalProperty { get; set; }
        public double DoubleProperty { get; set; }
        public float SingleProperty { get; set; }
        public Guid GuidProperty { get; set; }
        public Int16 Int16Property { get; set; }
        public int Int32Property { get; set; }
        public Int64 Int64Property { get; set; }
        public sbyte SbyteProperty { get; set; }
        public DateTimeOffset DateTimeOffsetProperty { get; set; }
        public string StringProperty { get; set; }
        public SimpleEnumeration EnumerationProperty { get; set; }
    }
}
