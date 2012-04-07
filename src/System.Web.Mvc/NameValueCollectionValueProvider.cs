// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Threading;

namespace System.Web.Mvc
{
    public class NameValueCollectionValueProvider : IValueProvider, IUnvalidatedValueProvider, IEnumerableValueProvider
    {
        private readonly Lazy<PrefixContainer> _prefixContainer;
        private readonly Dictionary<string, ValueProviderResultPlaceholder> _values = new Dictionary<string, ValueProviderResultPlaceholder>(StringComparer.OrdinalIgnoreCase);

        public NameValueCollectionValueProvider(NameValueCollection collection, CultureInfo culture)
            : this(collection, unvalidatedCollection: null, culture: culture)
        {
        }

        public NameValueCollectionValueProvider(NameValueCollection collection, NameValueCollection unvalidatedCollection, CultureInfo culture)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }

            unvalidatedCollection = unvalidatedCollection ?? collection;

            // Need to read keys from the unvalidated collection, as M.W.I's granular request validation is a bit touchy
            // and validated entries at the time the key or value is looked at. For example, GetKey() will throw if the
            // value fails request validation, even though the value's not being looked at (M.W.I can't tell the difference).

            _prefixContainer = new Lazy<PrefixContainer>(() => new PrefixContainer(unvalidatedCollection.AllKeys), isThreadSafe: true);

            foreach (string key in unvalidatedCollection)
            {
                if (key != null)
                {
                    // need to look up values lazily, as eagerly looking at the collection might trigger validation
                    _values[key] = new ValueProviderResultPlaceholder(key, collection, unvalidatedCollection, culture);
                }
            }
        }

        public virtual bool ContainsPrefix(string prefix)
        {
            return _prefixContainer.Value.ContainsPrefix(prefix);
        }

        public virtual ValueProviderResult GetValue(string key)
        {
            return GetValue(key, skipValidation: false);
        }

        public virtual ValueProviderResult GetValue(string key, bool skipValidation)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            ValueProviderResultPlaceholder placeholder;
            _values.TryGetValue(key, out placeholder);
            if (placeholder == null)
            {
                return null;
            }
            else
            {
                return (skipValidation) ? placeholder.UnvalidatedResult : placeholder.ValidatedResult;
            }
        }

        public virtual IDictionary<string, string> GetKeysFromPrefix(string prefix)
        {
            return _prefixContainer.Value.GetKeysFromPrefix(prefix);
        }

        // Placeholder that can store a validated (in relation to request validation) or unvalidated
        // ValueProviderResult for a given key.
        private sealed class ValueProviderResultPlaceholder
        {
            private readonly Lazy<ValueProviderResult> _validatedResultPlaceholder;
            private readonly Lazy<ValueProviderResult> _unvalidatedResultPlaceholder;

            public ValueProviderResultPlaceholder(string key, NameValueCollection validatedCollection, NameValueCollection unvalidatedCollection, CultureInfo culture)
            {
                _validatedResultPlaceholder = new Lazy<ValueProviderResult>(() => GetResultFromCollection(key, validatedCollection, culture), LazyThreadSafetyMode.None);
                _unvalidatedResultPlaceholder = new Lazy<ValueProviderResult>(() => GetResultFromCollection(key, unvalidatedCollection, culture), LazyThreadSafetyMode.None);
            }

            public ValueProviderResult ValidatedResult
            {
                get { return _validatedResultPlaceholder.Value; }
            }

            public ValueProviderResult UnvalidatedResult
            {
                get { return _unvalidatedResultPlaceholder.Value; }
            }

            private static ValueProviderResult GetResultFromCollection(string key, NameValueCollection collection, CultureInfo culture)
            {
                string[] rawValue = collection.GetValues(key);
                string attemptedValue = collection[key];
                return new ValueProviderResult(rawValue, attemptedValue, culture);
            }
        }
    }
}
