// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Web.OData
{
    internal class ExpressionHelperMethods
    {
        private static MethodInfo _orderByMethod = GenericMethodOf(_ => Queryable.OrderBy<int, int>(default(IQueryable<int>), default(Expression<Func<int, int>>)));
        private static MethodInfo _orderByDescendingMethod = GenericMethodOf(_ => Queryable.OrderByDescending<int, int>(default(IQueryable<int>), default(Expression<Func<int, int>>)));
        private static MethodInfo _thenByMethod = GenericMethodOf(_ => Queryable.ThenBy<int, int>(default(IOrderedQueryable<int>), default(Expression<Func<int, int>>)));
        private static MethodInfo _thenByDescendingMethod = GenericMethodOf(_ => Queryable.ThenByDescending<int, int>(default(IOrderedQueryable<int>), default(Expression<Func<int, int>>)));
        private static MethodInfo _countMethod = GenericMethodOf(_ => Queryable.LongCount<int>(default(IQueryable<int>)));
        private static MethodInfo _skipMethod = GenericMethodOf(_ => Queryable.Skip<int>(default(IQueryable<int>), default(int)));
        private static MethodInfo _whereMethod = GenericMethodOf(_ => Queryable.Where<int>(default(IQueryable<int>), default(Expression<Func<int, bool>>)));

        private static MethodInfo _queryableEmptyAnyMethod = GenericMethodOf(_ => Queryable.Any<int>(default(IQueryable<int>)));
        private static MethodInfo _queryableNonEmptyAnyMethod = GenericMethodOf(_ => Queryable.Any<int>(default(IQueryable<int>), default(Expression<Func<int, bool>>)));
        private static MethodInfo _queryableAllMethod = GenericMethodOf(_ => Queryable.All(default(IQueryable<int>), default(Expression<Func<int, bool>>)));

        private static MethodInfo _enumerableEmptyAnyMethod = GenericMethodOf(_ => Enumerable.Any<int>(default(IEnumerable<int>)));
        private static MethodInfo _enumerableNonEmptyAnyMethod = GenericMethodOf(_ => Enumerable.Any<int>(default(IEnumerable<int>), default(Func<int, bool>)));
        private static MethodInfo _enumerableAllMethod = GenericMethodOf(_ => Enumerable.All<int>(default(IEnumerable<int>), default(Func<int, bool>)));

        private static MethodInfo _enumerableOfTypeMethod = GenericMethodOf(_ => Enumerable.OfType<int>(default(IEnumerable)));
        private static MethodInfo _queryableOfTypeMethod = GenericMethodOf(_ => Queryable.OfType<int>(default(IQueryable)));

        private static MethodInfo _enumerableSelectMethod = GenericMethodOf(_ => Enumerable.Select<int, int>(default(IEnumerable<int>), i => i));
        private static MethodInfo _queryableSelectMethod = GenericMethodOf(_ => Queryable.Select<int, int>(default(IQueryable<int>), i => i));

        private static MethodInfo _queryableTakeMethod = GenericMethodOf(_ => Queryable.Take<int>(default(IQueryable<int>), default(int)));
        private static MethodInfo _enumerableTakeMethod = GenericMethodOf(_ => Enumerable.Take<int>(default(IEnumerable<int>), default(int)));

        public static MethodInfo QueryableOrderByGeneric
        {
            get { return _orderByMethod; }
        }

        public static MethodInfo QueryableOrderByDescendingGeneric
        {
            get { return _orderByDescendingMethod; }
        }

        public static MethodInfo QueryableThenByGeneric
        {
            get { return _thenByMethod; }
        }

        public static MethodInfo QueryableThenByDescendingGeneric
        {
            get { return _thenByDescendingMethod; }
        }

        public static MethodInfo QueryableCountGeneric
        {
            get { return _countMethod; }
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

        public static MethodInfo QueryableWhereGeneric
        {
            get { return _whereMethod; }
        }

        public static MethodInfo QueryableSelectGeneric
        {
            get { return _queryableSelectMethod; }
        }

        public static MethodInfo EnumerableSelectGeneric
        {
            get { return _enumerableSelectMethod; }
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
    }
}
