//-----------------------------------------------------------------------------
// <copyright file="ODataBatchRequestHeaders.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.AspNet.OData.Batch
{
    internal static class ODataBatchRequestHeaders
    {
        private static readonly HashSet<string> blockedHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Authorization",
            "Proxy-Authorization",
            "Host",
            "Forwarded",
            "X-Forwarded-For",
            "X-Forwarded-Host",
            "X-Forwarded-Proto",
            "X-Forwarded-Scheme",
            "X-Original-Host",
            "X-Real-IP",
            "X-ARR-ClientCert",
            "X-ARR-LOG-ID",
            "X-MS-Client-Principal-Id",
            "X-MS-Client-Principal-Name",
            "X-MS-Client-Principal-IdP",
        };

        internal static bool IsBlocked(string headerName)
        {
            return blockedHeaders.Contains(headerName);
        }
    }
}
