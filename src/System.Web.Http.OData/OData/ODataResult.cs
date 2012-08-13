// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Web.Http.OData.Properties;

namespace System.Web.Http.OData
{ 
    /// <summary>
    /// ODataResult is a feed of Entities that include additional information that OData Formats support
    /// Currently limited to: 
    ///     the Count of all matching entities on the server (requested using $inlinecount=allpages) 
    ///     the NextLink to retrieve the next page of results (added if the server enforces Server Driven Paging)
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Collection suffix not appropriate")]
    public class ODataResult<T> : ODataResult, IEnumerable<T>
    {
        private IEnumerable<T> _data;
        
        /// <summary>
        /// Creates a partial set of results - used when server driven paging is enabled.
        /// </summary>
        /// <param name="data">The subset of matching results that should be serialized in this page.</param>
        /// <param name="nextPageLink">A link to the next page of matching results (if more exists).</param>
        /// <param name="count">A total count of matching results so clients can know the number of matches on the server.</param>
        public ODataResult(IEnumerable<T> data, Uri nextPageLink, long? count)
            : base(nextPageLink, count)
        {
            if (data == null)
            {
                throw Error.ArgumentNull("data");
            }

            _data = data;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _data.GetEnumerator();
        }
    }

    public abstract class ODataResult
    {
        private long? _count;

        protected ODataResult(Uri nextPageLink, long? count)
        {
            NextPageLink = nextPageLink;
            Count = count;
        }

        public Uri NextPageLink
        {
            get;
            private set;
        }

        public long? Count
        {
            get 
            {
                return _count;
            }
            private set
            {
                if (value.HasValue && value.Value <= 0)
                {
                    throw Error.ArgumentMustBeGreaterThanOrEqualTo("value", value.Value, 1);
                }
                _count = value;
            }
        }
    }
}
