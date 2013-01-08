// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace System.Web.Http.OData
{
    /// <summary>
    /// PageResult is a feed of Entities that include additional information that OData Formats support
    /// Currently limited to: 
    ///     the Count of all matching entities on the server (requested using $inlinecount=allpages) 
    ///     the NextLink to retrieve the next page of results (added if the server enforces Server Driven Paging)
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Collection suffix not appropriate")]
    [DataContract]
    [JsonObject]
    public class PageResult<T> : PageResult, IEnumerable<T>
    {
        /// <summary>
        /// Creates a partial set of results - used when server driven paging is enabled.
        /// </summary>
        /// <param name="items">The subset of matching results that should be serialized in this page.</param>
        /// <param name="nextPageLink">A link to the next page of matching results (if more exists).</param>
        /// <param name="count">A total count of matching results so clients can know the number of matches on the server.</param>
        public PageResult(IEnumerable<T> items, Uri nextPageLink, long? count)
            : base(nextPageLink, count)
        {
            if (items == null)
            {
                throw Error.ArgumentNull("data");
            }

            Items = items;
        }

        [DataMember]
        public IEnumerable<T> Items { get; private set; }

        public IEnumerator<T> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }
    }
}