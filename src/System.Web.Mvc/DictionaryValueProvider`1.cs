using System.Collections.Generic;
using System.Globalization;

namespace System.Web.Mvc
{
    public class DictionaryValueProvider<TValue> : IValueProvider, IEnumerableValueProvider
    {
        private readonly HashSet<string> _prefixes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, ValueProviderResult> _values = new Dictionary<string, ValueProviderResult>(StringComparer.OrdinalIgnoreCase);

        public DictionaryValueProvider(IDictionary<string, TValue> dictionary, CultureInfo culture)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException("dictionary");
            }

            AddValues(dictionary, culture);
        }

        private void AddValues(IDictionary<string, TValue> dictionary, CultureInfo culture)
        {
            if (dictionary.Count > 0)
            {
                _prefixes.Add(String.Empty);
            }

            foreach (var entry in dictionary)
            {
                _prefixes.UnionWith(ValueProviderUtil.GetPrefixes(entry.Key));

                object rawValue = entry.Value;
                string attemptedValue = Convert.ToString(rawValue, culture);
                _values[entry.Key] = new ValueProviderResult(rawValue, attemptedValue, culture);
            }
        }

        public virtual bool ContainsPrefix(string prefix)
        {
            if (prefix == null)
            {
                throw new ArgumentNullException("prefix");
            }

            return _prefixes.Contains(prefix);
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
            return ValueProviderUtil.GetKeysFromPrefix(_prefixes, prefix);
        }
    }
}
