//-----------------------------------------------------------------------------
// <copyright file="ODataNullValueMessageHandler.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Represents an <see href="HttpMessageHandler" /> that converts null values in OData responses to
    /// HTTP NotFound responses or NoContent responses following the OData specification.
    /// </summary>
    public partial class ODataNullValueMessageHandler : DelegatingHandler
    {
        /// <inheritdoc />
        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        protected async override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            // This message handler is intended for helping with queries that return a null value, for example in a
            // get request for a particular entity on an entity set, for a single valued navigation property or for
            // a structural property of a given entity. The only case in which a data modification request will result
            // in a 204 response status code, is when a primitive property is set to null through a PUT request to the
            // property URL and in that case, the user can return the right status code himself.
            ObjectContent content = response == null ? null : response.Content as ObjectContent;
            if (request.Method == System.Net.Http.HttpMethod.Get && content != null && content.Value == null &&
                response.StatusCode == HttpStatusCode.OK)
            {
                HttpStatusCode? newStatusCode = GetUpdatedResponseStatusCodeOrNull(request.ODataProperties().Path);
                if (newStatusCode.HasValue)
                {
                    response = request.CreateResponse(newStatusCode.Value);
                }
            }

            return response;
        }

        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        /// <remarks>This method is intended for unit testing purposes only.</remarks>
        internal Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            return SendAsync(request, CancellationToken.None);
        }
    }
}
