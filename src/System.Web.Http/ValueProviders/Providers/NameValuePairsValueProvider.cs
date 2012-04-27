// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace System.Web.Http.ValueProviders.Providers
{
    public class NameValuePairsValueProvider : IEnumerableValueProvider
    {
        private readonly CultureInfo _culture;
        private readonly Lazy<PrefixContainer> _prefixContainer;
        private readonly Lazy<Dictionary<string, object>> _values;

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Represents a collection of name/value pairs, cannot use NameValueCollection because it performs poorly")]
        public NameValuePairsValueProvider(IEnumerable<KeyValuePair<string, string>> values, CultureInfo culture)
            : this(() => values, culture)
        {
            if (values == null)
            {
                throw Error.ArgumentNull("values");
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Represents a collection of name/value pairs, cannot use NameValueCollection because it performs poorly")]
        public NameValuePairsValueProvider(Func<IEnumerable<KeyValuePair<string, string>>> valuesFactory, CultureInfo culture)
        {
            if (valuesFactory == null)
            {
                throw Error.ArgumentNull("valuesFactory");
            }

            _values = new Lazy<Dictionary<string, object>>(() => InitializeValues(valuesFactory()), isThreadSafe: true);
            _culture = culture;
            _prefixContainer = new Lazy<PrefixContainer>(() => new PrefixContainer(_values.Value.Keys), isThreadSafe: true);
        }

        // For unit testing purposes
        internal CultureInfo Culture
        {
            get
            {
                return _culture;
            }
        }

        // This method turns a collection of name/value pairs into a Dictionary<string, object> for fast lookups
        // We optimize for the common case of a name being associated with exactly one value by avoiding a List allocation if we can avoid it
        // The first time the key appears, the value gets stored as a string. Only if the key appears a second time do we allocate a List to store the values for that key.
        private static Dictionary<string, object> InitializeValues(IEnumerable<KeyValuePair<string, string>> nameValuePairs)
        {
            Dictionary<string, object> values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, string> nameValuePair in nameValuePairs)
            {
                string name = nameValuePair.Key;
                object value;
                if (values.TryGetValue(name, out value))
                {
                    List<string> valueStrings = value as List<string>;
                    if (valueStrings == null)
                    {
                        Contract.Assert(value is string);

                        // allocate a new list to store the first value and the second, new value
                        values[name] = new List<string>() { value as string, nameValuePair.Value };
                    }
                    else
                    {
                        valueStrings.Add(nameValuePair.Value);
                    }         
                }
                else
                {
                    values[name] = nameValuePair.Value;
                }
            }
            return values;
        }

        public virtual bool ContainsPrefix(string prefix)
        {
            return _prefixContainer.Value.ContainsPrefix(prefix);
        }

        public virtual IDictionary<string, string> GetKeysFromPrefix(string prefix)
        {
            if (prefix == null)
            {
                throw Error.ArgumentNull("prefix");
            }

            return _prefixContainer.Value.GetKeysFromPrefix(prefix);
        }

        public virtual ValueProviderResult GetValue(string key)
        {
            if (key == null)
            {
                throw Error.ArgumentNull("key");
            }

            object value;
            if (_values.Value.TryGetValue(key, out value))
            {
                return new ValueProviderResult(value, GetAttemptedValue(value), _culture);
            }
            else
            {
                return null;
            }
        }

        private static string GetAttemptedValue(object value)
        {
            List<string> valueStrings = value as List<string>;
            if (valueStrings == null)
            {
                Contract.Assert(value is string);
                return value as string;
            }
            else
            {
                return String.Join(",", valueStrings);
            }
        }
    }
}
