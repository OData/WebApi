//-----------------------------------------------------------------------------
// <copyright file="WebApiRequestHeaders.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

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
        private IHeaderDictionary innerCollection;

        /// <summary>
        /// Initializes a new instance of the WebApiRequestMessage class.
        /// </summary>
        /// <param name="headers">The inner collection.</param>
        public WebApiRequestHeaders(IHeaderDictionary headers)
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
            StringValues stringValues;
            bool found = this.innerCollection.TryGetValue(key, out stringValues);

            values = stringValues.AsEnumerable();
            return found;
        }
    }
}
