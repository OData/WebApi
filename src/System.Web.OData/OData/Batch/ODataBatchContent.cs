// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData.Extensions;
using System.Web.OData.Formatter;
using Microsoft.OData.Core;

namespace System.Web.OData.Batch
{
    /// <summary>
    /// Encapsulates a collection of OData batch responses.
    /// </summary>
    public class ODataBatchContent : HttpContent
    {
        private ODataMessageWriterSettings _writerSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataBatchContent"/> class.
        /// </summary>
        /// <param name="responses">The batch responses.</param>
        public ODataBatchContent(IEnumerable<ODataBatchResponseItem> responses)
            : this(responses, new ODataMessageWriterSettings())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataBatchContent"/> class.
        /// </summary>
        /// <param name="responses">The batch responses.</param>
        /// <param name="writerSettings">The <see cref="ODataMessageWriterSettings"/>.</param>
        public ODataBatchContent(IEnumerable<ODataBatchResponseItem> responses, ODataMessageWriterSettings writerSettings)
        {
            if (responses == null)
            {
                throw Error.ArgumentNull("responses");
            }
            if (writerSettings == null)
            {
                throw Error.ArgumentNull("writerSettings");
            }

            Responses = responses;
            _writerSettings = writerSettings;
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
            IODataResponseMessage responseMessage = new ODataMessageWrapper(stream, Headers);
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