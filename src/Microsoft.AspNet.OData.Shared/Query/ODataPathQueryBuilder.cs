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
    internal class ODataPathQueryBuilder
    {
        readonly IQueryable source;
        readonly IEdmModel model;
        readonly Routing.ODataPath path;

        public ODataPathQueryBuilder(IQueryable source, IEdmModel model, Routing.ODataPath path)
        {
            this.source = source;
            this.model = model;
            this.path = path;
        }

        public ODataPathQueryResult BuildQuery()
        {
            var result = new ODataPathQueryResult();

            var segments = path.Segments;
            // assume first segment is entitySet
            var firstSegment = segments.FirstOrDefault() as EntitySetSegment;

            //var edmType = firstSegment.EdmType;

            var queryable = source;
            var currentType = queryable.ElementType;

            var remainingSegments = segments.Skip(1);

            foreach (var segment in remainingSegments)
            {
                if (segment is KeySegment keySegment)
                {
                    var keys = new Dictionary<string, object>();
                    foreach (var kvp in keySegment.Keys)
                    {
                        keys.Add(kvp.Key, kvp.Value);
                    }

                    // filerPredicate
                    var filterParam = Expression.Parameter(currentType, "entity");
                    var conditions = keySegment.Keys.Select(kvp =>
                        Expression.Equal(
                            Expression.Property(filterParam, kvp.Key),
                            Expression.Constant(kvp.Value)));
                    var filterBody = conditions.Aggregate((left, right) => Expression.AndAlso(left, right));
                    var filterPredicate = Expression.Lambda(filterBody, filterParam);

                    queryable = Where(queryable, filterPredicate, currentType);

                }
                else if (segment is NavigationPropertySegment navigationSegment)
                {
                    var param = Expression.Parameter(currentType);
                    var navPropExpression = Expression.Property(param, navigationSegment.NavigationProperty.Name);

                    if (navigationSegment.NavigationProperty.TargetMultiplicity() == EdmMultiplicity.Many)
                    {
                        var condition = Expression.NotEqual(navPropExpression, Expression.Constant(null));
                        var nullFilter = Expression.Lambda(condition, param);
                        queryable = Where(queryable, nullFilter, currentType);
                        // collection navigation property
                        // e.g. Product/Categories
                        var propertyType = currentType.GetProperty(navigationSegment.NavigationProperty.Name).PropertyType;
                        propertyType = GetEnumerableItemType(navPropExpression.Type);

                        currentType = propertyType;

                        var delegateType = typeof(Func<,>).MakeGenericType(
                            queryable.ElementType,
                            typeof(IEnumerable<>).MakeGenericType(currentType));
                        var selectBody =
                            Expression.Lambda(delegateType, navPropExpression, param);

                        queryable = SelectMany(queryable, selectBody, currentType);
                    }
                    else
                    {
                        var condition = Expression.NotEqual(navPropExpression, Expression.Constant(null));
                        var nullFilter = Expression.Lambda(condition, param);
                        queryable = Where(queryable, nullFilter, currentType);

                        currentType = navPropExpression.Type;
                        var selectBody =
                            Expression.Lambda(navPropExpression, param);
                        queryable = Select(queryable, selectBody);
                    }
                }
                else if (segment is PropertySegment propertySegment)
                {
                    var param = Expression.Parameter(currentType);
                    var propertyExpression = Expression.Property(param, propertySegment.Property.Name);

                    // check whether property is null or not before further selection
                    if (propertySegment.Property.Type.IsNullable && !propertySegment.Property.Type.IsPrimitive())
                    {
                        var condition = Expression.NotEqual(propertyExpression, Expression.Constant(null));
                        var nullFilter = Expression.Lambda(condition, param);
                        queryable = Where(queryable, nullFilter, currentType);
                    }

                    if (propertySegment.Property.Type.IsCollection())
                    {
                        // Produces new query like 'queryable.SelectMany(param => param.PropertyName)'.
                        // Suppose 'param.PropertyName' is of type 'IEnumerable<T>', the type of the
                        // resulting query would be 'IEnumerable<T>' too.
                        currentType = GetEnumerableItemType(propertyExpression.Type);
                        var delegateType = typeof(Func<,>).MakeGenericType(
                            queryable.ElementType,
                            typeof(IEnumerable<>).MakeGenericType(currentType));
                        var selectBody =
                        Expression.Lambda(delegateType, propertyExpression, param);
                        queryable = SelectMany(queryable, selectBody, currentType);
                    }
                    else
                    {
                        // Produces new query like 'queryable.Select(param => param.PropertyName)'.
                        currentType = propertyExpression.Type;
                        var selectBody =
                            Expression.Lambda(propertyExpression, param);
                        queryable = Select(queryable, selectBody);
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

        private static MethodInfo QueryableWhereGeneric { get; } = GenericMethodOf(_ => Queryable.Where(default, default(Expression<Func<int, bool>>)));

        private static MethodInfo QueryableSelectGeneric { get; } = GenericMethodOf(_ => Queryable.Select(default(IQueryable<int>), i => i));

        private static MethodInfo QueryableSelectManyGeneric { get; } = GenericMethodOf(_ => Queryable.SelectMany(default(IQueryable<int>), i => default(IQueryable<int>)));

        public static MethodInfo QueryableOfTypeGeneric { get; } = GenericMethodOf(_ => Queryable.OfType<int>(default(IQueryable)));

        private static IQueryable Where(IQueryable query, LambdaExpression where, Type type)
        {
            var whereMethod = QueryableWhereGeneric.MakeGenericMethod(type);
            return whereMethod.Invoke(null, new object[] { query, where }) as IQueryable;
        }

        public static IQueryable OfType(IQueryable query, Type type)
        {
            var ofTypeMethod = QueryableOfTypeGeneric.MakeGenericMethod(type);
            return ofTypeMethod.Invoke(null, new object[] { query }) as IQueryable;
        }

        private static IQueryable SelectMany(IQueryable query, LambdaExpression selectMany, Type selectedPropertyType)
        {
            var selectManyMethod = QueryableSelectManyGeneric.MakeGenericMethod(query.ElementType, selectedPropertyType);
            return selectManyMethod.Invoke(null, new object[] { query, selectMany }) as IQueryable;
        }

        private static IQueryable Select(IQueryable query, LambdaExpression select)
        {
            var selectMethod =
                QueryableSelectGeneric.MakeGenericMethod(
                    query.ElementType,
                    select.Body.Type);
            return selectMethod.Invoke(null, new object[] { query, select }) as IQueryable;
        }

        private static MethodInfo GenericMethodOf<TReturn>(Expression<Func<object, TReturn>> expression) => GenericMethodOf(expression as Expression);

        private static MethodInfo GenericMethodOf(Expression expression)
        {
            var lambdaExpression = expression as LambdaExpression;

            Contract.Assert(expression.NodeType == ExpressionType.Lambda);
            Contract.Assert(lambdaExpression != null);
            Contract.Assert(lambdaExpression.Body.NodeType == ExpressionType.Call);

            return (lambdaExpression.Body as MethodCallExpression).Method.GetGenericMethodDefinition();
        }

        private static Type GetEnumerableItemType(Type enumerableType)
        {
            var type = FindGenericType(enumerableType, typeof(IEnumerable<>));
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
                foreach (var interfaceType in type.GetInterfaces())
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

        private static bool IsGenericDefinition(Type type, Type definition)
        {
            return type.IsGenericType &&
                   type.GetGenericTypeDefinition() == definition;
        }

        /// <summary>
        /// Get the clr type for a specified edm type
        /// </summary>
        /// <param name="edmType">The edm type to get clr type</param>
        /// <param name="edmModel">The edm model </param>
        /// <returns>The clr type</returns>
        private static Type GetClrType(IEdmType edmType, IEdmModel edmModel)
        {

            var annotation = edmModel.GetAnnotationValue<ClrTypeAnnotation>(edmType);
            if (annotation != null)
            {
                return annotation.ClrType;
            }

            throw new NotSupportedException(string.Format(
                "Cast not supported for type {0}",
                edmType.FullTypeName()));
        }
    }

    internal class ODataPathQueryResult
    {
        public IQueryable Result { get; set; }
        public bool HasCountSegment { get; set; }
        public bool HasValueSegment { get; set; }
    }
}
