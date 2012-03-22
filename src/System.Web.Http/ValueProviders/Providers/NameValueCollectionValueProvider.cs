using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;

namespace System.Web.Http.ValueProviders.Providers
{
    public class NameValueCollectionValueProvider : IEnumerableValueProvider
    {
        private readonly CultureInfo _culture;
        private readonly Lazy<PrefixContainer> _prefixContainer;
        private readonly Lazy<NameValueCollection> _values;

        public NameValueCollectionValueProvider(NameValueCollection values, CultureInfo culture)
            : this(() => values, culture)
        {
            if (values == null)
            {
                throw Error.ArgumentNull("values");
            }
        }

        public NameValueCollectionValueProvider(Func<NameValueCollection> valuesFactory, CultureInfo culture)
        {
            if (valuesFactory == null)
            {
                throw Error.ArgumentNull("valuesFactory");
            }

            _values = new Lazy<NameValueCollection>(valuesFactory, isThreadSafe: true);
            _culture = culture;
            _prefixContainer = new Lazy<PrefixContainer>(() => new PrefixContainer(_values.Value.AllKeys), isThreadSafe: true);
        }

        public virtual bool ContainsPrefix(string prefix)
        {
            return _prefixContainer.Value.ContainsPrefix(prefix);
        }

        public virtual IDictionary<string, string> GetKeysFromPrefix(string prefix)
        {
            return _prefixContainer.Value.GetKeysFromPrefix(prefix);
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
