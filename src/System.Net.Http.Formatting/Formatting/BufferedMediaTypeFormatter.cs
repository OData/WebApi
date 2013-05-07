// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
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
                    throw Error.ArgumentMustBeGreaterThanOrEqualTo("value", value, MinBufferSize);
                }
                _bufferSizeInBytes = value;
            }
        }

        /// <summary>
        /// Writes synchronously to the buffered stream.
        /// </summary>
        /// <remarks>
        /// An implementation of this method should close <paramref name="writeStream"/> upon completion.
        /// </remarks>
        /// <param name="type">The type of the object to write.</param>
        /// <param name="value">The object value to write.  It may be <c>null</c>.</param>
        /// <param name="writeStream">The <see cref="Stream"/> to which to write.</param>
        /// <param name="content">The <see cref="HttpContent"/> if available. Note that
        /// modifying the headers of the content will have no effect on the generated HTTP message; they should only be used to guide the writing.</param>
        public virtual void WriteToStream(Type type, object value, Stream writeStream, HttpContent content)
        {
            throw Error.NotSupported(Properties.Resources.MediaTypeFormatterCannotWriteSync, GetType().Name);
        }

        /// <summary>
        /// Reads synchronously from the buffered stream.
        /// </summary>
        /// <remarks>
        /// An implementation of this method should close <paramref name="readStream"/> upon completion.
        /// </remarks>
        /// <param name="type">The type of the object to deserialize.</param>
        /// <param name="readStream">The <see cref="Stream"/> to read.</param>
        /// <param name="content">The <see cref="HttpContent"/> if available.</param>
        /// <param name="formatterLogger">The <see cref="IFormatterLogger"/> to log events to.</param>
        /// <returns>An object of the given type.</returns>
        public virtual object ReadFromStream(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            throw Error.NotSupported(Properties.Resources.MediaTypeFormatterCannotReadSync, GetType().Name);
        }

        // Sealed because derived classes shouldn't override the async version. Override sync version instead.
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The caught exception type is reflected into a faulted task.")]
        public sealed override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            if (writeStream == null)
            {
                throw Error.ArgumentNull("writeStream");
            }

            try
            {
                WriteToStreamSync(type, value, writeStream, content);
                return TaskHelpers.Completed();
            }
            catch (Exception e)
            {
                return TaskHelpers.FromError(e);
            }
        }

        private void WriteToStreamSync(Type type, object value, Stream writeStream, HttpContent content)
        {
            using (Stream bufferedStream = GetBufferStream(writeStream, _bufferSizeInBytes))
            {
                WriteToStream(type, value, bufferedStream, content);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The caught exception type is reflected into a faulted task.")]
        public sealed override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            if (readStream == null)
            {
                throw Error.ArgumentNull("readStream");
            }

            try
            {
                return Task.FromResult(ReadFromStreamSync(type, readStream, content, formatterLogger));
            }
            catch (Exception e)
            {
                return TaskHelpers.FromError<object>(e);
            }
        }

        private object ReadFromStreamSync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            object result;
            HttpContentHeaders contentHeaders = content == null ? null : content.Headers;
            if (contentHeaders != null && contentHeaders.ContentLength == 0)
            {
                result = GetDefaultValueForType(type);
            }
            else
            {
                using (Stream bufferedStream = GetBufferStream(readStream, _bufferSizeInBytes))
                {
                    result = ReadFromStream(type, bufferedStream, content, formatterLogger);
                }
            }
            return result;
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
            return new BufferedStream(nonClosingStream, bufferSize);
        }
    }
}
