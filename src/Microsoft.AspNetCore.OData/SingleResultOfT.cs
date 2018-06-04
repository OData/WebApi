﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Represents an <see cref="IQueryable{T}"/> containing zero or one entities. Use together with an
    /// <c>[EnableQuery]</c>.
    /// </summary>
    /// <typeparam name="T">The type of the data in the data source.</typeparam>
    public sealed class SingleResult<T> : SingleResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SingleResult{T}"/> class.
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