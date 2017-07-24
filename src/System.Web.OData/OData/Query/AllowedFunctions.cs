﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace System.Web.OData.Query
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
        /// A value that corresponds to allowing 'Contains' function in $filter.
        /// </summary>
        Contains = 0x4,

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
        /// A value that corresponds to allowing 'Date' function in $filter.
        /// </summary>
        Date = 0x1000,

        /// <summary>
        /// A value that corresponds to allowing 'Month' function in $filter.
        /// </summary>
        Month = 0x2000,

        /// <summary>
        /// A value that corresponds to allowing 'Time' function in $filter.
        /// </summary>
        Time = 0x4000,

        /// <summary>
        /// A value that corresponds to allowing 'Day' function in $filter.
        /// </summary>
        Day = 0x8000,

        /// <summary>
        /// A value that corresponds to allowing 'Hour' function in $filter.
        /// </summary>
        Hour = 0x20000,

        /// <summary>
        /// A value that corresponds to allowing 'Minute' function in $filter.
        /// </summary>
        Minute = 0x80000,

        /// <summary>
        /// A value that corresponds to allowing 'Second' function in $filter.
        /// </summary>
        Second = 0x200000,

        /// <summary>
        /// A value that corresponds to allowing 'Fractionalseconds' function in $filter.
        /// </summary>
        FractionalSeconds = 0x400000,

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
        AllStringFunctions = StartsWith | EndsWith | Contains | Length | IndexOf | Concat | Substring | ToLower | ToUpper | Trim,

        /// <summary>
        /// A value that corresponds to allowing all datetime related functions in $filter.
        /// </summary>
        AllDateTimeFunctions = Year | Month | Day | Hour | Minute | Second | FractionalSeconds | Date | Time,

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