//-----------------------------------------------------------------------------
// <copyright file="WebApiRequestHeaders.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Net.Http.Headers;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Interfaces;

namespace Microsoft.AspNet.OData.Adapters
{
    /// <summary>
    /// Adapter class to convert Asp.Net WebApi request headers to OData WebApi.
    /// </summary>
    internal class WebApiRequestHeaders : IWebApiHeaders
    {
        /// <summary>
        /// The inner collection wrapped by this instance.
        /// </summary>
        private HttpRequestHeaders innerCollection;

        /// <summary>
        /// Initializes a new instance of the WebApiRequestMessage class.
        /// </summary>
        /// <param name="headers">The inner collection.</param>
        public WebApiRequestHeaders(HttpRequestHeaders headers)
        {
            if (headers == null)
            {
                throw Error.ArgumentNull("headers");
            }

            this.innerCollection = headers;
        }

        /// <summary>
        /// Return if a specified header and specified values are stored in the collection.
        /// </summary>
        /// <param name="key">The specified header.</param>
        /// <param name="values">The specified header values.</param>
        /// <returns>true is the specified header name and values are stored in the collection; otherwise false.</returns>
        public bool TryGetValues(string key, out IEnumerable<string> values)
        {
            return this.innerCollection.TryGetValues(key, out values);
        }
    }
}
