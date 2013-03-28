// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.OData.Formatter;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Batch
{
    /// <summary>
    /// Provides extension methods for the <see cref="HttpContent"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ODataHttpContentExtensions
    {
        /// <summary>
        /// Gets the <see cref="ODataMessageReader"/> for the <see cref="HttpContent"/> stream.
        /// </summary>
        /// <param name="content">The <see cref="HttpContent"/>.</param>
        /// <param name="settings">The <see cref="ODataMessageReaderSettings"/>.</param>
        /// <returns>A <see cref="ODataMessageReader"/>.</returns>
        public static async Task<ODataMessageReader> GetODataMessageReaderAsync(this HttpContent content, ODataMessageReaderSettings settings)
        {
            if (content == null)
            {
                throw Error.ArgumentNull("content");
            }

            Stream contentStream = await content.ReadAsStreamAsync();
            IODataRequestMessage oDataRequestMessage = new ODataMessageWrapper(contentStream, content.Headers);
            ODataMessageReader oDataMessageReader = new ODataMessageReader(oDataRequestMessage, settings);
            return oDataMessageReader;
        }
    }
}