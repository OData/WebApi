// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;

namespace System.Web.Mvc
{
    public class NameValueCollectionValueProvider : IValueProvider, IUnvalidatedValueProvider, IEnumerableValueProvider
    {
        private PrefixContainer _prefixContainer;
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

            foreach (string key in unvalidatedCollection)
            {
                if (key != null)
                {
                    // need to look up values lazily, as eagerly looking at the collection might trigger validation
                    _values[key] = new ValueProviderResultPlaceholder(key, collection, unvalidatedCollection, culture);
                }
            }
        }

        private PrefixContainer PrefixContainer
        {
            get
            {
                if (_prefixContainer == null)
                {
                    // Race condition on initialization has no side effects
                    _prefixContainer = new PrefixContainer(_values.Keys);
                }
                return _prefixContainer;
            }
        }

        public virtual bool ContainsPrefix(string prefix)
        {
            return PrefixContainer.ContainsPrefix(prefix);
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
            return PrefixContainer.GetKeysFromPrefix(prefix);
        }

        // Placeholder that can store a validated (in relation to request validation) or unvalidated
        // ValueProviderResult for a given key.
        private sealed class ValueProviderResultPlaceholder
        {
            private ValueProviderResult _validatedResult;
            private ValueProviderResult _unvalidatedResult;
            private string _key;
            private NameValueCollection _validatedCollection;
            private NameValueCollection _unvalidatedCollection;
            private CultureInfo _culture;

            public ValueProviderResultPlaceholder(string key, NameValueCollection validatedCollection, NameValueCollection unvalidatedCollection, CultureInfo culture)
            {
                _key = key;
                _validatedCollection = validatedCollection;
                _unvalidatedCollection = unvalidatedCollection;
                _culture = culture;
            }

            public ValueProviderResult ValidatedResult
            {
                get 
                {
                    if (_validatedResult == null)
                    {
                        _validatedResult = GetResultFromCollection(_key, _validatedCollection, _culture);
                    }
                    return _validatedResult;
                }
            }

            public ValueProviderResult UnvalidatedResult
            {
                get 
                {
                    if (_unvalidatedResult == null)
                    {
                        _unvalidatedResult = GetResultFromCollection(_key, _unvalidatedCollection, _culture);
                    }
                    return _unvalidatedResult;
                }
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
