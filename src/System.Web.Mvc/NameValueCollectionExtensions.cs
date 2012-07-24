// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Specialized;

namespace System.Web.Mvc
{
    public static class NameValueCollectionExtensions
    {
        public static void CopyTo(this NameValueCollection collection, IDictionary<string, object> destination)
        {
            CopyTo(collection, destination, false /* replaceEntries */);
        }

        public static void CopyTo(this NameValueCollection collection, IDictionary<string, object> destination, bool replaceEntries)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            if (destination == null)
            {
                throw new ArgumentNullException("destination");
            }

            foreach (string key in collection.Keys)
            {
                if (replaceEntries || !destination.ContainsKey(key))
                {
                    destination[key] = collection[key];
                }
            }
        }
    }
}
