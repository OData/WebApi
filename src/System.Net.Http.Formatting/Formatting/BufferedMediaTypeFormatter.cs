using System.IO;
using System.Net.Http.Headers;
using System.Net.Http.Internal;
using System.Threading.Tasks;

namespace System.Net.Http.Formatting
{
    /// <summary>
    /// Helper class to allow a synchronous formatter on top of the async formatter infrastructure. 
    /// This does not guarantee non-blocking threads. The only way to guarantee that we don't block a thread on IO is:
    /// a) use the async form, or 
    /// b) fully buffer the entire write operation.  
    /// The user opted out of the async form, meaning they can tolerate potential thread blockages.
    /// This class just tries to do smart buffering to minimize that blockage. 
    /// It also gives us a place to do future optimizations on synchronous usage. 
    /// </summary>
    public abstract class BufferedMediaTypeFormatter : MediaTypeFormatter
    {
        private const int DefaultBufferSize = 16 * 1024;

        private int _bufferSizeInBytes = DefaultBufferSize;

        /// <summary>
        /// Suggested size of buffer to use with streams, in bytes. 
        /// </summary>
        public int BufferSize
        {
            get { return _bufferSizeInBytes; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                _bufferSizeInBytes = value;
            }
        }

        /// <summary>
        /// Writes synchronously to the buffered stream.
        /// </summary>
        /// <param name="type">The type of the object to write.</param>
        /// <param name="value">The object value to write.  It may be <c>null</c>.</param>
        /// <param name="stream">The <see cref="Stream"/> to which to write.</param>
        /// <param name="contentHeaders">The <see cref="HttpContentHeaders"/> if available. It may be <c>null</c>.</param>
        public virtual void WriteToStream(Type type, object value, Stream stream, HttpContentHeaders contentHeaders)
        {
            throw new NotSupportedException(RS.Format(Properties.Resources.MediaTypeFormatterCannotWriteSync, GetType().Name));
        }

        /// <summary>
        /// Reads synchronously from the buffered stream.
        /// </summary>
        /// <param name="type">The type of the object to deserialize.</param>
        /// <param name="stream">The <see cref="Stream"/> to read.</param>
        /// <param name="contentHeaders">The <see cref="HttpContentHeaders"/> if available. It may be <c>null</c>.</param>
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

            // Underlying stream will do encoding into separate sections. This is just buffering.
            return TaskHelpers.RunSynchronously(
                () =>
                {
                    Stream bufferedStream = GetBufferStream(stream);

                    try
                    {
                        WriteToStream(type, value, bufferedStream, contentHeaders);
                    }
                    finally
                    {
                        // Disposing the bufferStream will dispose the underlying stream. 
                        // So Flush any remaining bytes that have been written, but don't actually close the stream.
                        bufferedStream.Flush();
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

            // See explanation in OnWriteToStreamAsync.
            return TaskHelpers.RunSynchronously<object>(
                () =>
                {
                    // When using a buffered read, the buffer really owns the underlying stream because it's whole purpose 
                    // is to eagerly read bytes from the underlying stream.
                    // This means this reader can't cooperate with other readers (in the same way that writers can). 
                    // So when this reader is done, we close the stream to prevent subsequent readers from getting random bytes. 
                    using (Stream bufferedStream = GetBufferStream(stream))
                    {
                        return ReadFromStream(type, bufferedStream, contentHeaders, formatterLogger);
                    }
                });
        }

        private Stream GetBufferStream(Stream inner)
        {
            // This uses a naive buffering. BufferedStream() will block the thread while it drains the buffer. 
            // We can explore a smarter implementation that async drains the buffer. 
            Stream bufferedStream = new BufferedStream(inner, _bufferSizeInBytes);

            return bufferedStream;
        }
    }
}
