// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.Linq;
using System.Linq.Expressions;

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
    public class PageResult<T> : PageResult, IQueryable<T>
    {
        private IQueryable<T> _items;
        [NonSerialized]
        private Type _elementType;
        [NonSerialized]
        private Expression _expression;
        [NonSerialized]
        private IQueryProvider _provider;

        /// <summary>
        /// Gets the collection of entities for this feed.
        /// </summary>
        [DataMember]
        public IQueryable<T> Items
        {
            get
            {
                return _items;
            }
            private set
            {
                _items = value;
                _elementType = _items.ElementType;
                _expression = _items.Expression;
                _provider = _items.Provider;
            }
        }

        /// <summary>
        /// Gets the type of the element(s) that are returned when the expression tree associated
        ///     with this instance of System.Linq.IQueryable is executed.
        /// </summary>
        [JsonIgnore]
        [IgnoreDataMember]
        public Type ElementType
        {
            get
            {
                return _elementType;
            }
        }

        /// <summary>
        /// Gets the expression tree that is associated with the instance of System.Linq.IQueryable.
        /// </summary>
        [JsonIgnore]
        [IgnoreDataMember]
        public Expression Expression
        {
            get
            {
                return _expression;
            }
        }

        /// <summary>
        /// Gets the query provider that is associated with this data source.
        /// </summary>
        [JsonIgnore]
        [IgnoreDataMember]
        public IQueryProvider Provider
        {
            get
            {
                return _provider;
            }
        }

        /// <summary>
        /// Creates a partial set of results - used when server driven paging is enabled.
        /// </summary>
        /// <param name="items">The subset of matching results that should be serialized in this page.</param>
        /// <param name="nextPageLink">A link to the next page of matching results (if more exists).</param>
        /// <param name="count">A total count of matching results so clients can know the number of matches on the server.</param>
        public PageResult(IQueryable<T> items, Uri nextPageLink, long? count)
            : base(nextPageLink, count)
        {
            if (items == null)
            {
                throw Error.ArgumentNull("data");
            }

            Items = items;
        }

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

            Items = items.AsQueryable();
        }

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
