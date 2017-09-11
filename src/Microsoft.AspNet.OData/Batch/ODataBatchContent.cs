// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Batch
{
    /// <summary>
    /// Encapsulates a collection of OData batch responses.
    /// </summary>
    public class ODataBatchContent : HttpContent
    {
        private readonly IServiceProvider _requestContainer;
        private readonly ODataMessageWriterSettings _writerSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataBatchContent"/> class.
        /// </summary>
        /// <param name="responses">The batch responses.</param>
        /// <param name="requestContainer">The dependency injection container for the request.</param>
        public ODataBatchContent(IEnumerable<ODataBatchResponseItem> responses, IServiceProvider requestContainer)
        {
            if (responses == null)
            {
                throw Error.ArgumentNull("responses");
            }

            Responses = responses;
            _requestContainer = requestContainer;
            _writerSettings = requestContainer.GetRequiredService<ODataMessageWriterSettings>();
            Headers.ContentType = MediaTypeHeaderValue.Parse(String.Format(CultureInfo.InvariantCulture, "multipart/mixed;boundary=batchresponse_{0}", Guid.NewGuid()));
            ODataVersion version = _writerSettings.Version ?? HttpRequestMessageProperties.DefaultODataVersion;
            Headers.TryAddWithoutValidation(HttpRequestMessageProperties.ODataServiceVersionHeader, ODataUtils.ODataVersionToString(version));
        }

        /// <summary>
        /// Gets the batch responses.
        /// </summary>
        public IEnumerable<ODataBatchResponseItem> Responses { get; private set; }

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

        /// <inheritdoc/>
        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            IODataResponseMessage responseMessage = new ODataMessageWrapper(stream, Headers)
            {
                Container = _requestContainer
            };
            ODataMessageWriter messageWriter = new ODataMessageWriter(responseMessage, _writerSettings);
            ODataBatchWriter writer = messageWriter.CreateODataBatchWriter();

            writer.WriteStartBatch();

            foreach (ODataBatchResponseItem response in Responses)
            {
                await response.WriteResponseAsync(writer, CancellationToken.None);
            }

            writer.WriteEndBatch();
        }

        /// <inheritdoc/>
        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }
    }
}