using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http.Common;
using System.Web.Http.Internal;

namespace System.Web.Http.ValueProviders.Providers
{
    /// <summary>
    /// This class provides a <see cref="IValueProvider"/> facade over a
    /// <see cref="IKeyValueModel"/> instance.
    /// </summary>
    public class KeyValueModelValueProvider : IValueProvider
    {
        private readonly Lazy<HashSet<string>> _prefixes;
        private IKeyValueModel _innerKeyValueProvider;
        private CultureInfo _culture;

        /// <summary>
        /// Creates a new instance of the <see cref="KeyValueModelValueProvider"/> class.
        /// </summary>
        /// <param name="provider">The inner <see cref="IKeyValueModel"/> to use.  It may be null.</param>
        /// <param name="culture">The culture to use.  It cannot be null.</param>
        public KeyValueModelValueProvider(IKeyValueModel provider, CultureInfo culture)
        {
            _innerKeyValueProvider = provider;
            _culture = culture;
            _prefixes = new Lazy<HashSet<string>>(CalculatePrefixes, isThreadSafe: true);
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

            if (_innerKeyValueProvider == null)
            {
                return null;
            }

            object value;
            if (!_innerKeyValueProvider.TryGetValue(key, out value))
            {
                return null;
            }

            return new ValueProviderResult(value, value == null ? String.Empty : value.ToString(), _culture);
        }

        private HashSet<string> CalculatePrefixes()
        {
            HashSet<string> result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (_innerKeyValueProvider != null)
            {
                IEnumerable<string> keys = _innerKeyValueProvider.Keys;

                if (keys.Any())
                {
                    result.Add(String.Empty);
                }

                foreach (string key in keys)
                {
                    if (key != null)
                    {
                        result.UnionWith(ValueProviderUtil.GetPrefixes(key));
                    }
                }
            }

            return result;
        }
    }
}
