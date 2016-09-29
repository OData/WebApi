// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.OData.Extensions
{
    public static class HttpResponseExtensions
    {
        public static bool IsSuccessStatusCode(this HttpResponse response)
        {
            return response?.StatusCode >= 200 && response.StatusCode < 300;
        }
    }
}