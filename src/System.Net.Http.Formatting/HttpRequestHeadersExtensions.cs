// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Web.Http;

namespace System.Net.Http
{
    /// <summary>
    /// Provides extension methods for the <see cref="HttpRequestHeaders"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpRequestHeadersExtensions
    {
        private const string Cookie = "Cookie";

        /// <summary>
        /// Gets any cookie headers present in the request. Each <c>Cookie</c> header is 
        /// represented as one <see cref="CookieHeaderValue"/> instance. A <see cref="CookieHeaderValue"/>
        /// contains information about the domain, path, and other cookie information as well as one or
        /// more <see cref="CookieState"/> instances. Each <see cref="CookieState"/> instance contains
        /// a cookie name and whatever cookie state is associate with that name. The state is in the form of a 
        /// <see cref="System.Collections.Specialized.NameValueCollection"/> which on the wire is encoded as HTML Form URL-encoded data. 
        /// This representation allows for multiple related "cookies" to be carried within the same
        /// <c>Cookie</c> header while still providing separation between each cookie state. A sample
        /// <c>Cookie</c> header is shown below. In this example, there are two <see cref="CookieState"/>
        /// with names <c>state1</c> and <c>state2</c> respectively. Further, each cookie state contains two name/value
        /// pairs (name1/value1 and name2/value2) and (name3/value3 and name4/value4).
        /// <code>
        /// Cookie: state1:name1=value1&amp;name2=value2; state2:name3=value3&amp;name4=value4; domain=domain1; path=path1;
        /// </code>
        /// </summary>
        /// <param name="headers">The request headers</param>
        /// <returns>A collection of <see cref="CookieHeaderValue"/> instances.</returns>
        public static Collection<CookieHeaderValue> GetCookies(this HttpRequestHeaders headers)
        {
            if (headers == null)
            {
                throw Error.ArgumentNull("headers");
            }

            Collection<CookieHeaderValue> result = new Collection<CookieHeaderValue>();
            IEnumerable<string> cookieHeaders;
            if (headers.TryGetValues(Cookie, out cookieHeaders))
            {
                foreach (string cookieHeader in cookieHeaders)
                {
                    CookieHeaderValue cookieHeaderValue;
                    if (CookieHeaderValue.TryParse(cookieHeader, out cookieHeaderValue))
                    {
                        result.Add(cookieHeaderValue);
                    }
                }
            }
            return result;
        }
    }
}
