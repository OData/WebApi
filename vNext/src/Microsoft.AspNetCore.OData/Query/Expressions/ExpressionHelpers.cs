// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.OData.Edm;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Query.Expressions
{
    internal static class ExpressionHelpers
    {
        public static Func<long> Count(IQueryable query, Type type)
        {
            MethodInfo countMethod = ExpressionHelperMethods.QueryableCountGeneric.MakeGenericMethod(type);
            Func<long> func = () => (long)countMethod.Invoke(null, new object[] { query });
            return func;
        }

        public static IQueryable Skip(IQueryable query, int count, Type type, bool parameterize)
        {
            MethodInfo skipMethod = ExpressionHelperMethods.QueryableSkipGeneric.MakeGenericMethod(type);
            Expression skipValueExpression = parameterize ? LinqParameterContainer.Parameterize(typeof(int), count) : Expression.Constant(count);

            Expression skipQuery = Expression.Call(null, skipMethod, new[] { query.Expression, skipValueExpression });
            return query.Provider.CreateQuery(skipQuery);
        }

        public static IQueryable Take(IQueryable query, int count, Type type, bool parameterize)
        {
            Expression takeQuery = Take(query.Expression, count, type, parameterize);
            return query.Provider.CreateQuery(takeQuery);
        }

        public static Expression Skip(Expression source, int count, Type type, bool parameterize)
        {
            MethodInfo skipMethod;
            if (typeof(IQueryable).IsAssignableFrom(source.Type))
            {
                skipMethod = ExpressionHelperMethods.QueryableSkipGeneric.MakeGenericMethod(type);
            }
            else
            {
                skipMethod = ExpressionHelperMethods.EnumerableSkipGeneric.MakeGenericMethod(type);
            }

            Expression skipValueExpression = parameterize ? LinqParameterContainer.Parameterize(typeof(int), count) : Expression.Constant(count);
            Expression skipQuery = Expression.Call(null, skipMethod, new[] { source, skipValueExpression });
            return skipQuery;
        }

        public static Expression Take(Expression source, int count, Type elementType, bool parameterize)
        {
            MethodInfo takeMethod;
            if (typeof(IQueryable).IsAssignableFrom(source.Type))
            {
                takeMethod = ExpressionHelperMethods.QueryableTakeGeneric.MakeGenericMethod(elementType);
            }
            else
            {
                takeMethod = ExpressionHelperMethods.EnumerableTakeGeneric.MakeGenericMethod(elementType);
            }

            Expression takeValueExpression = parameterize ? LinqParameterContainer.Parameterize(typeof(int), count) : Expression.Constant(count);
            Expression takeQuery = Expression.Call(null, takeMethod, new[] { source, takeValueExpression });
            return takeQuery;
        }

        public static Expression OrderByPropertyExpression(
            Expression source, 
            string propertyName, 
            Type elementType, 
            bool alreadyOrdered = false)
        {
            LambdaExpression orderByLambda = GetPropertyAccessLambda(elementType, propertyName);
            return OrderBy(source, orderByLambda, elementType, OrderByDirection.Ascending, alreadyOrdered);
        }

        public static Expression OrderBy(
            Expression source,
            LambdaExpression orderByLambda,
            Type elementType,
            OrderByDirection direction,
            bool alreadyOrdered = false)
        {
            Type returnType = orderByLambda.Body.Type;
            MethodInfo orderByMethod;
            if (!alreadyOrdered)
            {
                if (typeof(IQueryable).IsAssignableFrom(source.Type))
                {
                    if (direction == OrderByDirection.Ascending)
                    {
                        orderByMethod = ExpressionHelperMethods.QueryableOrderByGeneric.MakeGenericMethod(elementType,
                            returnType);
                    }
                    else
                    {
                        orderByMethod = ExpressionHelperMethods.QueryableOrderByDescendingGeneric.MakeGenericMethod(elementType,
                            returnType);
                    }
                }
                else
                {
                    if (direction == OrderByDirection.Ascending)
                    {
                        orderByMethod = ExpressionHelperMethods.EnumerableOrderByGeneric.MakeGenericMethod(elementType,
                            returnType);
                    }
                    else
                    {
                        orderByMethod = ExpressionHelperMethods.EnumerableOrderByDescendingGeneric.MakeGenericMethod(elementType,
                            returnType);
                    }
                }
            }
            else
            {
                if (typeof(IQueryable).IsAssignableFrom(source.Type))
                {
                    if (direction == OrderByDirection.Ascending)
                    {
                        orderByMethod = ExpressionHelperMethods.QueryableThenByGeneric.MakeGenericMethod(elementType,
                            returnType);
                    }
                    else
                    {
                        orderByMethod = ExpressionHelperMethods.QueryableThenByDescendingGeneric.MakeGenericMethod(elementType,
                            returnType);
                    }
                }
                else
                {
                    if (direction == OrderByDirection.Ascending)
                    {
                        orderByMethod = ExpressionHelperMethods.EnumerableThenByGeneric.MakeGenericMethod(elementType,
                            returnType);
                    }
                    else
                    {
                        orderByMethod = ExpressionHelperMethods.EnumerableThenByDescendingGeneric.MakeGenericMethod(elementType,
                            returnType);
                    }
                }
            }
            return Expression.Call(null, orderByMethod, new[] { source, orderByLambda });
        }

        public static IQueryable OrderByIt(IQueryable query, OrderByDirection direction, Type type, bool alreadyOrdered = false)
        {
            ParameterExpression odataItParameter = Expression.Parameter(type, "$it");
            LambdaExpression orderByLambda = Expression.Lambda(odataItParameter, odataItParameter);
            return OrderBy(query, orderByLambda, direction, type, alreadyOrdered);
        }

        public static IQueryable OrderByProperty(IQueryable query, IEdmModel model, IEdmProperty property, OrderByDirection direction, Type type, bool alreadyOrdered = false)
        {
            // property aliasing
            string propertyName = EdmLibHelpers.GetClrPropertyName(property, model);
            LambdaExpression orderByLambda = GetPropertyAccessLambda(type, propertyName);
            return OrderBy(query, orderByLambda, direction, type, alreadyOrdered);
        }

        public static IQueryable OrderBy(IQueryable query, LambdaExpression orderByLambda, OrderByDirection direction, Type type, bool alreadyOrdered = false)
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

        public static IQueryable GroupBy(IQueryable query, Expression expression, Type type, Type wrapperType)
        {
            MethodInfo groupByMethod = ExpressionHelperMethods.QueryableGroupByGeneric.MakeGenericMethod(type, wrapperType);
            return groupByMethod.Invoke(null, new object[] { query, expression }) as IQueryable;
        }

        public static IQueryable Select(IQueryable query, LambdaExpression expression, Type type)
        {
            MethodInfo selectMethod = ExpressionHelperMethods.QueryableSelectGeneric.MakeGenericMethod(type, expression.Body.Type);
            return selectMethod.Invoke(null, new object[] { query, expression }) as IQueryable;
        }

        public static IQueryable Aggregate(IQueryable query, object init, LambdaExpression sumLambda, Type type, Type wrapperType)
        {
            Type returnType = sumLambda.Body.Type;
            MethodInfo sumMethod = ExpressionHelperMethods.QueryableAggregateGeneric.MakeGenericMethod(type, returnType);
            var agg = sumMethod.Invoke(null, new object[] { query, init, sumLambda });

            MethodInfo converterMethod = ExpressionHelperMethods.EntityAsQueryable.MakeGenericMethod(wrapperType);

            return converterMethod.Invoke(null, new object[] { agg }) as IQueryable;
        }

        public static IQueryable Where(IQueryable query, Expression where, Type type)
        {
            MethodInfo whereMethod = ExpressionHelperMethods.QueryableWhereGeneric.MakeGenericMethod(type);
            return whereMethod.Invoke(null, new object[] { query, where }) as IQueryable;
        }

        // If the expression is not a nullable type, cast it to one.
        public static Expression ToNullable(Expression expression)
        {
            if (!expression.Type.IsNullable())
            {
                return Expression.Convert(expression, expression.Type.ToNullable());
            }

            return expression;
        }

        // Entity Framework does not understand default(T) expression. Hence, generate a constant expression with the default value.
        public static Expression Default(Type type)
        {
            if (type.GetTypeInfo().IsValueType)
            {
                return Expression.Constant(Activator.CreateInstance(type), type);
            }
            else
            {
                return Expression.Constant(null, type);
            }
        }

        public static LambdaExpression GetPropertyAccessLambda(Type type, string propertyName)
        {
            ParameterExpression odataItParameter = Expression.Parameter(type, "$it");
            MemberExpression propertyAccess = Expression.Property(odataItParameter, propertyName);
            return Expression.Lambda(propertyAccess, odataItParameter);
        }
    }
}
