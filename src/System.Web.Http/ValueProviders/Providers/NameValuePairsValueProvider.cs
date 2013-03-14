// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Threading;

namespace System.Web.Http.ValueProviders.Providers
{
    public class NameValuePairsValueProvider : IEnumerableValueProvider
    {
        private readonly CultureInfo _culture;
        private PrefixContainer _prefixContainer;
        private Dictionary<string, object> _values;
        private Func<IEnumerable<KeyValuePair<string, string>>> _valuesFactory;
        private object _valuesLock;
        private bool _valuesInitialized;

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Represents a collection of name/value pairs, cannot use NameValueCollection because it performs poorly")]
        public NameValuePairsValueProvider(IEnumerable<KeyValuePair<string, string>> values, CultureInfo culture)
        {
            if (values == null)
            {
                throw Error.ArgumentNull("values");
            }

            _values = InitializeValues(values);
            _culture = culture;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Represents a collection of name/value pairs, cannot use NameValueCollection because it performs poorly")]
        public NameValuePairsValueProvider(Func<IEnumerable<KeyValuePair<string, string>>> valuesFactory, CultureInfo culture)
        {
            if (valuesFactory == null)
            {
                throw Error.ArgumentNull("valuesFactory");
            }

            _valuesFactory = valuesFactory;
            _culture = culture;
        }

        // For unit testing purposes
        internal CultureInfo Culture
        {
            get
            {
                return _culture;
            }
        }

        private PrefixContainer PrefixContainer
        {
            get
            {
                PrefixContainer prefixContainer = Volatile.Read(ref _prefixContainer);
                if (prefixContainer == null)
                {
                    // Initialization race is OK providing data remains read-only and object identity is not significant
                    prefixContainer = new PrefixContainer(Values.Keys);
                    Volatile.Write(ref _prefixContainer, prefixContainer);
                }
                return prefixContainer;
            }
        }

        private Dictionary<string, object> Values
        {
            get
            {
                Dictionary<string, object> values = Volatile.Read(ref _values);
                if (values == null)
                {
                    return InitializeValuesThreadSafe();
                }
                return values;
            }
        }

        // This method turns a collection of name/value pairs into a Dictionary<string, object> for fast lookups
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "One of the casts is conditionally compiled")]
        private static Dictionary<string, object> InitializeValues(IEnumerable<KeyValuePair<string, string>> nameValuePairs)
        {
            // Performance-sensitive.
            // Optimize for the cases of 0 pairs, and for names being unique when present.
            Dictionary<string, object> values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            // Avoid looping in the 0 case.
            KeyValuePair<string, string>[] nameValuePairsArray = nameValuePairs as KeyValuePair<string, string>[];
            if (nameValuePairsArray != null && nameValuePairsArray.Length == 0)
            {
                return values;
            }
            foreach (KeyValuePair<string, string> nameValuePair in nameValuePairs)
            {
                string name = nameValuePair.Key;
                object value;
                // We optimize for the common case of a name being associated with exactly one value by avoiding a List
                // allocation if we can avoid it. The first time the key appears, the value gets stored as a string. 
                // Only if the key appears a second time do we allocate a List to store the values for that key.
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

        private Dictionary<string, object> InitializeValuesThreadSafe()
        {
            return LazyInitializer.EnsureInitialized(ref _values, ref _valuesInitialized, ref _valuesLock, () => InitializeValues(_valuesFactory()));
        }

        public virtual bool ContainsPrefix(string prefix)
        {
            return PrefixContainer.ContainsPrefix(prefix);
        }

        public virtual IDictionary<string, string> GetKeysFromPrefix(string prefix)
        {
            if (prefix == null)
            {
                throw Error.ArgumentNull("prefix");
            }

            return PrefixContainer.GetKeysFromPrefix(prefix);
        }

        public virtual ValueProviderResult GetValue(string key)
        {
            if (key == null)
            {
                throw Error.ArgumentNull("key");
            }

            object value;
            if (Values.TryGetValue(key, out value))
            {
                return new ValueProviderResult(value, GetAttemptedValue(value), _culture);
            }
            else
            {
                return null;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "One of the casts is conditionally compiled")]
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
