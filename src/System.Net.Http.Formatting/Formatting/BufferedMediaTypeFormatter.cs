// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http.Internal;
using System.Threading.Tasks;
using System.Web.Http;

namespace System.Net.Http.Formatting
{
    /// <summary>
    /// Base class for writing a synchronous formatter on top of the asynchronous formatter infrastructure. 
    /// This does not guarantee non-blocking threads. The only way to guarantee that we don't block a thread on IO is
    /// to use the asynchronous <see cref="MediaTypeFormatter"/>.
    /// </summary>
    public abstract class BufferedMediaTypeFormatter : MediaTypeFormatter
    {
        private const int MinBufferSize = 0;
        private const int DefaultBufferSize = 16 * 1024;

        private int _bufferSizeInBytes = DefaultBufferSize;

        /// <summary>
        /// Suggested size of buffer to use with streams, in bytes. The default size is 16K.
        /// </summary>
        public int BufferSize
        {
            get { return _bufferSizeInBytes; }
            set
            {
                if (value < MinBufferSize)
                {
                    throw Error.ArgumentGreaterThanOrEqualTo("value", value, MinBufferSize);
                }
                _bufferSizeInBytes = value;
            }
        }

        /// <summary>
        /// Writes synchronously to the buffered stream.
        /// </summary>
        /// <remarks>
        /// An implementation of this method should close <paramref name="stream"/> upon completion.
        /// </remarks>
        /// <param name="type">The type of the object to write.</param>
        /// <param name="value">The object value to write.  It may be <c>null</c>.</param>
        /// <param name="stream">The <see cref="Stream"/> to which to write.</param>
        /// <param name="contentHeaders">The <see cref="HttpContentHeaders"/> if available. Note that
        /// modifying the headers will have no effect on the generated HTTP message; they should only be used to guide the writing.</param>
        public virtual void WriteToStream(Type type, object value, Stream stream, HttpContentHeaders contentHeaders)
        {
            throw new NotSupportedException(RS.Format(Properties.Resources.MediaTypeFormatterCannotWriteSync, GetType().Name));
        }

        /// <summary>
        /// Reads synchronously from the buffered stream.
        /// </summary>
        /// <remarks>
        /// An implementation of this method should close <paramref name="stream"/> upon completion.
        /// </remarks>
        /// <param name="type">The type of the object to deserialize.</param>
        /// <param name="stream">The <see cref="Stream"/> to read.</param>
        /// <param name="contentHeaders">The <see cref="HttpContentHeaders"/> if available.</param>
        /// <param name="formatterLogger">The <see cref="IFormatterLogger"/> to log events to.</param>
        /// <returns>An object of the given type.</returns>
        public virtual object ReadFromStream(Type type, Stream stream, HttpContentHeaders contentHeaders, IFormatterLogger formatterLogger)
        {
            throw new NotSupportedException(RS.Format(Properties.Resources.MediaTypeFormatterCannotReadSync, GetType().Name));
        }

        // Sealed because derived classes shouldn't override the async version. Override sync version instead.
        public sealed override Task WriteToStreamAsync(Type type, object value, Stream stream, HttpContentHeaders contentHeaders, TransportContext transportContext)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            return TaskHelpers.RunSynchronously(
                () =>
                {
                    using (Stream bufferedStream = GetBufferStream(stream, _bufferSizeInBytes))
                    {
                        WriteToStream(type, value, bufferedStream, contentHeaders);
                    }
                });
        }

        public sealed override Task<object> ReadFromStreamAsync(Type type, Stream stream, HttpContentHeaders contentHeaders, IFormatterLogger formatterLogger)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            return TaskHelpers.RunSynchronously<object>(
                () =>
                {
                    if (contentHeaders != null && contentHeaders.ContentLength == 0)
                    {
                        return GetDefaultValueForType(type);
                    }

                    using (Stream bufferedStream = GetBufferStream(stream, _bufferSizeInBytes))
                    {
                        return ReadFromStream(type, bufferedStream, contentHeaders, formatterLogger);
                    }
                });
        }

        private static Stream GetBufferStream(Stream innerStream, int bufferSize)
        {
            Contract.Assert(innerStream != null);

            // We wrap the inner stream in a non-closing delegating stream so that we allow the user 
            // to use the using (...) pattern yet not break the contract of formatters not closing
            // the inner stream.
            Stream nonClosingStream = new NonClosingDelegatingStream(innerStream);

            // This uses a naive buffering. BufferedStream() will block the thread while it drains the buffer. 
            // We can explore a smarter implementation that async drains the buffer. 
            Stream bufferedStream = new BufferedStream(nonClosingStream, bufferSize);

            // We now have buffered, non-closing stream which we can pass to the user.
            return bufferedStream;
        }
    }
}
