// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon.Types;

namespace System.Web.OData.Builder.TestModels
{
    public class EnumModel
    {
        public int Id { get; set; }
        public SimpleEnum Simple { get; set; }
        public SimpleEnum? SimpleNullable { get; set; }
        public LongEnum Long { get; set; }
        public ByteEnum Byte { get; set; }
        public SByteEnum SByte { get; set; }
        public ShortEnum Short { get; set; }
        public UShortEnum UShort { get; set; }
        public UIntEnum UInt { get; set; }
        public FlagsEnum Flag { get; set; }
        public FlagsEnum? FlagNullable { get; set; }
    }
}
