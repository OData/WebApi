//-----------------------------------------------------------------------------
// <copyright file="TruncatedCollectionOfT.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData.Common;

namespace Microsoft.AspNet.OData.Query
{
    /// <summary>
    /// Represents a class that truncates a collection to a given page size.
    /// </summary>
    /// <typeparam name="T">The collection element type.</typeparam>
    public class TruncatedCollection<T> : List<T>, ITruncatedCollection, IEnumerable<T>, ICountOptionCollection
    {
        private const int MinPageSize = 1;

        private bool _isTruncated;
        private int _pageSize;
        private long? _totalCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="TruncatedCollection{T}"/> class.
        /// </summary>
        /// <param name="source">The collection to be truncated.</param>
        /// <param name="pageSize">The page size.</param>
        public TruncatedCollection(IEnumerable<T> source, int pageSize)
            : base(source.Take(checked(pageSize + 1)))
        {
            Initialize(pageSize);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TruncatedCollection{T}"/> class.
        /// </summary>
        /// <param name="source">The queryable collection to be truncated.</param>
        /// <param name="pageSize">The page size.</param>
        // NOTE: The queryable version calls Queryable.Take which actually gets translated to the backend query where as 
        // the enumerable version just enumerates and is inefficient.
        public TruncatedCollection(IQueryable<T> source, int pageSize) : this(source, pageSize, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TruncatedCollection{T}"/> class.
        /// </summary>
        /// <param name="source">The queryable collection to be truncated.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="parameterize">Flag indicating whether constants should be parameterized</param>
        // NOTE: The queryable version calls Queryable.Take which actually gets translated to the backend query where as 
        // the enumerable version just enumerates and is inefficient.
        public TruncatedCollection(IQueryable<T> source, int pageSize, bool parameterize)
            : base(Take(source, pageSize, parameterize))
        {
            Initialize(pageSize);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TruncatedCollection{T}"/> class.
        /// </summary>
        /// <param name="source">The queryable collection to be truncated.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="totalCount">The total count.</param>
        public TruncatedCollection(IEnumerable<T> source, int pageSize, long? totalCount)
            : base(pageSize > 0 ? source.Take(checked(pageSize + 1)) : source)
        {
            if (pageSize > 0)
            {
                Initialize(pageSize);
            }

            _totalCount = totalCount;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TruncatedCollection{T}"/> class.
        /// </summary>
        /// <param name="source">The queryable collection to be truncated.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="totalCount">The total count.</param>
        // NOTE: The queryable version calls Queryable.Take which actually gets translated to the backend query where as 
        // the enumerable version just enumerates and is inefficient.
        public TruncatedCollection(IQueryable<T> source, int pageSize, long? totalCount) : this(source, pageSize, totalCount, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TruncatedCollection{T}"/> class.
        /// </summary>
        /// <param name="source">The queryable collection to be truncated.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="totalCount">The total count.</param>
        /// <param name="parameterize">Flag indicating whether constants should be parameterized</param>
        // NOTE: The queryable version calls Queryable.Take which actually gets translated to the backend query where as 
        // the enumerable version just enumerates and is inefficient.
        public TruncatedCollection(IQueryable<T> source, int pageSize, long? totalCount, bool parameterize)
            : base(pageSize > 0 ? Take(source, pageSize, parameterize) : source)
        {
            if (pageSize > 0)
            {
                Initialize(pageSize);
            }

            _totalCount = totalCount;
        }

        private void Initialize(int pageSize)
        {
            if (pageSize < MinPageSize)
            {
                throw Error.ArgumentMustBeGreaterThanOrEqualTo("pageSize", pageSize, MinPageSize);
            }

            _pageSize = pageSize;

            if (Count > pageSize)
            {
                _isTruncated = true;
                RemoveAt(Count - 1);
            }
        }

        private static IQueryable<T> Take(IQueryable<T> source, int pageSize, bool parameterize)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }

            return ExpressionHelpers.Take(source, checked(pageSize + 1), typeof(T), parameterize) as IQueryable<T>;
        }

        /// <inheritdoc />
        public int PageSize
        {
            get { return _pageSize; }
        }

        /// <inheritdoc />
        public bool IsTruncated
        {
            get { return _isTruncated; }
        }

        /// <inheritdoc />
        public long? TotalCount
        {
            get { return _totalCount; }
        }
    }
}
