using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace System.Net.Http
{
    /// <summary>
    /// Extension methods to allow strongly typed objects to be read from the query component of <see cref="Uri"/> instances.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class UriExtensions
    {
        /// <summary>
        /// Parses the query portion of the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="address">The <see cref="Uri"/> instance from which to read.</param>
        /// <returns>A <see cref="NameValueCollection"/> containing the parsed result.</returns>
        public static NameValueCollection ParseQueryString(this Uri address)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }

            return UriQueryUtility.ParseQueryString(address.Query);
        }

        /// <summary>
        /// Reads HTML form URL encoded data provided in the <see cref="Uri"/> query component as a <see cref="JToken"/> object.
        /// </summary>
        /// <param name="address">The <see cref="Uri"/> instance from which to read.</param>
        /// <param name="value">An object to be initialized with this instance or null if the conversion cannot be performed.</param>
        /// <returns><c>true</c> if the query component can be read as <see cref="JToken"/>; otherwise <c>false</c>.</returns>
        public static bool TryReadQueryAsJson(this Uri address, out JObject value)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }

            IEnumerable<KeyValuePair<string, string>> query = ParseQueryString(address.Query);
            return FormUrlEncodedJson.TryParse(query, out value);
        }

        /// <summary>
        /// Reads HTML form URL encoded data provided in the <see cref="Uri"/> query component as an <see cref="Object"/> of the given <paramref name="type"/>.
        /// </summary>
        /// <param name="address">The <see cref="Uri"/> instance from which to read.</param>
        /// <param name="type">The type of the object to read.</param>
        /// <param name="value">An object to be initialized with this instance or null if the conversion cannot be performed.</param>
        /// <returns><c>true</c> if the query component can be read as the specified type; otherwise <c>false</c>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate", Justification = "This is the non-generic version.")]
        public static bool TryReadQueryAs(this Uri address, Type type, out object value)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }

            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            IEnumerable<KeyValuePair<string, string>> query = ParseQueryString(address.Query);
            JObject jObject;
            if (FormUrlEncodedJson.TryParse(query, out jObject))
            {
                using (JTokenReader jsonReader = new JTokenReader(jObject))
                {
                    value = new JsonSerializer().Deserialize(jsonReader, type);
                }
                return true;
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Reads HTML form URL encoded data provided in the <see cref="Uri"/> query component as an <see cref="Object"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the object to read.</typeparam>
        /// <param name="address">The <see cref="Uri"/> instance from which to read.</param>
        /// <param name="value">An object to be initialized with this instance or null if the conversion cannot be performed.</param>
        /// <returns><c>true</c> if the query component can be read as the specified type; otherwise <c>false</c>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "The T represents the output parameter, not an input parameter.")]
        public static bool TryReadQueryAs<T>(this Uri address, out T value)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }

            IEnumerable<KeyValuePair<string, string>> query = ParseQueryString(address.Query);
            JObject jObject;
            if (FormUrlEncodedJson.TryParse(query, out jObject))
            {
                value = jObject.ToObject<T>();
                return true;
            }

            value = default(T);
            return false;
        }

        private static IEnumerable<KeyValuePair<string, string>> ParseQueryString(string queryString)
        {
            if (!String.IsNullOrEmpty(queryString))
            {
                if ((queryString.Length > 0) && (queryString[0] == '?'))
                {
                    queryString = queryString.Substring(1);
                }

                if (!String.IsNullOrEmpty(queryString))
                {
                    string[] pairs = queryString.Split('&');
                    foreach (string str in pairs)
                    {
                        string[] keyValue = str.Split('=');
                        if (keyValue.Length == 2)
                        {
                            yield return
                                keyValue[1].Equals(FormattingUtilities.JsonNullLiteral, StringComparison.Ordinal)
                                    ? new KeyValuePair<string, string>(UriQueryUtility.UrlDecode(keyValue[0]), null)
                                    : new KeyValuePair<string, string>(UriQueryUtility.UrlDecode(keyValue[0]), UriQueryUtility.UrlDecode(keyValue[1]));
                        }
                        else if (keyValue.Length == 1)
                        {
                            yield return new KeyValuePair<string, string>(null, UriQueryUtility.UrlDecode(keyValue[0]));
                        }
                    }
                }
            }
        }
    }
}
