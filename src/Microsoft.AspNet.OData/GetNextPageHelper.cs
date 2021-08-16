//-----------------------------------------------------------------------------
// <copyright file="GetNextPageHelper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.Contracts;
using System.Net.Http.Formatting;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Helper to generate next page links.
    /// </summary>
    internal static partial class GetNextPageHelper
    {
        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        internal static Uri GetNextPageLink(Uri requestUri, int pageSize, object instance = null, Func<object, string> objectToSkipTokenValue = null)
        {
            Contract.Assert(requestUri != null);

            return GetNextPageLink(requestUri, new FormDataCollection(requestUri), pageSize, instance, objectToSkipTokenValue);
        }
    }
}
