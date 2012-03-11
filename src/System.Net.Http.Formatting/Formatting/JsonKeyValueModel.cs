using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Json;

namespace System.Net.Http.Formatting
{
    /// <summary>
    /// Class that provides an <see cref="IKeyValueModel"/> facade over
    /// a <see cref="JsonValue"/>.
    /// </summary>
    internal class JsonKeyValueModel : IKeyValueModel
    {
        private readonly Dictionary<string, object> _valuesByPrefix = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        private JsonValue _innerJsonValue;

        /// <summary>
        /// Creates an instance of the <see cref="JsonKeyValueModel"/> class.
        /// </summary>
        /// <param name="jsonValue">The <see cref="JsonValue"/> from which to extract the keys and values</param>
        public JsonKeyValueModel(JsonValue jsonValue)
        {
            Contract.Assert(jsonValue != null, "jsonValue cannot be null.");

            _innerJsonValue = jsonValue;
            CreateValuesByPrefix();
        }

        /// <summary>
        /// Gets all the keys for all the values.
        /// </summary>
        /// <returns>The set of all keys.</returns>
        public IEnumerable<string> Keys
        {
            get { return _valuesByPrefix.Keys; }
        }

        /// <summary>
        /// Attempts to retrieve the value associated with the given key.
        /// </summary>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <param name="value">The value associated with that key.</param>
        /// <returns>
        ///   <c>If there was a value associated with that key</c>
        /// </returns>
        public bool TryGetValue(string key, out object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            return _valuesByPrefix.TryGetValue(key, out value);
        }

        private static object GetJsonValueContent(JsonValue jsonValue)
        {
            Contract.Assert(jsonValue.JsonType != JsonType.Array && jsonValue.JsonType != JsonType.Object);

            switch (jsonValue.JsonType)
            {
                case JsonType.Boolean:
                    return jsonValue.ReadAs<bool>();

                case JsonType.Default:
                    return jsonValue.ReadAs<object>();

                case JsonType.Number:
                    // preserve numbers as strings and let model binding convert based on culture
                    return jsonValue.ReadAs<string>();

                case JsonType.String:
                    return jsonValue.ReadAs<string>();

                default:
                    Contract.Assert(false, "unknown JsonType");
                    return null;
            }
        }

        private void CreateValuesByPrefix()
        {
            // A single value has no key name
            if (_innerJsonValue.JsonType != JsonType.Array && _innerJsonValue.JsonType != JsonType.Object)
            {
                _valuesByPrefix[String.Empty] = GetJsonValueContent(_innerJsonValue);
            }
            else
            {
                if (_innerJsonValue.JsonType != JsonType.Array)
                {
                    // if we are a complex object and not an array, we need to set this so
                    // that the collection modelbinder can bind a single object non-collection 
                    // to a collection parameter (array or IEnumerable)
                    _valuesByPrefix[String.Empty] = String.Empty;
                }

                ExpandJsonValue(null, _innerJsonValue);
            }
        }

        private void ExpandJsonValue(string currentPrefix, JsonValue jsonValue)
        {
            bool isJsonArray = jsonValue.JsonType == JsonType.Array;

            foreach (KeyValuePair<string, JsonValue> pair in jsonValue)
            {
                string seperator = (isJsonArray || currentPrefix == null) ? String.Empty : "."; // no seperator if we are an array or we are the outermost object (i[0] or rootnode)
                string currentSuffix = isJsonArray ? '[' + pair.Key + ']' : pair.Key; // use square brackets for indexing arrays

                string thisPrefix = currentPrefix + seperator + currentSuffix; // foo + . + bar , foo +  + [0]

                JsonValue thisJsonValue = pair.Value;
                if (thisJsonValue == null)
                {
                    _valuesByPrefix[thisPrefix] = null;
                }
                else
                {
                    if (thisJsonValue.JsonType != JsonType.Object && thisJsonValue.JsonType != JsonType.Array)
                    {
                        _valuesByPrefix[thisPrefix] = GetJsonValueContent(thisJsonValue);
                    }
                    else
                    {
                        ExpandJsonValue(thisPrefix, thisJsonValue);
                    }
                }
            }
        }
    }
}
