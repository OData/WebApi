// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Net.Http.Headers;

namespace System.Net.Http
{
    /// <summary>
    /// An <see cref="IMultipartStreamProvider"/> implementation examines the headers provided by the MIME multipart parser
    /// as part of the MIME multipart extension methods (see <see cref="HttpContentMultipartExtensions"/>) and decides 
    /// what kind of stream to return for the body part to be written to.
    /// </summary>
    public interface IMultipartStreamProvider
    {
        /// <summary>
        /// When a MIME multipart body part has been parsed this method is called to get a stream for where to write the body part to.
        /// </summary>
        /// <param name="headers">Header fields describing the body part.</param>
        /// <returns>The <see cref="Stream"/> instance where the message body part is written to.</returns>
        Stream GetStream(HttpContentHeaders headers);
    }
}
