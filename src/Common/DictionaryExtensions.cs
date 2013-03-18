// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace System.Web.Http
{
    /// <summary>
    /// Extension methods for <see cref="IDictionary{TKey,TValue}"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class DictionaryExtensions
    {
        /// <summary>
        /// Gets the value of <typeparamref name="T"/> associated with the specified key or <c>default</c> value if
        /// either the key is not present or the value is not of type <typeparamref name="T"/>. 
        /// </summary>
        /// <typeparam name="T">The type of the value associated with the specified key.</typeparam>
        /// <param name="collection">The <see cref="IDictionary{TKey,TValue}"/> instance where <c>TValue</c> is <c>object</c>.</param>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter.</param>
        /// <returns><c>true</c> if key was found, value is non-null, and value is of type <typeparamref name="T"/>; otherwise false.</returns>
        public static bool TryGetValue<T>(this IDictionary<string, object> collection, string key, out T value)
        {
            if (collection == null)
            {
                throw Error.ArgumentNull("collection");
            }

            object valueObj;
            if (collection.TryGetValue(key, out valueObj))
            {
                if (valueObj is T)
                {
                    value = (T)valueObj;
                    return true;
                }
            }

            value = default(T);
            return false;
        }

        internal static IEnumerable<KeyValuePair<string, TValue>> FindKeysWithPrefix<TValue>(this IDictionary<string, TValue> dictionary, string prefix)
        {
            if (dictionary == null)
            {
                throw Error.ArgumentNull("dictionary");
            }

            if (prefix == null)
            {
                throw Error.ArgumentNull("prefix");
            }

            TValue exactMatchValue;
            if (dictionary.TryGetValue(prefix, out exactMatchValue))
            {
                yield return new KeyValuePair<string, TValue>(prefix, exactMatchValue);
            }

            foreach (var entry in dictionary)
            {
                string key = entry.Key;

                if (key.Length <= prefix.Length)
                {
                    continue;
                }

                if (!key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Everything is prefixed by the empty string
                if (prefix.Length == 0)
                {
                    yield return entry;
                }
                else
                {
                    char charAfterPrefix = key[prefix.Length];
                    switch (charAfterPrefix)
                    {
                        case '[':
                        case '.':
                            yield return entry;
                            break;
                    }
                }
            }
        }
    }
}
