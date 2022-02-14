//-----------------------------------------------------------------------------
// <copyright file="ETagMessageHandler.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Defines a <see cref="HttpMessageHandler"/> to add an ETag header value to an OData response when the response
    /// is a single resource that has an ETag defined.
    /// </summary>
    public partial class ETagMessageHandler : DelegatingHandler
    {
        /// <inheritdoc/>
        protected async override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            HttpConfiguration configuration = request.GetConfiguration();
            if (configuration == null)
            {
                throw Error.InvalidOperation(SRResources.RequestMustContainConfiguration);
            }

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            ObjectContent content = (response == null) ? null : response.Content as ObjectContent;
            if (content != null)
            {
                int? responseCode = (response == null) ? null : (int?)response.StatusCode;
                EntityTagHeaderValue etag = GetETag(
                    responseCode,
                    request.ODataProperties().Path,
                    request.GetModel(),
                    content.Value,
                    configuration.GetETagHandler());

                if (etag != null)
                {
                    response.Headers.ETag = etag;
                }
            }

            return response;
        }

        // This overload is for unit testing purposes only.
        internal Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            return SendAsync(request, CancellationToken.None);
        }
    }
}
