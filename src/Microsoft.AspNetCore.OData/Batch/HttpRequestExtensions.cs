//-----------------------------------------------------------------------------
// <copyright file="HttpRequestExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Batch
{
    /// <summary>
    /// Provides extension methods for the <see cref="HttpRequest"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpRequestExtensions
    {
        /// <summary>
        /// Gets the <see cref="IODataBatchFeature"/> from the services container.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The <see cref="IODataBatchFeature"/> from the services container.</returns>
        public static IODataBatchFeature ODataBatchFeature(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request.HttpContext.ODataBatchFeature();
        }

        /// <summary>
        /// Gets the <see cref="ODataMessageReader"/> for the <see cref="HttpRequest"/> stream.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="requestContainer">The dependency injection container for the request.</param>
        /// <param name="baseUri">The base uri.</param>
        /// <returns>A task object that produces an <see cref="ODataMessageReader"/> when completed.</returns>
        public static ODataMessageReader GetODataMessageReader(this HttpRequest request, IServiceProvider requestContainer, Uri baseUri = null)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            IODataRequestMessage oDataRequestMessage = ODataMessageWrapperHelper.Create(request.Body, request.Headers, requestContainer);

            // Let's clone the reader setting every time to reset the base uri.
            ODataMessageReaderSettings settings = requestContainer.GetRequiredService<ODataMessageReaderSettings>().Clone();
            if (baseUri != null)
            {
                settings.BaseUri = baseUri;
            }

            ODataMessageReader oDataMessageReader = new ODataMessageReader(oDataRequestMessage, settings);
            return oDataMessageReader;
        }

        /// <summary>
        /// Copy an absolute Uri to a <see cref="HttpRequest"/> stream.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="uri">The absolute uri to copy.</param>
        public static void CopyAbsoluteUrl(this HttpRequest request, Uri uri)
        {
            request.Scheme = uri.Scheme;
            request.Host = uri.IsDefaultPort ?
                new HostString(uri.Host) :
                new HostString(uri.Host, uri.Port);
            request.QueryString = new QueryString(uri.Query);
            var path = new PathString(uri.AbsolutePath);
            if (path.StartsWithSegments(request.PathBase, out PathString remainingPath))
            {
                path = remainingPath;
            }
            request.Path = path;
        }

        /// <summary>
        /// Copies the properties from another <see cref="HttpRequest"/>.
        /// </summary>
        /// <param name="subRequest">The sub-request.</param>
        /// <param name="batchRequest">The batch request that contains the properties to copy.</param>
        /// <remarks>
        /// Currently, this method is unused but is retained to keep a similar API surface area
        /// between the AspNet and AspNetCore versions of OData WebApi.
        /// </remarks>
        public static void CopyBatchRequestProperties(this HttpRequest subRequest, HttpRequest batchRequest)
        {
            if (subRequest == null)
            {
                throw new ArgumentNullException("subRequest");
            }
            if (batchRequest == null)
            {
                throw new ArgumentNullException("batchRequest");
            }
        }
    }
}
