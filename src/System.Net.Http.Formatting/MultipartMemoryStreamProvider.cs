// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Net.Http.Headers;

namespace System.Net.Http
{
    /// <summary>
    /// Provides a <see cref="IMultipartStreamProvider"/> implementation that returns a <see cref="MemoryStream"/> instance.
    /// This facilitates deserialization or other manipulation of the contents in memory.
    /// </summary>
    internal class MultipartMemoryStreamProvider : IMultipartStreamProvider
    {
        private static MultipartMemoryStreamProvider instance = new MultipartMemoryStreamProvider();

        private MultipartMemoryStreamProvider()
        {
        }

        /// <summary>
        /// Gets a static instance of the <see cref="MultipartMemoryStreamProvider"/>
        /// </summary>
        public static MultipartMemoryStreamProvider Instance
        {
            get { return MultipartMemoryStreamProvider.instance; }
        }

        /// <summary>
        /// This <see cref="IMultipartStreamProvider"/> implementation returns a <see cref="MemoryStream"/> instance.
        /// This facilitates deserialization or other manipulation of the contents in memory. 
        /// </summary>
        /// <param name="headers">The header fields describing the body parts content. Looking for header fields such as 
        /// Content-Type and Content-Disposition can help provide the appropriate stream. In addition to using the information
        /// in the provided header fields, it is also possible to add new header fields or modify existing header fields. This can
        /// be useful to get around situations where the Content-type may say <b>application/octet-stream</b> but based on
        /// analyzing the <b>Content-Disposition</b> header field it is found that the content in fact is <b>application/json</b>, for example.</param>
        /// <returns>A stream instance where the contents of a body part will be written to.</returns>
        public Stream GetStream(HttpContentHeaders headers)
        {
            if (headers == null)
            {
                throw new ArgumentNullException("headers");
            }

            return new MemoryStream();
        }
    }
}
