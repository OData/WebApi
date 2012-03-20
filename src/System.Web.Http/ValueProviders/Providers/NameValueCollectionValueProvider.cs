using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Web.Http.Internal;

namespace System.Web.Http.ValueProviders.Providers
{
    public class NameValueCollectionValueProvider : IEnumerableValueProvider
    {
        private readonly CultureInfo _culture;
        private readonly Lazy<HashSet<string>> _prefixes;
        private readonly Lazy<NameValueCollection> _values;

        public NameValueCollectionValueProvider(NameValueCollection values, CultureInfo culture)
        {
            if (values == null)
            {
                throw Error.ArgumentNull("values");
            }

            _values = new Lazy<NameValueCollection>(() => values, isThreadSafe: true);
            _culture = culture;
            _prefixes = new Lazy<HashSet<string>>(CalculatePrefixes, isThreadSafe: true);
        }

        public NameValueCollectionValueProvider(Func<NameValueCollection> valuesFactory, CultureInfo culture)
        {
            if (valuesFactory == null)
            {
                throw Error.ArgumentNull("valuesFactory");
            }

            _values = new Lazy<NameValueCollection>(valuesFactory, isThreadSafe: true);
            _culture = culture;
            _prefixes = new Lazy<HashSet<string>>(CalculatePrefixes, isThreadSafe: true);
        }

        private HashSet<string> CalculatePrefixes()
        {
            HashSet<string> result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (_values.Value.Count > 0)
            {
                result.Add(String.Empty);
            }

            foreach (string key in _values.Value)
            {
                if (key != null)
                {
                    result.UnionWith(ValueProviderUtil.GetPrefixes(key));
                }
            }

            return result;
        }

        public virtual bool ContainsPrefix(string prefix)
        {
            if (prefix == null)
            {
                throw Error.ArgumentNull("prefix");
            }

            return _prefixes.Value.Contains(prefix);
        }

        public virtual IDictionary<string, string> GetKeysFromPrefix(string prefix)
        {
            if (prefix == null)
            {
                throw Error.ArgumentNull("prefix");
            }

            return ValueProviderUtil.GetKeysFromPrefix(_prefixes.Value, prefix);
        }

        public virtual ValueProviderResult GetValue(string key)
        {
            if (key == null)
            {
                throw Error.ArgumentNull("key");
            }

            string[] values = _values.Value.GetValues(key);
            if (values == null)
            {
                return null;
            }

            return new ValueProviderResult(values, _values.Value[key], _culture);
        }
    }
}
