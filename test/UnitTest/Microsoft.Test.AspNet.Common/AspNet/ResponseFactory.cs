// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;

namespace Microsoft.Test.AspNet.OData
{
    /// <summary>
    /// A class to create HttpResponse[Message].
    /// </summary>
    public class ResponseFactory
    {
        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
        public static HttpResponseMessage Create(HttpStatusCode statusCode, string content = null)
        {
            var response = new HttpResponseMessage(statusCode);
            if (content != null)
            {
                response.Content = new StringContent(content);
            }

            return response;
        }
    }
}
