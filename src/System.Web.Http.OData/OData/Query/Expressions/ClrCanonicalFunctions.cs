// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Web.Http.OData.Query.Expressions
{
    internal class ClrCanonicalFunctions
    {
        private static string _defaultString = default(string);

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
        public static readonly MethodInfo Ceiling;
        public static readonly MethodInfo Round;
        public static readonly MethodInfo Floor;

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

            Ceiling = MethodOf(_ => Math.Ceiling(default(decimal)));
            Round = MethodOf(_ => Math.Round(default(decimal)));
            Floor = MethodOf(_ => Math.Floor(default(decimal)));
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
