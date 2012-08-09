// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net.Http.Formatting.Parsers;
using System.Net.Http.Headers;
using System.Web.Http;

namespace System.Net.Http
{
    /// <summary>
    /// Maintains information about MIME body parts parsed by <see cref="MimeMultipartBodyPartParser"/>.
    /// </summary>
    internal class MimeBodyPart : IDisposable
    {
        private static readonly Type _streamType = typeof(Stream);
        private Stream _outputStream;
        private MultipartStreamProvider _streamProvider;
        private HttpContentHeaders _headers;

        /// <summary>
        /// Initializes a new instance of the <see cref="MimeBodyPart"/> class.
        /// </summary>
        /// <param name="streamProvider">The stream provider.</param>
        /// <param name="maxBodyPartHeaderSize">The max length of the MIME header within each MIME body part.</param>
        public MimeBodyPart(MultipartStreamProvider streamProvider, int maxBodyPartHeaderSize)
        {
            Contract.Assert(streamProvider != null);
            _streamProvider = streamProvider;
            Segments = new List<ArraySegment<byte>>(2);
            _headers = FormattingUtilities.CreateEmptyContentHeaders();
            HeaderParser = new InternetMessageFormatHeaderParser(_headers, maxBodyPartHeaderSize);
        }

        /// <summary>
        /// Gets the header parser.
        /// </summary>
        /// <value>
        /// The header parser.
        /// </value>
        public InternetMessageFormatHeaderParser HeaderParser { get; private set; }

        /// <summary>
        /// Gets the content of the HTTP.
        /// </summary>
        /// <value>
        /// The content of the HTTP.
        /// </value>
        public HttpContent HttpContent { get; private set; }

        /// <summary>
        /// Gets the set of <see cref="ArraySegment{T}"/> pointing to the read buffer with
        /// contents of this body part.
        /// </summary>
        public List<ArraySegment<byte>> Segments { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the body part has been completed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is complete; otherwise, <c>false</c>.
        /// </value>
        public bool IsComplete { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is the final body part.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is complete; otherwise, <c>false</c>.
        /// </value>
        public bool IsFinal { get; set; }

        /// <summary>
        /// Gets the output stream.
        /// </summary>
        /// <returns>The output stream to write the body part to.</returns>
        public Stream GetOutputStream(HttpContent parent)
        {
            Contract.Assert(parent != null);
            if (_outputStream == null)
            {
                try
                {
                    _outputStream = _streamProvider.GetStream(parent, _headers);
                }
                catch (Exception e)
                {
                    throw Error.InvalidOperation(e, Properties.Resources.ReadAsMimeMultipartStreamProviderException, _streamProvider.GetType().Name);
                }

                if (_outputStream == null)
                {
                    throw Error.InvalidOperation(Properties.Resources.ReadAsMimeMultipartStreamProviderNull, _streamProvider.GetType().Name, _streamType.Name);
                }

                if (!_outputStream.CanWrite)
                {
                    throw Error.InvalidOperation(Properties.Resources.ReadAsMimeMultipartStreamProviderReadOnly, _streamProvider.GetType().Name, _streamType.Name);
                }

                HttpContent = new StreamContent(_outputStream);
                _headers.CopyTo(HttpContent.Headers);
            }

            return _outputStream;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                CleanupOutputStream();
                HttpContent = null;
                HeaderParser = null;
                Segments.Clear();
            }
        }

        /// <summary>
        /// Resets the output stream by either closing it or, in the case of a <see cref="MemoryStream"/> resetting
        /// position to 0 so that it can be read by the caller.
        /// </summary>
        private void CleanupOutputStream()
        {
            if (_outputStream != null)
            {
                MemoryStream output = _outputStream as MemoryStream;
                if (output != null)
                {
                    output.Position = 0;
                }
                else
                {
#if NETFX_CORE
                    _outputStream.Dispose();
#else
                    _outputStream.Close();
#endif
                }

                _outputStream = null;
            }
        }
    }
}
