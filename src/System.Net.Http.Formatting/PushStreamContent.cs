// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Net.Http.Internal;
using System.Threading.Tasks;
using System.Web.Http;

namespace System.Net.Http
{
    /// <summary>
    /// Provides an <see cref="HttpContent"/> implementation that exposes an output <see cref="Stream"/>
    /// which can be written to directly. The ability to push data to the output stream differs from the 
    /// <see cref="StreamContent"/> where data is pulled and not pushed.
    /// </summary>
    public class PushStreamContent : HttpContent
    {
        private readonly Action<Stream, HttpContent, TransportContext> _onStreamAvailable;

        /// <summary>
        /// Initializes a new instance of the <see cref="PushStreamContent"/> class. The
        /// <paramref name="onStreamAvailable"/> action is called when an output stream
        /// has become available allowing the action to write to it directly. When the 
        /// stream is closed, it will signal to the content that is has completed and the 
        /// HTTP request or response will be completed.
        /// </summary>
        /// <param name="onStreamAvailable">The action to call when an output stream
        /// is available. Close the stream to complete the HTTP request or response.</param>
        public PushStreamContent(Action<Stream, HttpContent, TransportContext> onStreamAvailable)
            : this(onStreamAvailable, (MediaTypeHeaderValue)null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PushStreamContent"/> class
        /// with the given media type.
        /// </summary>
        public PushStreamContent(Action<Stream, HttpContent, TransportContext> onStreamAvailable, string mediaType)
            : this(onStreamAvailable, new MediaTypeHeaderValue(mediaType))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PushStreamContent"/> class
        /// with the given <see cref="MediaTypeHeaderValue"/>.
        /// </summary>
        public PushStreamContent(Action<Stream, HttpContent, TransportContext> onStreamAvailable, MediaTypeHeaderValue mediaType)
        {
            if (onStreamAvailable == null)
            {
                throw Error.ArgumentNull("onStreamAvailable");
            }

            _onStreamAvailable = onStreamAvailable;
            Headers.ContentType = mediaType ?? MediaTypeConstants.ApplicationOctetStreamMediaType;
        }

        /// <summary>
        /// When this method is called, it calls the action provided in the constructor with the output 
        /// stream to write to. Once the action has completed its work it closes the stream which will 
        /// close this content instance and complete the HTTP request or response.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to which to write.</param>
        /// <param name="context">The associated <see cref="TransportContext"/>.</param>
        /// <returns>A <see cref="Task"/> instance that is asynchronously serializing the object's content.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is passed as task result.")]
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            TaskCompletionSource<bool> serializeToStreamTask = new TaskCompletionSource<bool>();
            try
            {
                Stream wrappedStream = new CompleteTaskOnCloseStream(stream, serializeToStreamTask);
                _onStreamAvailable(wrappedStream, this, context);
            }
            catch (Exception e)
            {
                serializeToStreamTask.TrySetException(e);
            }

            return serializeToStreamTask.Task;
        }

        /// <summary>
        /// Computes the length of the stream if possible.
        /// </summary>
        /// <param name="length">The computed length of the stream.</param>
        /// <returns><c>true</c> if the length has been computed; otherwise <c>false</c>.</returns>
        protected override bool TryComputeLength(out long length)
        {
            // We can't know the length of the content being pushed to the output stream.
            length = -1;
            return false;
        }

        internal class CompleteTaskOnCloseStream : DelegatingStream
        {
            private TaskCompletionSource<bool> _serializeToStreamTask;

            public CompleteTaskOnCloseStream(Stream innerStream, TaskCompletionSource<bool> serializeToStreamTask)
                : base(innerStream)
            {
                Contract.Assert(serializeToStreamTask != null);
                _serializeToStreamTask = serializeToStreamTask;
            }

#if NETFX_CORE
            protected override void Dispose(bool disposing)
            {
                _serializeToStreamTask.TrySetResult(true);
                base.Dispose(disposing);
            }
#else
            public override void Close()
            {
                // Note we don't call close on the inner stream as the stream will get closed when this
                // HttpContent instance is disposed.
                _serializeToStreamTask.TrySetResult(true);
            }
#endif
        }
    }
}
