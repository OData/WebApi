// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Web.Http.OData
{
    internal class ExpressionHelperMethods
    {
        private static MethodInfo _orderByMethod = GenericMethodOf(_ => Queryable.OrderBy<int, int>(default(IQueryable<int>), default(Expression<Func<int, int>>)));
        private static MethodInfo _orderByDescendingMethod = GenericMethodOf(_ => Queryable.OrderByDescending<int, int>(default(IQueryable<int>), default(Expression<Func<int, int>>)));
        private static MethodInfo _thenByMethod = GenericMethodOf(_ => Queryable.ThenBy<int, int>(default(IOrderedQueryable<int>), default(Expression<Func<int, int>>)));
        private static MethodInfo _thenByDescendingMethod = GenericMethodOf(_ => Queryable.ThenByDescending<int, int>(default(IOrderedQueryable<int>), default(Expression<Func<int, int>>)));
        private static MethodInfo _takeMethod = GenericMethodOf(_ => Queryable.Take<int>(default(IQueryable<int>), default(int)));
        private static MethodInfo _skipMethod = GenericMethodOf(_ => Queryable.Skip<int>(default(IQueryable<int>), default(int)));
        private static MethodInfo _whereMethod = GenericMethodOf(_ => Queryable.Where<int>(default(IQueryable<int>), default(Expression<Func<int, bool>>)));

        private static MethodInfo _queryableEmptyAnyMethod = GenericMethodOf(_ => Queryable.Any<int>(default(IQueryable<int>)));
        private static MethodInfo _queryableNonEmptyAnyMethod = GenericMethodOf(_ => Queryable.Any<int>(default(IQueryable<int>), default(Expression<Func<int, bool>>)));
        private static MethodInfo _queryableAllMethod = GenericMethodOf(_ => Queryable.All(default(IQueryable<int>), default(Expression<Func<int, bool>>)));

        private static MethodInfo _enumerableEmptyAnyMethod = GenericMethodOf(_ => Enumerable.Any<int>(default(IEnumerable<int>)));
        private static MethodInfo _enumerableNonEmptyAnyMethod = GenericMethodOf(_ => Enumerable.Any<int>(default(IEnumerable<int>), default(Func<int, bool>)));
        private static MethodInfo _enumerableAllMethod = GenericMethodOf(_ => Enumerable.All<int>(default(IEnumerable<int>), default(Func<int, bool>)));

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

        public static MethodInfo QueryableTakeGeneric
        {
            get { return _takeMethod; }
        }

        public static MethodInfo QueryableSkipGeneric
        {
            get { return _skipMethod; }
        }

        public static MethodInfo QueryableWhereGeneric
        {
            get { return _whereMethod; }
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
