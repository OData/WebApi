// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
        internal static Uri GetNextPageLink(Uri requestUri, int pageSize)
        {
            Contract.Assert(requestUri != null);
            Contract.Assert(requestUri.IsAbsoluteUri);

            return GetNextPageLink(requestUri, new FormDataCollection(requestUri), pageSize);
        }
    }
}
