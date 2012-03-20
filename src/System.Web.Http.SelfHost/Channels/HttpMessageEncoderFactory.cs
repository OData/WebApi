using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net.Http;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Web.Http.SelfHost.Properties;
using System.Web.Http.SelfHost.ServiceModel.Channels;

namespace System.Web.Http.SelfHost.Channels
{
    internal class HttpMessageEncoderFactory : MessageEncoderFactory
    {
        private HttpMessageEncoder _encoder;

        public HttpMessageEncoderFactory()
        {
            _encoder = new HttpMessageEncoder();
        }

        public override MessageEncoder Encoder
        {
            get { return _encoder; }
        }

        public override MessageVersion MessageVersion
        {
            get { return MessageVersion.None; }
        }

        public override MessageEncoder CreateSessionEncoder()
        {
            throw Error.NotSupported(SRResources.HttpMessageEncoderFactoryDoesNotSupportSessionEncoder, typeof(HttpMessageEncoderFactory).Name);
        }

        private class HttpMessageEncoder : MessageEncoder
        {
            private const string ContentTypeHeaderName = "Content-Type";
            private const string MaxSentMessageSizeExceededResourceStringName = "MaxSentMessageSizeExceeded";
            private static readonly string _httpBindingClassName = typeof(HttpBinding).FullName;
            private static readonly string _httpResponseMessageClassName = typeof(HttpResponseMessage).FullName;

            public override string ContentType
            {
                get { return String.Empty; }
            }

            public override string MediaType
            {
                get { return String.Empty; }
            }

            public override MessageVersion MessageVersion
            {
                get { return MessageVersion.None; }
            }

            public override bool IsContentTypeSupported(string contentType)
            {
                if (contentType == null)
                {
                    throw Error.ArgumentNull("contentType");
                }

                return true;
            }

            [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "disposed later.")]
            public override Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager, string contentType)
            {
                if (bufferManager == null)
                {
                    throw Error.ArgumentNull("bufferManager");
                }

                HttpRequestMessage request = new HttpRequestMessage();
                request.Content = new ByteArrayBufferManagerContent(bufferManager, buffer.Array, buffer.Offset, buffer.Count);
                if (!String.IsNullOrEmpty(contentType))
                {
                    request.Content.Headers.Add(ContentTypeHeaderName, contentType);
                }

                Message message = request.ToMessage();
                message.Properties.Encoder = this;

                return message;
            }

            [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "disposed later.")]
            public override Message ReadMessage(Stream stream, int maxSizeOfHeaders, string contentType)
            {
                if (stream == null)
                {
                    throw Error.ArgumentNull("stream");
                }

                HttpRequestMessage request = new HttpRequestMessage();
                request.Content = new StreamContent(stream);
                if (!String.IsNullOrEmpty(contentType))
                {
                    request.Content.Headers.Add(ContentTypeHeaderName, contentType);
                }

                Message message = request.ToMessage();
                message.Properties.Encoder = this;

                return message;
            }

            public override ArraySegment<byte> WriteMessage(Message message, int maxMessageSize, BufferManager bufferManager, int messageOffset)
            {
                if (message == null)
                {
                    throw Error.ArgumentNull("message");
                }

                if (bufferManager == null)
                {
                    throw Error.ArgumentNull("bufferManager");
                }

                if (maxMessageSize < 0)
                {
                    throw Error.ArgumentOutOfRange("maxMessageSize", maxMessageSize, SRResources.NonnegativeNumberRequired);
                }

                if (messageOffset < 0)
                {
                    throw Error.ArgumentOutOfRange("messageOffset", messageOffset, SRResources.NonnegativeNumberRequired);
                }

                if (messageOffset > maxMessageSize)
                {
                    throw Error.Argument(String.Empty, SRResources.ParameterMustBeLessThanOrEqualSecondParameter, "messageOffset", "maxMessageSize");
                }

                using (BufferManagerOutputStream stream = new BufferManagerOutputStream(MaxSentMessageSizeExceededResourceStringName, 0, maxMessageSize, bufferManager))
                {
                    int num;
                    stream.Skip(messageOffset);
                    WriteMessage(message, stream);
                    ArraySegment<byte> messageData = new ArraySegment<byte>(stream.ToArray(out num), 0, num - messageOffset);

                    return messageData;
                }
            }

            [SuppressMessage("Microsoft.WebAPI", "CR4001:DoNotCallProblematicMethodsOnTask", Justification = "The WriteMessage() API is synchronous, and Wait() won't deadlock in self-host.")]
            public override void WriteMessage(Message message, Stream stream)
            {
                if (message == null)
                {
                    throw Error.ArgumentNull("message");
                }

                if (stream == null)
                {
                    throw Error.ArgumentNull("stream");
                }

                ThrowIfMismatchedMessageVersion(message);

                message.Properties.Encoder = this;

                HttpResponseMessage response = GetHttpResponseMessageOrThrow(message);

                if (response.Content != null)
                {
                    response.Content.CopyToAsync(stream).Wait();
                }
            }

            internal void ThrowIfMismatchedMessageVersion(Message message)
            {
                if (message.Version != MessageVersion)
                {
                    throw new ProtocolException(Error.Format(SRResources.EncoderMessageVersionMismatch, message.Version, MessageVersion));
                }
            }

            private static HttpResponseMessage GetHttpResponseMessageOrThrow(Message message)
            {
                HttpResponseMessage response = message.ToHttpResponseMessage();
                if (response == null)
                {
                    throw Error.InvalidOperation(
                        SRResources.MessageInvalidForHttpMessageEncoder,
                        _httpBindingClassName,
                        HttpMessageExtensions.ToMessageMethodName,
                        _httpResponseMessageClassName);
                }

                return response;
            }

            private class ByteArrayBufferManagerContent : ByteArrayContent
            {
                private bool _disposed;
                private BufferManager _bufferManager;
                private byte[] _content;
                private object _disposingLock;

                public ByteArrayBufferManagerContent(BufferManager bufferManager, byte[] content, int offset, int count)
                    : base(content, offset, count)
                {
                    Contract.Assert(bufferManager != null, "The 'bufferManager' parameter should never be null.");

                    _bufferManager = bufferManager;
                    _content = content;
                    _disposingLock = new object();
                }

                protected override void Dispose(bool disposing)
                {
                    try
                    {
                        if (disposing && !_disposed)
                        {
                            lock (_disposingLock)
                            {
                                if (!_disposed)
                                {
                                    _disposed = true;
                                    _bufferManager.ReturnBuffer(_content);
                                    _content = null;
                                }
                            }
                        }
                    }
                    finally
                    {
                        base.Dispose(disposing);
                    }
                }
            }
        }
    }
}
