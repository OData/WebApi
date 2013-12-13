// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace System.Web.Http.OData
{
    /// <summary>Represents a feed of entities that includes additional information that OData formats support.</summary>
    /// <remarks>
    /// Currently limited to:
    /// <list type="bullet">
    /// <item><description>The Count of all matching entities on the server (requested using $inlinecount=allpages).</description></item>
    /// <item><description>The NextLink to retrieve the next page of results (added if the server enforces Server Driven Paging).</description></item>
    /// </list>
    /// </remarks>
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

        /// <summary>
        /// Gets the collection of entities for this feed.
        /// </summary>
        [DataMember]
        public IEnumerable<T> Items { get; private set; }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="IEnumerator" /> object that can be used to iterate through the collection.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }
    }
}