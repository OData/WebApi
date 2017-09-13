﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.OData.Edm;

namespace System.Web.OData.Query.Expressions
{
    internal class ClrCanonicalFunctions
    {
        private static string _defaultString = default(string);
        private static Enum _defaultEnum = default(Enum);

        // function names
        internal const string StartswithFunctionName = "startswith";
        internal const string EndswithFunctionName = "endswith";
        internal const string ContainsFunctionName = "contains";
        internal const string SubstringFunctionName = "substring";
        internal const string LengthFunctionName = "length";
        internal const string IndexofFunctionName = "indexof";
        internal const string TolowerFunctionName = "tolower";
        internal const string ToupperFunctionName = "toupper";
        internal const string TrimFunctionName = "trim";
        internal const string ConcatFunctionName = "concat";
        internal const string YearFunctionName = "year";
        internal const string MonthFunctionName = "month";
        internal const string DayFunctionName = "day";
        internal const string HourFunctionName = "hour";
        internal const string MinuteFunctionName = "minute";
        internal const string SecondFunctionName = "second";
        internal const string MillisecondFunctionName = "millisecond";
        internal const string FractionalSecondsFunctionName = "fractionalseconds";
        internal const string RoundFunctionName = "round";
        internal const string FloorFunctionName = "floor";
        internal const string CeilingFunctionName = "ceiling";
        internal const string CastFunctionName = "cast";
        internal const string IsofFunctionName = "isof";
        internal const string DateFunctionName = "date";
        internal const string TimeFunctionName = "time";

        // string functions
        public static readonly MethodInfo StartsWith = MethodOf(_ => _defaultString.StartsWith(default(string)));
        public static readonly MethodInfo EndsWith = MethodOf(_ => _defaultString.EndsWith(default(string)));
        public static readonly MethodInfo Contains = MethodOf(_ => _defaultString.Contains(default(string)));
        public static readonly MethodInfo SubstringStart = MethodOf(_ => _defaultString.Substring(default(int)));
        public static readonly MethodInfo SubstringStartAndLength = MethodOf(_ => _defaultString.Substring(default(int), default(int)));
        public static readonly MethodInfo SubstringStartNoThrow = MethodOf(_ => ClrSafeFunctions.SubstringStart(default(string), default(int)));
        public static readonly MethodInfo SubstringStartAndLengthNoThrow = MethodOf(_ => ClrSafeFunctions.SubstringStartAndLength(default(string), default(int), default(int)));
        public static readonly MethodInfo IndexOf = MethodOf(_ => _defaultString.IndexOf(default(string)));
        public static readonly MethodInfo ToLower = MethodOf(_ => _defaultString.ToLower());
        public static readonly MethodInfo ToUpper = MethodOf(_ => _defaultString.ToUpper());
        public static readonly MethodInfo Trim = MethodOf(_ => _defaultString.Trim());
        public static readonly MethodInfo Concat = MethodOf(_ => String.Concat(default(string), default(string)));

        // math functions
        public static readonly MethodInfo CeilingOfDouble = MethodOf(_ => Math.Ceiling(default(double)));
        public static readonly MethodInfo RoundOfDouble = MethodOf(_ => Math.Round(default(double)));
        public static readonly MethodInfo FloorOfDouble = MethodOf(_ => Math.Floor(default(double)));

        public static readonly MethodInfo CeilingOfDecimal = MethodOf(_ => Math.Ceiling(default(decimal)));
        public static readonly MethodInfo RoundOfDecimal = MethodOf(_ => Math.Round(default(decimal)));
        public static readonly MethodInfo FloorOfDecimal = MethodOf(_ => Math.Floor(default(decimal)));

        // enum functions
        public static readonly MethodInfo HasFlag = MethodOf(_ => _defaultEnum.HasFlag(default(Enum)));

