// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http.Formatting;

namespace System.Web.Http
{
    /// <summary>
    /// Provides extension methods for the <see cref="MediaTypeFormatterCollection"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ODataMediaTypeFormatterCollectionExtensions
    {
        /// <summary>
        /// Adds the elements of the specified collection to the end of the <see cref="MediaTypeFormatterCollection"/>.
        /// </summary>
        /// <param name="collection">The collection to which to add the items.</param>
        /// <param name="items">
        /// The items that should be added to the end of the <see cref="MediaTypeFormatterCollection"/>.
        /// The items collection itself cannot be <see langword="null"/>, but it can contain elements that are
        /// <see langword="null"/>.
        /// </param>
        [Obsolete("This method is obsolete; use the AddRange method in the MediaTypeFormatterCollection class.")]
        public static void AddRange(this MediaTypeFormatterCollection collection,
            IEnumerable<MediaTypeFormatter> items)
        {
            if (collection == null)
            {
                throw Error.ArgumentNull("collection");
            }

            collection.AddRange(items);
        }

        /// <summary>
        /// Inserts the elements of a collection into the <see cref="MediaTypeFormatterCollection"/> at the specified
        /// index.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="index">The zero-based index at which the new elements should be inserted.</param>
        /// <param name="items">
        /// The items that should be inserted into the <see cref="MediaTypeFormatterCollection"/>. The items collection
        /// itself cannot be <see langword="null"/>, but it can contain elements that are <see langword="null"/>.
        /// </param>
        [Obsolete("This method is obsolete; use the InsertRange method in the MediaTypeFormatterCollection class.")]
        public static void InsertRange(this MediaTypeFormatterCollection collection,
            int index, IEnumerable<MediaTypeFormatter> items)
        {
            if (collection == null)
            {
                throw Error.ArgumentNull("collection");
            }

            collection.InsertRange(index, items);
        }
    }
}
