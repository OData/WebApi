//-----------------------------------------------------------------------------
// <copyright file="ODataBatchContent.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Batch
{
    /// <summary>
    /// Encapsulates a collection of OData batch responses.
    /// </summary>
    public partial class ODataBatchContent
    {
        private IServiceProvider _requestContainer;
        private ODataMessageWriterSettings _writerSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataBatchContent"/> class.
        /// </summary>
        /// <param name="responses">The batch responses.</param>
        /// <param name="requestContainer">The dependency injection container for the request.</param>
        private void Initialize(IEnumerable<ODataBatchResponseItem> responses, IServiceProvider requestContainer)
        {
            if (responses == null)
            {
                throw Error.ArgumentNull("responses");
            }

            Responses = responses;
            _requestContainer = requestContainer;
            _writerSettings = requestContainer.GetRequiredService<ODataMessageWriterSettings>();
        }

        /// <summary>
        /// Gets the batch responses.
        /// </summary>
        public IEnumerable<ODataBatchResponseItem> Responses { get; private set; }

        /// <summary>
        ///  Serialize the batch responses to an <see cref="IODataResponseMessage"/>.
        /// </summary>
        /// <param name="responseMessage">The response message.</param>
        /// <returns></returns>
        private async Task WriteToResponseMessageAsync(IODataResponseMessage responseMessage)
        {
            ODataMessageWriter messageWriter = new ODataMessageWriter(responseMessage, _writerSettings);
            ODataBatchWriter writer = await messageWriter.CreateODataBatchWriterAsync();

            await writer.WriteStartBatchAsync();

            foreach (ODataBatchResponseItem response in Responses)
            {
                await response.WriteResponseAsync(writer, /*asyncWriter*/ true);
            }

            await writer.WriteEndBatchAsync();
        }
    }
}
