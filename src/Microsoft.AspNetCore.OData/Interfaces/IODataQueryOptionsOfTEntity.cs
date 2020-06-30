using Microsoft.AspNet.OData.Formatter;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AspNet.OData.Interfaces
{
    /// <summary>
    /// This defines a composite OData query options that can be used to perform query composition.
    /// Currently this only supports $filter, $orderby, $top, $skip.
    /// </summary>
    [ODataQueryParameterBinding]
    public interface IODataQueryOptions<TEntity>
        : IODataQueryOptions
    {
        /// <summary>
        /// Gets the <see cref="ETag{TEntity}"/> from IfMatch header.
        /// </summary>
        new ETag<TEntity> IfMatch { get; }

        /// <summary>
        /// Gets the <see cref="ETag{TEntity}"/> from IfNoneMatch header.
        /// </summary>
        new ETag<TEntity> IfNoneMatch { get; }
    }
}
