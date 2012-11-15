// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;

namespace System.Web.Mvc
{
    public class DictionaryValueProvider<TValue> : IValueProvider, IEnumerableValueProvider
    {
        // The class could be read from multiple threads, so mark volatile to ensure the lazy initialization works on all memory models
        private volatile PrefixContainer _prefixContainer;
        private readonly Dictionary<string, ValueProviderResult> _values = new Dictionary<string, ValueProviderResult>(StringComparer.OrdinalIgnoreCase);

        public DictionaryValueProvider(IDictionary<string, TValue> dictionary, CultureInfo culture)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException("dictionary");
            }

            foreach (KeyValuePair<string, TValue> entry in dictionary)
            {
                object rawValue = entry.Value;
                string attemptedValue = Convert.ToString(rawValue, culture);
                _values[entry.Key] = new ValueProviderResult(rawValue, attemptedValue, culture);
            }
        }

        private PrefixContainer PrefixContainer
        {
            get
            {
                // The class could be read from multiple threads, which could result in a race condition where this is created more than once.
                // Ensure that:
                //     - The input data and object remain read-only
                //     - There is no dependency on the identity
                //     - The object is not modified after assignment
                //     - The field remains declared as volatile
                //     - Use a local to minimize volatile operations on the common code path
                PrefixContainer prefixContainer = _prefixContainer;
                if (prefixContainer == null)
                {
                    prefixContainer = new PrefixContainer(_values.Keys);
                    _prefixContainer = prefixContainer;
                }
                return prefixContainer;
            }
        }

        public virtual bool ContainsPrefix(string prefix)
        {
            return PrefixContainer.ContainsPrefix(prefix);
        }

        public virtual ValueProviderResult GetValue(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            ValueProviderResult valueProviderResult;
            _values.TryGetValue(key, out valueProviderResult);
            return valueProviderResult;
        }

        public virtual IDictionary<string, string> GetKeysFromPrefix(string prefix)
        {
            return PrefixContainer.GetKeysFromPrefix(prefix);
        }
    }
}
