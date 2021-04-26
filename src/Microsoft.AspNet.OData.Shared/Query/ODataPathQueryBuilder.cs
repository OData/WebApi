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
        private readonly IQueryable source;
        private readonly Routing.ODataPath path;

        /// <summary>
        /// Creates an instance of <see cref="ODataPathQueryBuilder"/>
        /// </summary>
        /// <param name="source">The original <see cref="IQueryable"/> to be transformed</param>
        /// <param name="model">The <see cref="IEdmModel"/></param>
        /// <param name="path">The <see cref="ODataPath"/> based on which to transform the <see cref="source"/></param>
        public ODataPathQueryBuilder(IQueryable source, IEdmModel model, Routing.ODataPath path)
        {
            this.source = source;
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
                if (segment is KeySegment keySegment)
                {
                    // filterPredicate = entity => (entity.KeyProp1 == Val1) && (entity.keyProp2 == Val2) && ...
                    ParameterExpression filterParam = Expression.Parameter(queryable.ElementType, "entity");
                    IEnumerable<BinaryExpression> conditions = keySegment.Keys.Select(kvp =>
                        Expression.Equal(
                            Expression.Property(filterParam, kvp.Key),
                            Expression.Constant(kvp.Value)));
                    BinaryExpression filterBody = conditions.Aggregate((left, right) => Expression.AndAlso(left, right));
                    LambdaExpression filterPredicate = Expression.Lambda(filterBody, filterParam);

                    // queryable = queryable.Where(entity => (entity.KeyProp1 == Val1) && (entity.KeyProp2 == Val2) ...)
                    queryable = ExpressionHelpers.Where(queryable, filterPredicate, queryable.ElementType);

                }
                else if (segment is NavigationPropertySegment navigationSegment)
                {
                    string propertyName = navigationSegment.NavigationProperty.Name;

                    if (navigationSegment.NavigationProperty.TargetMultiplicity() == EdmMultiplicity.Many)
                    {
                        queryable = GetCollectionPropertyQuery(queryable, propertyName);
                    }
                    else
                    {
                        queryable = GetSinglePropertyQuery(queryable, propertyName, checkIfNull: true);
                    }
                }
                else if (segment is PropertySegment propertySegment)
                {
                    string propertyName = propertySegment.Property.Name;

                    if (propertySegment.Property.Type.IsCollection())
                    {
                        queryable = GetCollectionPropertyQuery(queryable, propertyName);
                    }
                    else
                    {
                        // we don't check whether param.Property is null because
                        // comparisons on complex properties cause errors in EF
                        queryable = GetSinglePropertyQuery(queryable, propertyName, checkIfNull: false);
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
        /// Transforms <paramref name="queryable"/> by selecting
        /// the single-valued property of called <paramref name="propertyName"/>.
        /// </summary>
        /// <param name="queryable">The <see cref="IQueryable"/> to apply the property selection to</param>
        /// <param name="propertyName">The name of the single-valued property to select</param>
        /// <param name="checkIfNull">If true, checks whether the property is null before selecting it</param>
        /// <returns>The transformed <see cref="IQueryable"/></returns>
        private static IQueryable GetSinglePropertyQuery(IQueryable queryable, string propertyName, bool checkIfNull = false)
        {
            ParameterExpression param = Expression.Parameter(queryable.ElementType);
            MemberExpression propertyExpression = Expression.Property(param, propertyName);
            IQueryable result = queryable;

            if (checkIfNull)
            {
                // queryable = queryable.Where(entity => entity.Property != null)
                BinaryExpression condition = Expression.NotEqual(propertyExpression, Expression.Constant(null));
                LambdaExpression nullFilter = Expression.Lambda(condition, param);
                result = ExpressionHelpers.Where(queryable, nullFilter, queryable.ElementType);
            }

            // return queryable.Select(entity => entity.NavProp)
            LambdaExpression selectBody =
                Expression.Lambda(propertyExpression, param);
            return ExpressionHelpers.Select(result, selectBody, queryable.ElementType);
        }

        /// <summary>
        /// Transforms <paramref name="queryable"/> by selecting
        /// the collection-valued property called <paramref name="propertyName"/>.
        /// </summary>
        /// <param name="queryable">The <see cref="IQueryable"/> to apply the property selection to</param>
        /// <param name="propertyName">The name of the collection-valued property to select</param>
        /// <returns>The transformed <see cref="IQueryable"/></returns>
        private static IQueryable GetCollectionPropertyQuery(IQueryable queryable, string propertyName)
        {
            ParameterExpression param = Expression.Parameter(queryable.ElementType);
            MemberExpression propertyExpression = Expression.Property(param, propertyName);

            // for collection properties we use SelectMany instead of Select
            // because Select would return an IQueryable<IEnumerable<TResult>>,
            // but SelectMany flattens it to IQueryable<TResult>

            // we don't check whether the property is null before advancing
            // because that seems to cause exceptions in EF

            // We expect the collection navigation property to implements IEnumerable<T>
            // Here we extract the element type T
            Type collectionPropertyElementType = TypeHelper.GetImplementedIEnumerableType(propertyExpression.Type) ?? propertyExpression.Type;

            // The lambda passed to queryable.SelectMany() has a signature Func<TSource, IEnumerable<TResult>>
            // TSource corresponds to our queryable.ElementType and TResult is the element type of the nav property
            Type delegateType = typeof(Func<,>).MakeGenericType(
                queryable.ElementType,
                typeof(IEnumerable<>).MakeGenericType(collectionPropertyElementType));
            LambdaExpression selectBody =
                Expression.Lambda(delegateType, propertyExpression, param);

            // return queryable.SelectMany(entity => entity.CollectionNavProp)
            return ExpressionHelpers
                .SelectMany(queryable, selectBody, queryable.ElementType, collectionPropertyElementType);
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
