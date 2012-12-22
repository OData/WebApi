// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Data.Edm;
using Microsoft.Data.OData.Query;

namespace System.Web.Http.OData
{
    internal class ExpressionHelpers
    {
        public static long Count(IQueryable query, Type type)
        {
            MethodInfo countMethod = ExpressionHelperMethods.QueryableCountGeneric.MakeGenericMethod(type);
            return (long)countMethod.Invoke(null, new object[] { query });
        }

        public static IQueryable<TEntityType> Skip<TEntityType>(IQueryable<TEntityType> query, int count)
        {
            return Skip(query, count, typeof(TEntityType)) as IQueryable<TEntityType>;
        }

        public static IQueryable Skip(IQueryable query, int count, Type type)
        {
            MethodInfo skipMethod = ExpressionHelperMethods.QueryableSkipGeneric.MakeGenericMethod(type);
            return skipMethod.Invoke(null, new object[] { query, count }) as IQueryable;
        }

        public static IQueryable<TEntityType> Take<TEntityType>(IQueryable<TEntityType> query, int count)
        {
            return Take(query, count, typeof(TEntityType)) as IQueryable<TEntityType>;
        }

        public static IQueryable Take(IQueryable query, int count, Type type)
        {
            MethodInfo takeMethod = ExpressionHelperMethods.QueryableTakeGeneric.MakeGenericMethod(type);
            return takeMethod.Invoke(null, new object[] { query, count }) as IQueryable;
        }

        public static IQueryable<TEntityType> OrderBy<TEntityType>(IQueryable<TEntityType> query, IEdmProperty property, OrderByDirection direction, bool alreadyOrdered = false)
        {
            return OrderByProperty(query, property, direction, typeof(TEntityType), alreadyOrdered) as IQueryable<TEntityType>;
        }

        public static IQueryable OrderByIt(IQueryable query, OrderByDirection direction, Type type, bool alreadyOrdered = false)
        {
            ParameterExpression odataItParameter = Expression.Parameter(type, "$it");
            LambdaExpression orderByLambda = Expression.Lambda(odataItParameter, odataItParameter);
            return OrderBy(query, orderByLambda, direction, type, alreadyOrdered);
        }

        public static IQueryable OrderByProperty(IQueryable query, IEdmProperty property, OrderByDirection direction, Type type, bool alreadyOrdered = false)
        {
            LambdaExpression orderByLambda = GetPropertyAccessLambda(type, property.Name);
            return OrderBy(query, orderByLambda, direction, type, alreadyOrdered);
        }

        private static IQueryable OrderBy(IQueryable query, LambdaExpression orderByLambda, OrderByDirection direction, Type type, bool alreadyOrdered = false)
        {
            Type returnType = orderByLambda.Body.Type;

            MethodInfo orderByMethod = null;
            IOrderedQueryable orderedQuery = null;

            // unfortunately unordered L2O.AsQueryable implements IOrderedQueryable
            // so we can't try casting to IOrderedQueryable to provide a clue to whether
            // we should be calling ThenBy or ThenByDescending
            if (alreadyOrdered)
            {
                if (direction == OrderByDirection.Ascending)
                {
                    orderByMethod = ExpressionHelperMethods.QueryableThenByGeneric.MakeGenericMethod(type, returnType);
                }
                else
                {
                    orderByMethod = ExpressionHelperMethods.QueryableThenByDescendingGeneric.MakeGenericMethod(type, returnType);
                }

                orderedQuery = query as IOrderedQueryable;
                orderedQuery = orderByMethod.Invoke(null, new object[] { orderedQuery, orderByLambda }) as IOrderedQueryable;
            }
            else
            {
                if (direction == OrderByDirection.Ascending)
                {
                    orderByMethod = ExpressionHelperMethods.QueryableOrderByGeneric.MakeGenericMethod(type, returnType);
                }
                else
                {
                    orderByMethod = ExpressionHelperMethods.QueryableOrderByDescendingGeneric.MakeGenericMethod(type, returnType);
                }

                orderedQuery = orderByMethod.Invoke(null, new object[] { query, orderByLambda }) as IOrderedQueryable;
            }

            return orderedQuery;
        }

        public static IQueryable Where(IQueryable query, Expression where, Type type)
        {
            MethodInfo whereMethod = ExpressionHelperMethods.QueryableWhereGeneric.MakeGenericMethod(type);
            return whereMethod.Invoke(null, new object[] { query, where }) as IQueryable;
        }

        private static LambdaExpression GetPropertyAccessLambda(Type type, string propertyName)
        {
            ParameterExpression odataItParameter = Expression.Parameter(type, "$it");
            MemberExpression propertyAccess = Expression.Property(odataItParameter, propertyName);
            return Expression.Lambda(propertyAccess, odataItParameter);
        }
    }
}
