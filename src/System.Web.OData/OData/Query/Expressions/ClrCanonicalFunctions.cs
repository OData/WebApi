// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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
        internal const string YearsFunctionName = "years";
        internal const string MonthFunctionName = "month";
        internal const string MonthsFunctionName = "months";
        internal const string DayFunctionName = "day";
        internal const string DaysFunctionName = "days";
        internal const string HourFunctionName = "hour";
        internal const string HoursFunctionName = "hours";
        internal const string MinuteFunctionName = "minute";
        internal const string MinutesFunctionName = "minutes";
        internal const string SecondFunctionName = "second";
        internal const string SecondsFunctionName = "seconds";
        internal const string RoundFunctionName = "round";
        internal const string FloorFunctionName = "floor";
        internal const string CeilingFunctionName = "ceiling";
        internal const string CastFunctionName = "cast";

        // string functions
        public static readonly MethodInfo StartsWith;
        public static readonly MethodInfo EndsWith;
        public static readonly MethodInfo Contains;
        public static readonly MethodInfo SubstringStart;
        public static readonly MethodInfo SubstringStartAndLength;
        public static readonly MethodInfo SubstringStartNoThrow;
        public static readonly MethodInfo SubstringStartAndLengthNoThrow;
        public static readonly MethodInfo IndexOf;
        public static readonly MethodInfo ToLower;
        public static readonly MethodInfo ToUpper;
        public static readonly MethodInfo Trim;
        public static readonly MethodInfo Concat;

        // math functions
        public static readonly MethodInfo CeilingOfDouble;
        public static readonly MethodInfo RoundOfDouble;
        public static readonly MethodInfo FloorOfDouble;

        public static readonly MethodInfo CeilingOfDecimal;
        public static readonly MethodInfo RoundOfDecimal;
        public static readonly MethodInfo FloorOfDecimal;

        // enum functions
        public static readonly MethodInfo HasFlag;

        // Date properties
        public static readonly Dictionary<string, PropertyInfo> DateProperties = new[]
        {
            new KeyValuePair<string, PropertyInfo>(YearFunctionName, typeof(DateTime).GetProperty("Year")),
            new KeyValuePair<string, PropertyInfo>(MonthFunctionName, typeof(DateTime).GetProperty("Month")),
            new KeyValuePair<string, PropertyInfo>(DayFunctionName, typeof(DateTime).GetProperty("Day")),
            new KeyValuePair<string, PropertyInfo>(HourFunctionName, typeof(DateTime).GetProperty("Hour")),
            new KeyValuePair<string, PropertyInfo>(MinuteFunctionName, typeof(DateTime).GetProperty("Minute")),
            new KeyValuePair<string, PropertyInfo>(SecondFunctionName, typeof(DateTime).GetProperty("Second")),
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
        }.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        // TimeSpan properties
        public static readonly Dictionary<string, PropertyInfo> TimeSpanProperties = new[]
        {
            new KeyValuePair<string, PropertyInfo>(YearsFunctionName, typeof(TimeSpan).GetProperty("Years")),
            new KeyValuePair<string, PropertyInfo>(MonthsFunctionName, typeof(TimeSpan).GetProperty("Months")),
            new KeyValuePair<string, PropertyInfo>(DaysFunctionName, typeof(TimeSpan).GetProperty("Days")),
            new KeyValuePair<string, PropertyInfo>(HoursFunctionName, typeof(TimeSpan).GetProperty("Hours")),
            new KeyValuePair<string, PropertyInfo>(MinutesFunctionName, typeof(TimeSpan).GetProperty("Minutes")),
            new KeyValuePair<string, PropertyInfo>(SecondsFunctionName, typeof(TimeSpan).GetProperty("Seconds")),
        }.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        // String Properties
        public static readonly PropertyInfo Length = typeof(string).GetProperty("Length");

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Initialization is order dependent")]
        static ClrCanonicalFunctions()
        {
            StartsWith = MethodOf(_ => _defaultString.StartsWith(default(string)));
            EndsWith = MethodOf(_ => _defaultString.EndsWith(default(string)));
            IndexOf = MethodOf(_ => _defaultString.IndexOf(default(string)));
            SubstringStart = MethodOf(_ => _defaultString.Substring(default(int)));
            SubstringStartAndLength = MethodOf(_ => _defaultString.Substring(default(int), default(int)));
            SubstringStartNoThrow = MethodOf(_ => ClrSafeFunctions.SubstringStart(default(string), default(int)));
            SubstringStartAndLengthNoThrow = MethodOf(_ => ClrSafeFunctions.SubstringStartAndLength(default(string), default(int), default(int)));
            Contains = MethodOf(_ => _defaultString.Contains(default(string)));
            ToLower = MethodOf(_ => _defaultString.ToLower());
            ToUpper = MethodOf(_ => _defaultString.ToUpper());
            Trim = MethodOf(_ => _defaultString.Trim());
            Concat = MethodOf(_ => String.Concat(default(string), default(string)));

            CeilingOfDecimal = MethodOf(_ => Math.Ceiling(default(decimal)));
            RoundOfDecimal = MethodOf(_ => Math.Round(default(decimal)));
            FloorOfDecimal = MethodOf(_ => Math.Floor(default(decimal)));

            CeilingOfDouble = MethodOf(_ => Math.Ceiling(default(double)));
            RoundOfDouble = MethodOf(_ => Math.Round(default(double)));
            FloorOfDouble = MethodOf(_ => Math.Floor(default(double)));

            HasFlag = MethodOf(_ => _defaultEnum.HasFlag(default(Enum)));
        }

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
