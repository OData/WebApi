// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net.Http;
using Microsoft.AspNet.OData.Extensions;

namespace Microsoft.AspNet.OData.Test.Extensions
{
    /// <summary>
    /// Extensions for HttpRequestMessage.
    /// </summary>
    public static class HttpRequestMessageExtensions
    {
        /// <summary>
        /// Get the OData properties as context.
        /// </summary>
        /// <returns>The OData feature</returns>
        public static HttpRequestMessageProperties ODataContext(this HttpRequestMessage request)
        {
            return request.ODataProperties();
        }
    }
}
