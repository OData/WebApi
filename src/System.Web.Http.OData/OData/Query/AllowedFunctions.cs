// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.OData.Query
{
    [Flags]
    public enum AllowedFunctions
    {
        None = 0x0,
        StartsWith = 0x1,
        EndsWith = 0x2,
        SubstringOf = 0x4,
        Length = 0x8,
        IndexOf = 0x10,
        Concat = 0x20,
        Substring = 0x40,
        ToLower = 0x80,
        ToUpper = 0x100,
        Trim = 0x200,
        Cast = 0x400,
        Year = 0x800,
        Years = 0x1000,
        Month = 0x2000,
        Months = 0x4000,
        Day = 0x8000,
        Days = 0x10000,
        Hour = 0x20000,
        Hours = 0x40000,
        Minute = 0x80000,
        Minutes = 0x100000,
        Second = 0x200000,
        Seconds = 0x400000,
        Round = 0x800000,
        Floor = 0x1000000,
        Ceiling = 0x2000000,
        IsOf = 0x4000000,
        Any = 0x8000000,
        All = 0x10000000,
        AllStringFunctions = StartsWith | EndsWith | SubstringOf | Length | IndexOf | Concat | Substring | ToLower | ToUpper | Trim,
        AllDateTimeFunctions = Year | Years | Month | Months | Day | Days | Hour | Hours | Minute | Minutes | Second | Seconds,
        AllMathFunctions = Round | Floor | Ceiling,
        AllFunctions = AllStringFunctions | AllDateTimeFunctions | AllMathFunctions | Cast | IsOf | Any | All
    }
}
