//-----------------------------------------------------------------------------
// <copyright file="ODataBatchContent.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData.Routing;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Batch
{
    /// <summary>
    /// Encapsulates a collection of OData batch responses.
    /// </summary>
    /// <remarks>
    /// In AspNet, <see cref="ODataBatchContent"/> derives from <see cref="HttpContent"/>.
    /// </remarks>
    public partial class ODataBatchContent : HttpContent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataBatchContent"/> class.
        /// </summary>
        /// <param name="responses">The batch responses.</param>
        /// <param name="requestContainer">The dependency injection container for the request.</param>
        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        public ODataBatchContent(IEnumerable<ODataBatchResponseItem> responses, IServiceProvider requestContainer)
           : this(responses, requestContainer, null /*contentType*/)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataBatchContent"/> class.
        /// </summary>
        /// <param name="responses">The batch responses.</param>
        /// <param name="requestContainer">The dependency injection container for the request.</param>
        /// <param name="contentType">The response content type.</param>
        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        public ODataBatchContent(IEnumerable<ODataBatchResponseItem> responses, IServiceProvider requestContainer,
            MediaTypeHeaderValue contentType)
        {
            this.Initialize(responses, requestContainer);

            if (contentType == null)
            {
                contentType = MediaTypeHeaderValue.Parse(
                    String.Format(CultureInfo.InvariantCulture, "multipart/mixed;boundary=batchresponse_{0}", Guid.NewGuid()));
            }

            Headers.ContentType = contentType;
            ODataVersion version = _writerSettings.Version ?? ODataVersionConstraint.DefaultODataVersion;
            Headers.TryAddWithoutValidation(ODataVersionConstraint.ODataServiceVersionHeader, ODataUtils.ODataVersionToString(version));
        }

        /// <inheritdoc/>
        /// <remarks>This function uses types that are AspNet-specific.</remarks>
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            IODataResponseMessage responseMessage = ODataMessageWrapperHelper.Create(stream, this.Headers, _requestContainer);
            return WriteToResponseMessageAsync(responseMessage);
        }

        /// <inheritdoc/>
        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (ODataBatchResponseItem response in Responses)
                {
                    if (response != null)
                    {
                        response.Dispose();
                    }
                }
            }

            base.Dispose(disposing);
        }
    }
}
