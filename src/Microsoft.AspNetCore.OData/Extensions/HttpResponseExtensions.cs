// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNet.OData.Extensions
{
    /// <summary>
    /// Provides extension methods for the <see cref="HttpResponseExtensions"/>.
    /// </summary>
    public static class HttpResponseExtensions
    {
        /// <summary>
        /// Determine if the response has a success status code.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>True if the response has a success status code; false otherwise.</returns>
        public static bool IsSuccessStatusCode(this HttpResponse response)
        {
            return response?.StatusCode >= 200 && response.StatusCode < 300;
        }
    }
}