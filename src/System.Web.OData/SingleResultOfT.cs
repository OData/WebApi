// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;

namespace System.Web.Http
{
    /// <summary>
    /// Represents an <see cref="IQueryable"/> containing zero or one entities.
    /// </summary>
    /// <typeparam name="T">The type of the data in the data source.</typeparam>
    public sealed class SingleResult<T> : SingleResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SingleResult"/> class.
        /// </summary>
        /// <param name="queryable">The <see cref="IQueryable{T}"/> containing zero or one entities.</param>
        public SingleResult(IQueryable<T> queryable)
            : base(queryable)
        {
        }

        /// <summary>
        /// The <see cref="IQueryable{T}"/> containing zero or one entities.
        /// </summary>
        public new IQueryable<T> Queryable
        {
            get
            {
                return base.Queryable as IQueryable<T>;
            }
        }
    }
}
