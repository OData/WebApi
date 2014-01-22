// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http.Formatting;

namespace System.Web.Http
{
    /// <summary>
    /// Provides extension methods for the <see cref="MediaTypeFormatterCollection"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class ODataMediaTypeFormatterCollectionExtensions
    {
        public static void RemoveRange(this MediaTypeFormatterCollection collection,
            IEnumerable<MediaTypeFormatter> items)
        {
            Contract.Assert(collection != null);
            Contract.Assert(items != null);

            // Instantiate a separate array in case items and collection are linked. Otherwise, if modifying collection
            // itself modified items, this code would throw during enumeration.
            foreach (MediaTypeFormatter item in items.ToArray())
            {
                collection.Remove(item);
            }
        }
    }
}