        // Date properties
        public static readonly Dictionary<string, PropertyInfo> DateProperties = new[]
        {
            new KeyValuePair<string, PropertyInfo>(YearFunctionName, typeof(Date).GetProperty("Year")),
            new KeyValuePair<string, PropertyInfo>(MonthFunctionName, typeof(Date).GetProperty("Month")),
            new KeyValuePair<string, PropertyInfo>(DayFunctionName, typeof(Date).GetProperty("Day")),
        }.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        // DateTimeproperties
        public static readonly Dictionary<string, PropertyInfo> DateTimeProperties = new[]
        {
            new KeyValuePair<string, PropertyInfo>(YearFunctionName, typeof(DateTime).GetProperty("Year")),
            new KeyValuePair<string, PropertyInfo>(MonthFunctionName, typeof(DateTime).GetProperty("Month")),
            new KeyValuePair<string, PropertyInfo>(DayFunctionName, typeof(DateTime).GetProperty("Day")),
            new KeyValuePair<string, PropertyInfo>(HourFunctionName, typeof(DateTime).GetProperty("Hour")),
            new KeyValuePair<string, PropertyInfo>(MinuteFunctionName, typeof(DateTime).GetProperty("Minute")),
            new KeyValuePair<string, PropertyInfo>(SecondFunctionName, typeof(DateTime).GetProperty("Second")),
            new KeyValuePair<string, PropertyInfo>(MillisecondFunctionName, typeof(DateTime).GetProperty("Millisecond")),
        }.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        
        // DateTimeOffset properties
        public static readonly Dictionary<string, PropertyInfo> DateTimeOffsetProperties = new[]
        {
            new KeyValuePair<string, PropertyInfo>(YearFunctionName, typeof(DateTimeOffset).GetProperty("Year")),
            new KeyValuePair<string, PropertyInfo>(MonthFunctionName, typeof(DateTimeOffset).GetProperty("Month")),
            new KeyValuePair<string, PropertyInfo>(DayFunctionName, typeof(DateTimeOffset).GetProperty("Day")),
            new KeyValuePair<string, PropertyInfo>(HourFunctionName, typeof(DateTimeOffset).GetProperty("Hour")),
            new KeyValuePair<string, PropertyInfo>(MinuteFunctionName, typeof(DateTimeOffset).GetProperty("Minute")),
            new KeyValuePair<string, PropertyInfo>(SecondFunctionName, typeof(DateTimeOffset).GetProperty("Second")),
            new KeyValuePair<string, PropertyInfo>(MillisecondFunctionName, typeof(DateTimeOffset).GetProperty("Millisecond")),
        }.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        // TimeOfDay properties
        // ODL uses the Hour(s), Minute(s), Second(s), It's the wrong property name. It should be Hour, Minute, Second.
        public static readonly Dictionary<string, PropertyInfo> TimeOfDayProperties = new[]
        {
            new KeyValuePair<string, PropertyInfo>(HourFunctionName, typeof(TimeOfDay).GetProperty("Hours")),
            new KeyValuePair<string, PropertyInfo>(MinuteFunctionName, typeof(TimeOfDay).GetProperty("Minutes")),
            new KeyValuePair<string, PropertyInfo>(SecondFunctionName, typeof(TimeOfDay).GetProperty("Seconds")),
            new KeyValuePair<string, PropertyInfo>(MillisecondFunctionName, typeof(TimeOfDay).GetProperty("Milliseconds")),
        }.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        // TimeSpan properties
        public static readonly Dictionary<string, PropertyInfo> TimeSpanProperties = new[]
        {
            new KeyValuePair<string, PropertyInfo>(HourFunctionName, typeof(TimeSpan).GetProperty("Hours")),
            new KeyValuePair<string, PropertyInfo>(MinuteFunctionName, typeof(TimeSpan).GetProperty("Minutes")),
            new KeyValuePair<string, PropertyInfo>(SecondFunctionName, typeof(TimeSpan).GetProperty("Seconds")),
            new KeyValuePair<string, PropertyInfo>(MillisecondFunctionName, typeof(TimeSpan).GetProperty("Milliseconds")),
        }.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        // String Properties
        public static readonly PropertyInfo Length = typeof(string).GetProperty("Length");

        // PropertyInfo and MethodInfo of DateTime & DateTimeOffset related.
        public static readonly PropertyInfo DateTimeKindPropertyInfo = typeof(DateTime).GetProperty("Kind");
        public static readonly MethodInfo ToUniversalTimeDateTime = typeof(DateTime).GetMethod("ToUniversalTime", BindingFlags.Instance | BindingFlags.Public);
        public static readonly MethodInfo ToUniversalTimeDateTimeOffset = typeof(DateTimeOffset).GetMethod("ToUniversalTime", BindingFlags.Instance | BindingFlags.Public);
        public static readonly MethodInfo ToOffsetFunction = typeof(DateTimeOffset).GetMethod("ToOffset", BindingFlags.Instance | BindingFlags.Public);
        public static readonly MethodInfo GetUtcOffset = typeof(TimeZoneInfo).GetMethod("GetUtcOffset", new[] { typeof(DateTime) });

        private static MethodInfo MethodOf<TReturn>(Expression<Func<object, TReturn>> expression)
        {
            return MethodOf(expression as Expression);
        }

        private static MethodInfo MethodOf(Expression expression)
        {
            LambdaExpression lambdaExpression = expression as LambdaExpression;
            Contract.Assert(lambdaExpression != null);
            Contract.Assert(expression.NodeType == ExpressionType.Lambda);
            Contract.Assert(lambdaExpression.Body.NodeType == ExpressionType.Call);
            return (lambdaExpression.Body as MethodCallExpression).Method;
        }
    }
}
