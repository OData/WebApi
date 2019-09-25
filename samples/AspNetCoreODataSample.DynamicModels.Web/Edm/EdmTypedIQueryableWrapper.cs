using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.OData.Edm;

namespace AspNetCoreODataSample.DynamicModels.Web.Edm
{
    /// <summary>
    /// A set of extension methods to easily wrap IQueryable objects with their expected EDM types. 
    /// </summary>
    public static class EdmTypedIQueryableWrapperExtensions
    {
        public static IQueryable<T> WithEdmCollectionType<T>(this IQueryable<T> queryable, IEdmCollectionType collectionType)
        {
            return new EdmTypedIQueryableWrapper<T>(queryable, collectionType);
        }
        public static IQueryable<T> WithEdmElementType<T>(this IQueryable<T> queryable, IEdmEntityType entityType)
        {
            return new EdmTypedIQueryableWrapper<T>(queryable, new EdmCollectionType(new EdmEntityTypeReference(entityType, true)));
        }
        public static IQueryable<T> WithEdmElementType<T>(this IQueryable<T> queryable, IEdmComplexType complexType)
        {
            return new EdmTypedIQueryableWrapper<T>(queryable, new EdmCollectionType(new EdmComplexTypeReference(complexType, true)));
        }
    }

    /// <summary>
    /// This base interface allows non-generic access to the <see cref="IEdmCollectionType"/>. 
    /// </summary>
    public interface IEdmTypedIQueryableWrapper
    {
        IEdmCollectionType EdmCollectionType { get; }
    }

    /// <summary>
    /// This IQueryable wrapper allows specifying for an IQueryable which <see cref="IEdmCollectionType"/> is contained. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EdmTypedIQueryableWrapper<T> : IOrderedQueryable<T>, IEdmTypedIQueryableWrapper
    {
        public IEdmCollectionType EdmCollectionType { get; }
        private readonly IQueryable _queryable;

        public EdmTypedIQueryableWrapper(IQueryable queryable, IEdmCollectionType edmCollectionType)
        {
            EdmCollectionType = edmCollectionType;
            _queryable = queryable;
            Provider = new EdmTypedIQueryableWrapperQueryProvider(this);
        }


        private class EdmTypedIQueryableWrapperQueryProvider : IQueryProvider
        {
            private readonly EdmTypedIQueryableWrapper<T> _edmTypedIQueryableWrapper;

            public EdmTypedIQueryableWrapperQueryProvider(EdmTypedIQueryableWrapper<T> edmTypedIQueryableWrapper)
            {
                _edmTypedIQueryableWrapper = edmTypedIQueryableWrapper;
            }

            public IQueryable CreateQuery(Expression expression)
            {
                return new EdmTypedIQueryableWrapper<T>(_edmTypedIQueryableWrapper._queryable.Provider.CreateQuery(expression), _edmTypedIQueryableWrapper.EdmCollectionType);
            }

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            {
                return new EdmTypedIQueryableWrapper<TElement>(_edmTypedIQueryableWrapper._queryable.Provider.CreateQuery<TElement>(expression), _edmTypedIQueryableWrapper.EdmCollectionType);
            }

            public object Execute(Expression expression)
            {
                return _edmTypedIQueryableWrapper._queryable.Provider.Execute(expression);
            }

            public TResult Execute<TResult>(Expression expression)
            {
                return _edmTypedIQueryableWrapper._queryable.Provider.Execute<TResult>(expression);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Expression Expression => _queryable.Expression;

        public Type ElementType => _queryable.ElementType;

        public IQueryProvider Provider { get; }
    }
}
