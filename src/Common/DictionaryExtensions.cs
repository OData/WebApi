// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http.Properties;

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

        /// <summary>
        /// Gets the value of <typeparamref name="T"/> associated with the specified key or throw an <see cref="T:System.InvalidOperationException"/> 
        /// if either the key is not present or the value is not of type <typeparamref name="T"/>. 
        /// </summary>
        /// <param name="collection">The <see cref="IDictionary{TKey,TValue}"/> instance where <c>TValue</c> is <c>object</c>.</param>
        /// <param name="key">The key whose value to get.</param>
        /// <returns>An instance of type <typeparam name="T"/>.</returns>
        public static T GetValue<T>(this IDictionary<string, object> collection, string key)
        {
            if (collection == null)
            {
                throw Error.ArgumentNull("collection");
            }

            T value;
            if (collection.TryGetValue(key, out value))
            {
                return value;
            }

            throw Error.InvalidOperation(CommonWebApiResources.DictionaryMissingRequiredValue, collection.GetType().Name, key, typeof(T).Name);
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

        internal static bool DoesAnyKeyHavePrefix<TValue>(this IDictionary<string, TValue> dictionary, string prefix)
        {
            return FindKeysWithPrefix(dictionary, prefix).Any();
        }

        /// <summary>
        /// Adds a key/value pair of type <typeparamref name="T"/> to the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary{object, object}"/>
        ///     if the key does not already exist.
        /// </summary>
        /// <typeparam name="T">The actual type of the dictionary value.</typeparam>
        /// <param name="concurrentPropertyBag">A dictionary.</param>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="factory">The function used to generate a value for the <paramref name="key"/>.</param>
        /// <returns> The value for the key. This will be either the existing value for the <paramref name="key"/> if the key is already in the dictionary,
        /// or the new value for the key as returned by <paramref name="factory"/> if the key was not in the dictionary.</returns>
        internal static T GetOrAdd<T>(this ConcurrentDictionary<object, object> concurrentPropertyBag, object key, Func<object, T> factory)
        {
            Contract.Assert(concurrentPropertyBag != null);
            Contract.Assert(key != null);
            Contract.Assert(factory != null);

            // SIMPLIFYING ASSUMPTION: this method is internal and keys are private so it's assumed that client code won't be able to
            // replace the value with an object of a different type.

            return (T)concurrentPropertyBag.GetOrAdd(key, valueFactory: k => factory(k));
        }
    }
}
