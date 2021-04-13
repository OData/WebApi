// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Query
{

    /// <summary>
    /// This class transforms an <see cref="IQueryable"/> based on
    /// the key and property accesses in the segments of an <see cref="ODataPath"/>
    /// </summary>
    internal class ODataPathQueryBuilder
    {
        readonly IQueryable source;
        readonly IEdmModel model;
        readonly Routing.ODataPath path;

        /// <summary>
        /// Creates an instance of <see cref="ODataPathQueryBuilder"/>
        /// </summary>
        /// <param name="source">The original <see cref="IQueryable"/> to be transformed</param>
        /// <param name="model">The <see cref="IEdmModel"/></param>
        /// <param name="path">The <see cref="ODataPath"/> based on which to transform the <see cref="source"/></param>
        public ODataPathQueryBuilder(IQueryable source, IEdmModel model, Routing.ODataPath path)
        {
            this.source = source;
            this.model = model;
            this.path = path;
        }

        /// <summary>
        /// Transforms the source <see cref="IQueryable"/> based on the sequence of path segments in the <see cref="ODataPath"/>
        /// </summary>
        /// <returns>The result of the query transformtions or null if the path contained unsupported segments</returns>
        public ODataPathQueryResult BuildQuery()
        {
            ODataPathQueryResult result = new ODataPathQueryResult();

            IEnumerable<ODataPathSegment> segments = path.Segments;

            IQueryable queryable = source;
            
            ODataPathSegment firstSegment = segments.FirstOrDefault();

            if (!(firstSegment is EntitySetSegment || firstSegment is SingletonSegment))
            {
                return null;
            }

            IEnumerable<ODataPathSegment> remainingSegments = segments.Skip(1);

            foreach (ODataPathSegment segment in remainingSegments)
            {
                Type currentType = queryable.ElementType;

                if (segment is KeySegment keySegment)
                {
                    Dictionary<string, object> keys = new Dictionary<string, object>();
                    foreach (var kvp in keySegment.Keys)
                    {
                        keys.Add(kvp.Key, kvp.Value);
                    }

                    // filterPredicate
                    ParameterExpression filterParam = Expression.Parameter(currentType, "entity");
                    IEnumerable<BinaryExpression> conditions = keySegment.Keys.Select(kvp =>
                        Expression.Equal(
                            Expression.Property(filterParam, kvp.Key),
                            Expression.Constant(kvp.Value)));
                    BinaryExpression filterBody = conditions.Aggregate((left, right) => Expression.AndAlso(left, right));
                    LambdaExpression filterPredicate = Expression.Lambda(filterBody, filterParam);

                    queryable = ExpressionHelpers.Where(queryable, filterPredicate, currentType);

                }
                else if (segment is NavigationPropertySegment navigationSegment)
                {
                    ParameterExpression param = Expression.Parameter(currentType);
                    MemberExpression navPropExpression = Expression.Property(param, navigationSegment.NavigationProperty.Name);

                    if (navigationSegment.NavigationProperty.TargetMultiplicity() == EdmMultiplicity.Many)
                    {
                        BinaryExpression condition = Expression.NotEqual(navPropExpression, Expression.Constant(null));
                        LambdaExpression nullFilter = Expression.Lambda(condition, param);
                        queryable = ExpressionHelpers.Where(queryable, nullFilter, currentType);
                        // collection navigation property
                        // e.g. Product/Categories
                        Type propertyType = currentType.GetProperty(navigationSegment.NavigationProperty.Name).PropertyType;
                        propertyType = GetEnumerableItemType(navPropExpression.Type);

                        currentType = propertyType;

                        Type delegateType = typeof(Func<,>).MakeGenericType(
                            queryable.ElementType,
                            typeof(IEnumerable<>).MakeGenericType(currentType));
                        LambdaExpression selectBody =
                            Expression.Lambda(delegateType, navPropExpression, param);
                        queryable = ExpressionHelpers.SelectMany(queryable, selectBody, currentType);
                    }
                    else
                    {
                        BinaryExpression condition = Expression.NotEqual(navPropExpression, Expression.Constant(null));
                        LambdaExpression nullFilter = Expression.Lambda(condition, param);
                        queryable = ExpressionHelpers.Where(queryable, nullFilter, currentType);

                        currentType = navPropExpression.Type;
                        LambdaExpression selectBody =
                            Expression.Lambda(navPropExpression, param);
                        queryable = ExpressionHelpers.Select(queryable, selectBody, currentType);
                    }
                }
                else if (segment is PropertySegment propertySegment)
                {
                    ParameterExpression param = Expression.Parameter(currentType);
                    MemberExpression propertyExpression = Expression.Property(param, propertySegment.Property.Name);

                    // check whether property is null or not before further selection
                    if (propertySegment.Property.Type.IsNullable && !propertySegment.Property.Type.IsPrimitive())
                    {
                        // queryable = queryable.Where( => .Property != null)
                        BinaryExpression condition = Expression.NotEqual(propertyExpression, Expression.Constant(null));
                        LambdaExpression nullFilter = Expression.Lambda(condition, param);
                        queryable = ExpressionHelpers.Where(queryable, nullFilter, queryable.ElementType);
                    }

                    if (propertySegment.Property.Type.IsCollection())
                    {
                        // Produces new query like 'queryable.SelectMany(param => param.PropertyName)'.
                        // Suppose 'param.PropertyName' is of type 'IEnumerable<T>', the type of the
                        // resulting query would be 'IEnumerable<T>' too.
                        currentType = GetEnumerableItemType(propertyExpression.Type);
                        Type delegateType = typeof(Func<,>).MakeGenericType(
                            queryable.ElementType,
                            typeof(IEnumerable<>).MakeGenericType(currentType));
                        LambdaExpression selectBody =
                        Expression.Lambda(delegateType, propertyExpression, param);
                        queryable = ExpressionHelpers.SelectMany(queryable, selectBody, currentType);
                    }
                    else
                    {
                        // Produces new query like 'queryable.Select(param => param.PropertyName)'.
                        currentType = propertyExpression.Type;
                        LambdaExpression selectBody =
                            Expression.Lambda(propertyExpression, param);
                        queryable = ExpressionHelpers.Select(queryable, selectBody, currentType);
                    }
                }
                else if (segment is CountSegment)
                {
                    result.HasCountSegment = true;
                }
                else if (segment is ValueSegment)
                {
                    result.HasValueSegment = true;
                }
                else
                {
                    // reached unsupported segment
                    return null;
                }
            }

            result.Result = queryable;

            return result;
        }

        /// <summary>
        /// Gets the element type "T" of the given type
        /// if it implements <see cref="IEnumerable{T}"/>
        /// </summary>
        /// <param name="enumerableType"></param>
        /// <returns>
        /// The element type if <paramref name="enumerableType"/> implements <see cref="IEnumerable{T}"/>
        /// otherwise returns <paramref name="enumerableType"/> itself
        /// </returns>
        private static Type GetEnumerableItemType(Type enumerableType)
        {
            Type type = FindGenericType(enumerableType, typeof(IEnumerable<>));
            if (type != null)
            {
                return type.GetGenericArguments()[0];
            }

            return enumerableType;
        }

        /// <summary>
        /// Find a base type or implemented interface which has a generic definition
        /// represented by the parameter, <c>definition</c>.
        /// </summary>
        /// <param name="type">
        /// The subject type.
        /// </param>
        /// <param name="definition">
        /// The generic definition to check with.
        /// </param>
        /// <returns>
        /// The base type or the interface found; otherwise, <c>null</c>.
        /// </returns>
        private static Type FindGenericType(Type type, Type definition)
        {
            if (type == null)
            {
                return null;
            }

            // If the type conforms the given generic definition, no further check required.
            if (IsGenericDefinition(type, definition))
            {
                return type;
            }

            // If the definition is interface, we only need to check the interfaces implemented by the current type
            if (definition.IsInterface)
            {
                foreach (Type interfaceType in type.GetInterfaces())
                {
                    if (IsGenericDefinition(interfaceType, definition))
                    {
                        return interfaceType;
                    }
                }
            }
            else if (!type.IsInterface)
            {
                // If the definition is not an interface, then the current type cannot be an interface too.
                // Otherwise, we should only check the parent class types of the current type.

                // no null check for the type required, as we are sure it is not an interface type
                while (type != typeof(object))
                {
                    if (IsGenericDefinition(type, definition))
                    {
                        return type;
                    }

                    type = type.BaseType;
                }
            }

            return null;
        }

        /// <summary>
        /// Checks whether <paramref name="type"/> conforms to a generic
        /// definition of the given generic type <paramref name="definition"/>
        /// </summary>
        /// <param name="type">The type to test</param>
        /// <param name="definition">The generic type definition to test against</param>
        /// <returns>
        /// True if <paramref name="type"/> conforms to a generic
        /// definition of the given generic type <paramref name="definition"/> otherwise false
        /// </returns>
        private static bool IsGenericDefinition(Type type, Type definition)
        {
            return type.IsGenericType &&
                   type.GetGenericTypeDefinition() == definition;
        }
    }

    /// <summary>
    /// Result returned by <see cref="ODataPathQueryBuilder"/> after
    /// applying transformations based on path.
    /// </summary>
    internal class ODataPathQueryResult
    {
        /// <summary>
        /// The result of the query transformations applied to the original <see cref="IQueryable"/>
        /// by the <see cref="ODataPathQueryBuilder"/>
        /// </summary>
        public IQueryable Result { get; set; }
        /// <summary>
        /// Whether the path has a $count segment
        /// </summary>
        public bool HasCountSegment { get; set; }
        /// <summary>
        /// Whether the path has a $value segment
        /// </summary>
        public bool HasValueSegment { get; set; }
    }
}
