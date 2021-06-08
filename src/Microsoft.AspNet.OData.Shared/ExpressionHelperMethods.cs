// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.AspNet.OData
{
    internal class ExpressionHelperMethods
    {
        private static MethodInfo _enumerableWhereMethod = GenericMethodOf(_ => Enumerable.Where<int>(default(IEnumerable<int>), default(Func<int, bool>)));
        private static MethodInfo _queryableToListMethod = GenericMethodOf(_ => Enumerable.ToList<int>(default(IEnumerable<int>)));
        private static MethodInfo _orderByMethod = GenericMethodOf(_ => Queryable.OrderBy<int, int>(default(IQueryable<int>), default(Expression<Func<int, int>>)));
        private static MethodInfo _enumerableOrderByMethod = GenericMethodOf(_ => Enumerable.OrderBy<int, int>(default(IEnumerable<int>), default(Func<int, int>)));
        private static MethodInfo _orderByDescendingMethod = GenericMethodOf(_ => Queryable.OrderByDescending<int, int>(default(IQueryable<int>), default(Expression<Func<int, int>>)));
        private static MethodInfo _enumerableOrderByDescendingMethod = GenericMethodOf(_ => Enumerable.OrderByDescending<int, int>(default(IEnumerable<int>), default(Func<int, int>)));
        private static MethodInfo _thenByMethod = GenericMethodOf(_ => Queryable.ThenBy<int, int>(default(IOrderedQueryable<int>), default(Expression<Func<int, int>>)));
        private static MethodInfo _enumerableThenByMethod = GenericMethodOf(_ => Enumerable.ThenBy<int, int>(default(IOrderedEnumerable<int>), default(Func<int, int>)));
        private static MethodInfo _thenByDescendingMethod = GenericMethodOf(_ => Queryable.ThenByDescending<int, int>(default(IOrderedQueryable<int>), default(Expression<Func<int, int>>)));
        private static MethodInfo _enumerableThenByDescendingMethod = GenericMethodOf(_ => Enumerable.ThenByDescending<int, int>(default(IOrderedEnumerable<int>), default(Func<int, int>)));
        private static MethodInfo _countMethod = GenericMethodOf(_ => Queryable.LongCount<int>(default(IQueryable<int>)));
        private static MethodInfo _enumerableGroupByMethod = GenericMethodOf(_ => Enumerable.GroupBy<int, int>(default(IQueryable<int>), default(Func<int, int>)));
        private static MethodInfo _groupByMethod = GenericMethodOf(_ => Queryable.GroupBy<int, int>(default(IQueryable<int>), default(Expression<Func<int, int>>)));
        private static MethodInfo _aggregateMethod = GenericMethodOf(_ => Queryable.Aggregate<int, int>(default(IQueryable<int>), default(int), default(Expression<Func<int, int, int>>)));
        private static MethodInfo _skipMethod = GenericMethodOf(_ => Queryable.Skip<int>(default(IQueryable<int>), default(int)));
        private static MethodInfo _enumerableSkipMethod = GenericMethodOf(_ => Enumerable.Skip<int>(default(IEnumerable<int>), default(int)));
        private static MethodInfo _whereMethod = GenericMethodOf(_ => Queryable.Where<int>(default(IQueryable<int>), default(Expression<Func<int, bool>>)));

        private static MethodInfo _queryableCastMethod = GenericMethodOf(_ => Queryable.Cast<int>(default(IQueryable<int>)));
        private static MethodInfo _enumerableCastMethod = GenericMethodOf(_ => Enumerable.Cast<int>(default(IEnumerable<int>)));

        private static MethodInfo _queryableContainsMethod = GenericMethodOf(_ => Queryable.Contains<int>(default(IQueryable<int>), default(int)));
        private static MethodInfo _enumerableContainsMethod = GenericMethodOf(_ => Enumerable.Contains<int>(default(IEnumerable<int>), default(int)));

        private static MethodInfo _queryableEmptyAnyMethod = GenericMethodOf(_ => Queryable.Any<int>(default(IQueryable<int>)));
        private static MethodInfo _queryableNonEmptyAnyMethod = GenericMethodOf(_ => Queryable.Any<int>(default(IQueryable<int>), default(Expression<Func<int, bool>>)));
        private static MethodInfo _queryableAllMethod = GenericMethodOf(_ => Queryable.All(default(IQueryable<int>), default(Expression<Func<int, bool>>)));

        private static MethodInfo _enumerableEmptyAnyMethod = GenericMethodOf(_ => Enumerable.Any<int>(default(IEnumerable<int>)));
        private static MethodInfo _enumerableNonEmptyAnyMethod = GenericMethodOf(_ => Enumerable.Any<int>(default(IEnumerable<int>), default(Func<int, bool>)));
        private static MethodInfo _enumerableAllMethod = GenericMethodOf(_ => Enumerable.All<int>(default(IEnumerable<int>), default(Func<int, bool>)));

        private static MethodInfo _enumerableOfTypeMethod = GenericMethodOf(_ => Enumerable.OfType<int>(default(IEnumerable)));
        private static MethodInfo _queryableOfTypeMethod = GenericMethodOf(_ => Queryable.OfType<int>(default(IQueryable)));

        private static MethodInfo _enumerableSelectManyMethod = GenericMethodOf(_ => Enumerable.SelectMany<int, int>(default(IEnumerable<int>), default(Func<int, IEnumerable<int>>)));
        private static MethodInfo _queryableSelectManyMethod = GenericMethodOf(_ => Queryable.SelectMany<int, int>(default(IQueryable<int>), default(Expression<Func<int, IEnumerable<int>>>)));

        private static MethodInfo _enumerableSelectMethod = GenericMethodOf(_ => Enumerable.Select<int, int>(default(IEnumerable<int>), i => i));
        private static MethodInfo _queryableSelectMethod = GenericMethodOf(_ => Queryable.Select<int, int>(default(IQueryable<int>), i => i));

        private static MethodInfo _queryableTakeMethod = GenericMethodOf(_ => Queryable.Take<int>(default(IQueryable<int>), default(int)));
        private static MethodInfo _enumerableTakeMethod = GenericMethodOf(_ => Enumerable.Take<int>(default(IEnumerable<int>), default(int)));

        private static MethodInfo _queryableAsQueryableMethod = GenericMethodOf(_ => Queryable.AsQueryable<int>(default(IEnumerable<int>)));

        private static MethodInfo _toQueryableMethod = GenericMethodOf(_ => ExpressionHelperMethods.ToQueryable<int>(default(int)));

        private static Dictionary<Type, MethodInfo> _queryableSumMethods = GetQueryableAggregationMethods("Sum");
        private static Dictionary<Type, MethodInfo> _enumerableSumMethods = GetEnumerableAggregationMethods("Sum");

        private static MethodInfo _enumerableMinMethod = GenericMethodOf(_ => Enumerable.Min<int, int>(default(IQueryable<int>), default(Func<int, int>)));
        private static MethodInfo _enumerableMaxMethod = GenericMethodOf(_ => Enumerable.Max<int, int>(default(IQueryable<int>), default(Func<int, int>)));

        private static MethodInfo _enumerableDistinctMethod = GenericMethodOf(_ => Enumerable.Distinct<int>(default(IEnumerable<int>)));

        private static MethodInfo _queryableMinMethod = GenericMethodOf(_ => Queryable.Min<int, int>(default(IQueryable<int>), default(Expression<Func<int, int>>)));
        private static MethodInfo _queryableMaxMethod = GenericMethodOf(_ => Queryable.Max<int, int>(default(IQueryable<int>), default(Expression<Func<int, int>>)));

        private static MethodInfo _queryableDistinctMethod = GenericMethodOf(_ => Queryable.Distinct<int>(default(IQueryable<int>)));

        private static MethodInfo _createQueryGenericMethod = GetCreateQueryGenericMethod();

        //Unlike the Sum method, the return types are not unique and do not match the input type of the expression.
        //Inspecting the 2nd parameters expression's function's 2nd argument is too specific for the GetQueryableAggregationMethods        
        private static Dictionary<Type, MethodInfo> _enumerableAverageMethods = new Dictionary<Type, MethodInfo>()
        {
            { typeof(int), GenericMethodOf(_ => Enumerable.Average<string>(default(IEnumerable<string>), default(Func<string, int>))) },
            { typeof(int?), GenericMethodOf(_ => Enumerable.Average<string>(default(IEnumerable<string>), default(Func<string, int?>))) },
            { typeof(long), GenericMethodOf(_ => Enumerable.Average<string>(default(IEnumerable<string>), default(Func<string, long>))) },
            { typeof(long?), GenericMethodOf(_ => Enumerable.Average<string>(default(IEnumerable<string>), default(Func<string, long?>))) },
            { typeof(float), GenericMethodOf(_ => Enumerable.Average<string>(default(IEnumerable<string>), default(Func<string, float>))) },
            { typeof(float?), GenericMethodOf(_ => Enumerable.Average<string>(default(IEnumerable<string>), default(Func<string, float?>))) },
            { typeof(decimal), GenericMethodOf(_ => Enumerable.Average<string>(default(IEnumerable<string>), default(Func<string, decimal>))) },
            { typeof(decimal?), GenericMethodOf(_ => Enumerable.Average<string>(default(IEnumerable<string>), default(Func<string, decimal?>))) },
            { typeof(double), GenericMethodOf(_ => Enumerable.Average<string>(default(IEnumerable<string>), default(Func<string, double>))) },
            { typeof(double?), GenericMethodOf(_ => Enumerable.Average<string>(default(IEnumerable<string>), default(Func<string, double?>))) },
        };

        private static Dictionary<Type, MethodInfo> _queryableAverageMethods = new Dictionary<Type, MethodInfo>()
        {
            { typeof(int), GenericMethodOf(_ => Queryable.Average<string>(default(IQueryable<string>), default(Expression<Func<string, int>>))) },
            { typeof(int?), GenericMethodOf(_ => Queryable.Average<string>(default(IQueryable<string>), default(Expression<Func<string, int?>>))) },
            { typeof(long), GenericMethodOf(_ => Queryable.Average<string>(default(IQueryable<string>), default(Expression<Func<string, long>>))) },
            { typeof(long?), GenericMethodOf(_ => Queryable.Average<string>(default(IQueryable<string>), default(Expression<Func<string, long?>>))) },
            { typeof(float), GenericMethodOf(_ => Queryable.Average<string>(default(IQueryable<string>), default(Expression<Func<string, float>>))) },
            { typeof(float?), GenericMethodOf(_ => Queryable.Average<string>(default(IQueryable<string>), default(Expression<Func<string, float?>>))) },
            { typeof(decimal), GenericMethodOf(_ => Queryable.Average<string>(default(IQueryable<string>), default(Expression<Func<string, decimal>>))) },
            { typeof(decimal?), GenericMethodOf(_ => Queryable.Average<string>(default(IQueryable<string>), default(Expression<Func<string, decimal?>>))) },
            { typeof(double), GenericMethodOf(_ => Queryable.Average<string>(default(IQueryable<string>), default(Expression<Func<string, double>>))) },
            { typeof(double?), GenericMethodOf(_ => Queryable.Average<string>(default(IQueryable<string>), default(Expression<Func<string, double?>>))) },
        };

        private static MethodInfo _enumerableCountMethod = GenericMethodOf(_ => Enumerable.LongCount<int>(default(IEnumerable<int>)));

        private static MethodInfo _safeConvertToDecimalMethod = typeof(ExpressionHelperMethods).GetMethod("SafeConvertToDecimal");

#if NETCORE // Not supported in NETFX
        private static MethodInfo _queryableCreateSingleResultMethod = GenericMethodOf(_ => SingleResult.Create<int>(default(IQueryable<int>)));
#endif

        public static MethodInfo EnumerableWhereGeneric
        {
            get { return _enumerableWhereMethod; }
        }

        public static MethodInfo QueryableToList
        {
            get { return _queryableToListMethod; }
        }

        public static MethodInfo QueryableOrderByGeneric
        {
            get { return _orderByMethod; }
        }

        public static MethodInfo EnumerableOrderByGeneric
        {
            get { return _enumerableOrderByMethod; }
        }

        public static MethodInfo QueryableOrderByDescendingGeneric
        {
            get { return _orderByDescendingMethod; }
        }

        public static MethodInfo EnumerableOrderByDescendingGeneric
        {
            get { return _enumerableOrderByDescendingMethod; }
        }

        public static MethodInfo QueryableThenByGeneric
        {
            get { return _thenByMethod; }
        }

        public static MethodInfo EnumerableThenByGeneric
        {
            get { return _enumerableThenByMethod; }
        }

        public static MethodInfo QueryableThenByDescendingGeneric
        {
            get { return _thenByDescendingMethod; }
        }

        public static MethodInfo EnumerableThenByDescendingGeneric
        {
            get { return _enumerableThenByDescendingMethod; }
        }

        public static MethodInfo QueryableCountGeneric
        {
            get { return _countMethod; }
        }

        public static Dictionary<Type, MethodInfo> QueryableSumGenerics
        {
            get { return _queryableSumMethods; }
        }

        public static Dictionary<Type, MethodInfo> EnumerableSumGenerics
        {
            get { return _enumerableSumMethods; }
        }

        public static MethodInfo QueryableMin
        {
            get { return _queryableMinMethod; }
        }

        public static MethodInfo EnumerableMin
        {
            get { return _enumerableMinMethod; }
        }

        public static MethodInfo QueryableMax
        {
            get { return _queryableMaxMethod; }
        }

        public static MethodInfo EnumerableMax
        {
            get { return _enumerableMaxMethod; }
        }

        public static Dictionary<Type, MethodInfo> QueryableAverageGenerics
        {
            get { return _queryableAverageMethods; }
        }

        public static Dictionary<Type, MethodInfo> EnumerableAverageGenerics
        {
            get { return _enumerableAverageMethods; }
        }

        public static MethodInfo QueryableDistinct
        {
            get { return _queryableDistinctMethod; }
        }

        public static MethodInfo EnumerableDistinct
        {
            get { return _enumerableDistinctMethod; }
        }

        public static MethodInfo QueryableGroupByGeneric
        {
            get { return _groupByMethod; }
        }

        public static MethodInfo EnumerableGroupByGeneric
        {
            get { return _enumerableGroupByMethod; }
        }

        public static MethodInfo QueryableAggregateGeneric
        {
            get { return _aggregateMethod; }
        }

        public static MethodInfo QueryableTakeGeneric
        {
            get { return _queryableTakeMethod; }
        }

        public static MethodInfo EnumerableTakeGeneric
        {
            get { return _enumerableTakeMethod; }
        }

        public static MethodInfo QueryableSkipGeneric
        {
            get { return _skipMethod; }
        }

        public static MethodInfo EnumerableSkipGeneric
        {
            get { return _enumerableSkipMethod; }
        }

        public static MethodInfo QueryableWhereGeneric
        {
            get { return _whereMethod; }
        }

        public static MethodInfo QueryableCastGeneric
        {
            get { return _queryableCastMethod; }
        }

        public static MethodInfo EnumerableCastGeneric
        {
            get { return _enumerableCastMethod; }
        }

        public static MethodInfo QueryableContainsGeneric
        {
            get { return _queryableContainsMethod; }
        }

        public static MethodInfo EnumerableContainsGeneric
        {
            get { return _enumerableContainsMethod; }
        }

        public static MethodInfo QueryableSelectGeneric
        {
            get { return _queryableSelectMethod; }
        }

        public static MethodInfo EnumerableSelectGeneric
        {
            get { return _enumerableSelectMethod; }
        }

        public static MethodInfo QueryableSelectManyGeneric
        {
            get { return _queryableSelectManyMethod; }
        }

        public static MethodInfo EnumerableSelectManyGeneric
        {
            get { return _enumerableSelectManyMethod; }
        }

        public static MethodInfo QueryableEmptyAnyGeneric
        {
            get { return _queryableEmptyAnyMethod; }
        }

        public static MethodInfo QueryableNonEmptyAnyGeneric
        {
            get { return _queryableNonEmptyAnyMethod; }
        }

        public static MethodInfo QueryableAllGeneric
        {
            get { return _queryableAllMethod; }
        }

        public static MethodInfo EnumerableEmptyAnyGeneric
        {
            get { return _enumerableEmptyAnyMethod; }
        }

        public static MethodInfo EnumerableNonEmptyAnyGeneric
        {
            get { return _enumerableNonEmptyAnyMethod; }
        }

        public static MethodInfo EnumerableAllGeneric
        {
            get { return _enumerableAllMethod; }
        }

        public static MethodInfo EnumerableOfType
        {
            get { return _enumerableOfTypeMethod; }
        }

        public static MethodInfo QueryableOfType
        {
            get { return _queryableOfTypeMethod; }
        }

        public static MethodInfo QueryableAsQueryable
        {
            get { return _queryableAsQueryableMethod; }
        }

        public static MethodInfo EntityAsQueryable
        {
            get { return _toQueryableMethod; }
        }

        public static IQueryable ToQueryable<T>(T value)
        {
            return (new List<T> { value }).AsQueryable();
        }

        public static MethodInfo EnumerableCountGeneric
        {
            get { return _enumerableCountMethod; }
        }

        public static MethodInfo ConvertToDecimal
        {
            get { return _safeConvertToDecimalMethod; }
        }

        public static MethodInfo CreateQueryGeneric
        {
            get { return _createQueryGenericMethod; }
        }

#if NETCORE // Not supported in NETFX
        public static MethodInfo QueryableCreateSingleResult
        {
            get { return _queryableCreateSingleResultMethod; }
        }
#endif

        public static decimal? SafeConvertToDecimal(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return null;
            }

            Type type = value.GetType();
            type = Nullable.GetUnderlyingType(type) ?? type;
            if (type == typeof(short) ||
                type == typeof(int) ||
                type == typeof(long) ||
                type == typeof(decimal) ||
                type == typeof(double) ||
                type == typeof(float))
            {
                return (decimal?)Convert.ChangeType(value, typeof(decimal), CultureInfo.InvariantCulture);
            }

            return null;
        }

        private static MethodInfo GenericMethodOf<TReturn>(Expression<Func<object, TReturn>> expression)
        {
            return GenericMethodOf(expression as Expression);
        }

        private static MethodInfo GenericMethodOf(Expression expression)
        {
            LambdaExpression lambdaExpression = expression as LambdaExpression;

            Contract.Assert(expression.NodeType == ExpressionType.Lambda);
            Contract.Assert(lambdaExpression != null);
            Contract.Assert(lambdaExpression.Body.NodeType == ExpressionType.Call);

            return (lambdaExpression.Body as MethodCallExpression).Method.GetGenericMethodDefinition();
        }

        private static Dictionary<Type, MethodInfo> GetQueryableAggregationMethods(string methodName)
        {
            //Sum to not have generic by property method return type so have to generate a table
            // Looking for methods like
            // Queryable.Sum<TSource>(default(IQueryable<TSource>), default(Expression<Func<TSource, int?>>)))

            return typeof(Queryable).GetMethods()
                .Where(m => m.Name == methodName)
                .Where(m => m.GetParameters().Count() == 2)
                .ToDictionary(m => m.ReturnType);
        }

        private static Dictionary<Type, MethodInfo> GetEnumerableAggregationMethods(string methodName)
        {
            //Sum to not have generic by property method return type so have to generate a table
            // Looking for methods like
            // Queryable.Sum<TSource>(default(IQueryable<TSource>), default(Expression<Func<TSource, int?>>)))

            return typeof(Enumerable).GetMethods()
                .Where(m => m.Name == methodName)
                .Where(m => m.GetParameters().Count() == 2)
                .ToDictionary(m => m.ReturnType);
        }

        private static MethodInfo GetCreateQueryGenericMethod()
        {
            return typeof(IQueryProvider).GetTypeInfo()
                .GetDeclaredMethods("CreateQuery")
                .Where(m => m.IsGenericMethod)
                .FirstOrDefault();
        }
    }
}
