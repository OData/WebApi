//-----------------------------------------------------------------------------
// <copyright file="HttpClientExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Microsoft.Test.E2E.AspNet.OData.Common
{
    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> GetWithAcceptAsync(this HttpClient self, string uri, string acceptHeader)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Clear();
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(acceptHeader));

            return await self.SendAsync(request);
        }
        public static async Task<HttpResponseMessage> GetWithAcceptAsync(this HttpClient self, Uri uri, string acceptHeader)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Clear();
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(acceptHeader));

            return await self.SendAsync(request);
        }
    }
}
