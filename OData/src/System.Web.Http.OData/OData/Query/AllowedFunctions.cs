// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.OData.Query
{
    /// <summary>
    /// Functions to allow for querying using $filter.
    /// </summary>
    [Flags]
    public enum AllowedFunctions
    {
        /// <summary>
        /// A value that corresponds to allowing no functions in $filter.
        /// </summary>
        None = 0x0,

        /// <summary>
        /// A value that corresponds to allowing 'StartsWith' function in $filter.
        /// </summary>
        StartsWith = 0x1,

        /// <summary>
        /// A value that corresponds to allowing 'EndsWith' function in $filter.
        /// </summary>
        EndsWith = 0x2,

        /// <summary>
        /// A value that corresponds to allowing 'SubstringOf' function in $filter.
        /// </summary>
        SubstringOf = 0x4,

        /// <summary>
        /// A value that corresponds to allowing 'Length' function in $filter.
        /// </summary>
        Length = 0x8,

        /// <summary>
        /// A value that corresponds to allowing 'IndexOf' function in $filter.
        /// </summary>
        IndexOf = 0x10,

        /// <summary>
        /// A value that corresponds to allowing 'Concat' function in $filter.
        /// </summary>
        Concat = 0x20,

        /// <summary>
        /// A value that corresponds to allowing 'Substring' function in $filter.
        /// </summary>
        Substring = 0x40,

        /// <summary>
        /// A value that corresponds to allowing 'ToLower' function in $filter.
        /// </summary>
        ToLower = 0x80,

        /// <summary>
        /// A value that corresponds to allowing 'ToUpper' function in $filter.
        /// </summary>
        ToUpper = 0x100,

        /// <summary>
        /// A value that corresponds to allowing 'Trim' function in $filter.
        /// </summary>
        Trim = 0x200,

        /// <summary>
        /// A value that corresponds to allowing 'Cast' function in $filter.
        /// </summary>
        Cast = 0x400,

        /// <summary>
        /// A value that corresponds to allowing 'Year' function in $filter.
        /// </summary>
        Year = 0x800,

        /// <summary>
        /// A value that corresponds to allowing 'Years' function in $filter.
        /// </summary>
        Years = 0x1000,

        /// <summary>
        /// A value that corresponds to allowing 'Month' function in $filter.
        /// </summary>
        Month = 0x2000,

        /// <summary>
        /// A value that corresponds to allowing 'Months' function in $filter.
        /// </summary>
        Months = 0x4000,

        /// <summary>
        /// A value that corresponds to allowing 'Day' function in $filter.
        /// </summary>
        Day = 0x8000,

        /// <summary>
        /// A value that corresponds to allowing 'Days' function in $filter.
        /// </summary>
        Days = 0x10000,

        /// <summary>
        /// A value that corresponds to allowing 'Hour' function in $filter.
        /// </summary>
        Hour = 0x20000,

        /// <summary>
        /// A value that corresponds to allowing 'Hours' function in $filter.
        /// </summary>
        Hours = 0x40000,

        /// <summary>
        /// A value that corresponds to allowing 'Minute' function in $filter.
        /// </summary>
        Minute = 0x80000,

        /// <summary>
        /// A value that corresponds to allowing 'Minutes' function in $filter.
        /// </summary>
        Minutes = 0x100000,

        /// <summary>
        /// A value that corresponds to allowing 'Second' function in $filter.
        /// </summary>
        Second = 0x200000,

        /// <summary>
        /// A value that corresponds to allowing 'Seconds' function in $filter.
        /// </summary>
        Seconds = 0x400000,

        /// <summary>
        /// A value that corresponds to allowing 'Round' function in $filter.
        /// </summary>
        Round = 0x800000,

        /// <summary>
        /// A value that corresponds to allowing 'Floor' function in $filter.
        /// </summary>
        Floor = 0x1000000,

        /// <summary>
        /// A value that corresponds to allowing 'Ceiling' function in $filter.
        /// </summary>
        Ceiling = 0x2000000,

        /// <summary>
        /// A value that corresponds to allowing 'IsOf' function in $filter.
        /// </summary>
        IsOf = 0x4000000,

        /// <summary>
        /// A value that corresponds to allowing 'Any' function in $filter.
        /// </summary>
        Any = 0x8000000,

        /// <summary>
        /// A value that corresponds to allowing 'All' function in $filter.
        /// </summary>
        All = 0x10000000,

        /// <summary>
        /// A value that corresponds to allowing all string related functions in $filter.
        /// </summary>
        AllStringFunctions = StartsWith | EndsWith | SubstringOf | Length | IndexOf | Concat | Substring | ToLower | ToUpper | Trim,

        /// <summary>
        /// A value that corresponds to allowing all datetime related functions in $filter.
        /// </summary>
        AllDateTimeFunctions = Year | Years | Month | Months | Day | Days | Hour | Hours | Minute | Minutes | Second | Seconds,

        /// <summary>
        /// A value that corresponds to allowing math related functions in $filter.
        /// </summary>
        AllMathFunctions = Round | Floor | Ceiling,

        /// <summary>
        /// A value that corresponds to allowing all functions in $filter.
        /// </summary>
        AllFunctions = AllStringFunctions | AllDateTimeFunctions | AllMathFunctions | Cast | IsOf | Any | All
    }
}