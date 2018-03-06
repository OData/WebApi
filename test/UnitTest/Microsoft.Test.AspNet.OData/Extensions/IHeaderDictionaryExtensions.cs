// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System.Net.Http.Headers;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
#else
using System.Net.Http.Headers;
#endif

namespace Microsoft.Test.AspNet.OData.Extensions
{
#if NETCORE
    /// <summary>
    /// Extensions for IHeaderDictionaryExtensions.
    /// </summary>
    public static class IHeaderDictionaryExtensions
    {
        /// <summary>
        /// Add to IfMatch values;
        /// </summary>
        /// <returns>The IfMatch values.</returns>
        public static void AddIfMatch(this IHeaderDictionary headers, EntityTagHeaderValue value)
        {
            StringValues newValue = StringValues.Concat(headers["If-Match"], new StringValues(value.ToString()));
            headers["If-Match"] = newValue;
        }

        /// <summary>
        /// Add to IfNoneMatch values.
        /// </summary>
        /// <returns>The IfNoneMatch values.</returns>
        public static void AddIfNoneMatch(this IHeaderDictionary headers, EntityTagHeaderValue value)
        {
            StringValues newValue = StringValues.Concat(headers["If-None-Match"], new StringValues(value.ToString()));
            headers["If-None-Match"] = newValue;
        }
    }
#else
    /// <summary>
    /// Extensions for HttpRequestMessage.
    /// </summary>
    public static class HttpRequestHeadersExtensions
    {
        /// <summary>
        /// Get the IfMatch values;
        /// </summary>
        /// <returns>The IfMatch values.</returns>
        public static void AddIfMatch(this HttpRequestHeaders headers, EntityTagHeaderValue value)
        {
            headers.IfMatch.Add(value);
        }

        /// <summary>
        /// Get the IfNoneMatch values.
        /// </summary>
        /// <returns>The IfNoneMatch values.</returns>
        public static void AddIfNoneMatch(this HttpRequestHeaders headers, EntityTagHeaderValue value)
        {
            headers.IfNoneMatch.Add(value);
        }
    }
#endif
}
