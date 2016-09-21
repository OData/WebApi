// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.OData.Models
{
    [Flags]
    public enum Color
    {
        Red = 1,
        Green = 2,
        Blue = 4
    }

    public enum LongEnum : long
    {
        FirstLong,
        SecondLong,
        ThirdLong,
        FourthLong
    }

    public enum SByteEnum : sbyte
    {
        FirstSByte,
        SecondSByte,
        ThirdSByte
    }

    public enum ShortEnum : short
    {
        FirstShort,
        SecondShort,
        ThirdShort
    }

    public enum SimpleEnum
    {
        First,
        Second,
        Third,
        Fourth
    }

    public enum UIntEnum : uint
    {
        FirstUInt,
        SecondUInt,
        ThirdUInt
    }

    public enum UShortEnum : ushort
    {
        FirstUShort,
        SecondUShort,
        ThirdUShort
    }

    [Flags]
    public enum FlagsEnum
    {
        One = 0x1,
        Two = 0x2,
        Four = 0x4
    }
}
