//-----------------------------------------------------------------------------
// <copyright file="GetNextPageHelper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Helper to generate next page links.
    /// </summary>
    internal static partial class GetNextPageHelper
    {
        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        internal static Uri GetNextPageLink(Uri requestUri, int pageSize, object instance = null, Func<object, String> objectToSkipTokenValue = null)
        {
            Contract.Assert(requestUri != null);

            Dictionary<string, StringValues> queryValues = QueryHelpers.ParseQuery(requestUri.Query);
            IEnumerable<KeyValuePair<string, string>> queryParameters = queryValues.SelectMany(
                kvp => kvp.Value, (kvp, value) => new KeyValuePair<string, string>(kvp.Key, value));

            return GetNextPageLink(requestUri, queryParameters, pageSize, instance, objectToSkipTokenValue);
        }
    }
}
