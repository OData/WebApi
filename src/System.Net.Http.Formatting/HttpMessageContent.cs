// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace System.Net.Http
{
    /// <summary>
    /// Derived <see cref="HttpContent"/> class which can encapsulate an <see cref="HttpResponseMessage"/>
    /// or an <see cref="HttpRequestMessage"/> as an entity with media type "application/http".
    /// </summary>
    public class HttpMessageContent : HttpContent
    {
        private const string SP = " ";
        private const string CRLF = "\r\n";
        private const string CommaSeparator = ", ";

        private const int DefaultHeaderAllocation = 2 * 1024;

        private const string DefaultMediaType = "application/http";

        private const string MsgTypeParameter = "msgtype";
        private const string DefaultRequestMsgType = "request";
        private const string DefaultResponseMsgType = "response";

        private const string DefaultRequestMediaType = DefaultMediaType + "; " + MsgTypeParameter + "=" + DefaultRequestMsgType;
        private const string DefaultResponseMediaType = DefaultMediaType + "; " + MsgTypeParameter + "=" + DefaultResponseMsgType;
        private static readonly Task<HttpContent> _nullContentTask = TaskHelpers.FromResult<HttpContent>(null);
        private static readonly AsyncCallback _onWriteComplete = new AsyncCallback(OnWriteComplete);

        /// <summary>
        /// Set of header fields that only support single values such as Set-Cookie.
        /// </summary>
        private static readonly HashSet<string> _singleValueHeaderFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Set-Cookie",
            "X-Powered-By",
        };

        private bool _contentConsumed;
        private Lazy<Task<Stream>> _streamTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpMessageContent"/> class encapsulating an
        /// <see cref="HttpRequestMessage"/>.
        /// </summary>
        /// <param name="httpRequest">The <see cref="HttpResponseMessage"/> instance to encapsulate.</param>
        public HttpMessageContent(HttpRequestMessage httpRequest)
        {
            if (httpRequest == null)
            {
                throw new ArgumentNullException("httpRequest");
            }

            HttpRequestMessage = httpRequest;
            Headers.ContentType = new MediaTypeHeaderValue(DefaultMediaType);
            Headers.ContentType.Parameters.Add(new NameValueHeaderValue(MsgTypeParameter, DefaultRequestMsgType));

            InitializeStreamTask();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpMessageContent"/> class encapsulating an
        /// <see cref="HttpResponseMessage"/>.
        /// </summary>
        /// <param name="httpResponse">The <see cref="HttpResponseMessage"/> instance to encapsulate.</param>
        public HttpMessageContent(HttpResponseMessage httpResponse)
        {
            if (httpResponse == null)
            {
                throw new ArgumentNullException("httpResponse");
            }

            HttpResponseMessage = httpResponse;
            Headers.ContentType = new MediaTypeHeaderValue(DefaultMediaType);
            Headers.ContentType.Parameters.Add(new NameValueHeaderValue(MsgTypeParameter, DefaultResponseMsgType));

            InitializeStreamTask();
        }

        private HttpContent Content
        {
            get { return HttpRequestMessage != null ? HttpRequestMessage.Content : HttpResponseMessage.Content; }
        }

        /// <summary>
        /// Gets the HTTP request message.
        /// </summary>
        public HttpRequestMessage HttpRequestMessage { get; private set; }

        /// <summary>
        /// Gets the HTTP response message.
        /// </summary>
        public HttpResponseMessage HttpResponseMessage { get; private set; }

        private void InitializeStreamTask()
        {
            _streamTask = new Lazy<Task<Stream>>(() => Content == null ? null : Content.ReadAsStreamAsync());
        }

        /// <summary>
        /// Validates whether the content contains an HTTP Request or an HTTP Response.
        /// </summary>
        /// <param name="content">The content to validate.</param>
        /// <param name="isRequest">if set to <c>true</c> if the content is either an HTTP Request or an HTTP Response.</param>
        /// <param name="throwOnError">Indicates whether validation failure should result in an <see cref="Exception"/> or not.</param>
        /// <returns><c>true</c> if content is either an HTTP Request or an HTTP Response</returns>
        internal static bool ValidateHttpMessageContent(HttpContent content, bool isRequest, bool throwOnError)
        {
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }

            MediaTypeHeaderValue contentType = content.Headers.ContentType;
            if (contentType != null)
            {
                if (!contentType.MediaType.Equals(DefaultMediaType, StringComparison.OrdinalIgnoreCase))
                {
                    if (throwOnError)
                    {
                        throw new ArgumentException(
                            RS.Format(Properties.Resources.HttpMessageInvalidMediaType, FormattingUtilities.HttpContentType.Name,
                                      isRequest ? DefaultRequestMediaType : DefaultResponseMediaType),
                            "content");
                    }
                    else
                    {
                        return false;
                    }
                }

                foreach (NameValueHeaderValue parameter in contentType.Parameters)
                {
                    if (parameter.Name.Equals(MsgTypeParameter, StringComparison.OrdinalIgnoreCase))
                    {
                        string msgType = FormattingUtilities.UnquoteToken(parameter.Value);
                        if (!msgType.Equals(isRequest ? DefaultRequestMsgType : DefaultResponseMsgType, StringComparison.OrdinalIgnoreCase))
                        {
                            if (throwOnError)
                            {
                                throw new ArgumentException(
                                    RS.Format(Properties.Resources.HttpMessageInvalidMediaType, FormattingUtilities.HttpContentType.Name, isRequest ? DefaultRequestMediaType : DefaultResponseMediaType),
                                    "content");
                            }
                            else
                            {
                                return false;
                            }
                        }

                        return true;
                    }
                }
            }

            if (throwOnError)
            {
                throw new ArgumentException(
                    RS.Format(Properties.Resources.HttpMessageInvalidMediaType, FormattingUtilities.HttpContentType.Name, isRequest ? DefaultRequestMediaType : DefaultResponseMediaType),
                    "content");
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Asynchronously serializes the object's content to the given <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to which to write.</param>
        /// <param name="context">The associated <see cref="TransportContext"/>.</param>
        /// <returns>A <see cref="Task"/> instance that is asynchronously serializing the object's content.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is propagated.")]
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            Contract.Assert(stream != null);

            // Serialize header
            byte[] header = SerializeHeader();

            TaskCompletionSource<object> writeTask = new TaskCompletionSource<object>();
            try
            {
                // We don't use TaskFactory.FromAsync as it generates an FxCop CA908 error
                Tuple<HttpMessageContent, Stream, TaskCompletionSource<object>> state =
                    new Tuple<HttpMessageContent, Stream, TaskCompletionSource<object>>(this, stream, writeTask);
                IAsyncResult result = stream.BeginWrite(header, 0, header.Length, _onWriteComplete, state);
                if (result.CompletedSynchronously)
                {
                    WriteComplete(result, this, stream, writeTask);
                }
            }
            catch (Exception e)
            {
                writeTask.TrySetException(e);
            }

            return writeTask.Task;
        }

        /// <summary>
        /// Computes the length of the stream if possible.
        /// </summary>
        /// <param name="length">The computed length of the stream.</param>
        /// <returns><c>true</c> if the length has been computed; otherwise <c>false</c>.</returns>
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1108:BlockStatementsMustNotContainEmbeddedComments",
            Justification = "The code is more readable with such comments")]
        protected override bool TryComputeLength(out long length)
        {
            // We have four states we could be in:
            //   1. We have content, but the task is still running or finished without success
            //   2. We have content, the task has finished successfully, and the stream came back as a null or non-seekable
            //   3. We have content, the task has finished successfully, and the stream is seekable, so we know its length
            //   4. We don't have content (streamTask.Value == null)
            //
            // For #1 and #2, we return false.
            // For #3, we return true & the size of our headers + the content length
            // For #4, we return true & the size of our headers

            bool hasContent = _streamTask.Value != null;
            length = 0;

            // Cases #1, #2, #3
            if (hasContent)
            {
                Stream readStream;
                if (!_streamTask.Value.TryGetResult(out readStream) // Case #1
                    || readStream == null || !readStream.CanSeek) // Case #2
                {
                    length = -1;
                    return false;
                }

                length = readStream.Length; // Case #3
            }

            // We serialize header to a StringBuilder so that we can determine the length
            // following the pattern for HttpContent to try and determine the message length.
            // The perf overhead is no larger than for the other HttpContent implementations.
            byte[] header = SerializeHeader();
            length += header.Length;
            return true;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // If we ended up spinning up a task to get the content stream, make sure we observe any
                // exceptions so that the task finalizer doesn't tear down our app domain.
                if (_streamTask != null && _streamTask.IsValueCreated && _streamTask.Value != null)
                {
                    _streamTask.Value.Catch(info => info.Handled());
                    _streamTask = null;
                }

                if (HttpRequestMessage != null)
                {
                    HttpRequestMessage.Dispose();
                    HttpRequestMessage = null;
                }

                if (HttpResponseMessage != null)
                {
                    HttpResponseMessage.Dispose();
                    HttpResponseMessage = null;
                }
            }

            base.Dispose(disposing);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is propagated.")]
        private static void OnWriteComplete(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            Tuple<HttpMessageContent, Stream, TaskCompletionSource<object>> state =
                (Tuple<HttpMessageContent, Stream, TaskCompletionSource<object>>)result.AsyncState;
            Contract.Assert(state != null, "state cannot be null");
            try
            {
                WriteComplete(result, state.Item1, state.Item2, state.Item3);
            }
            catch (Exception e)
            {
                state.Item3.TrySetException(e);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is propagated.")]
        private static void WriteComplete(IAsyncResult result, HttpMessageContent thisPtr, Stream stream, TaskCompletionSource<object> writeTask)
        {
            Contract.Assert(result != null, "result cannot be null");
            Contract.Assert(thisPtr != null, "thisPtr cannot be null");
            Contract.Assert(stream != null, "stream cannot be null");
            Contract.Assert(writeTask != null, "writeTask cannot be null");

            try
            {
                stream.EndWrite(result);
            }
            catch (Exception e)
            {
                writeTask.TrySetException(e);
            }

            thisPtr.PrepareContentAsync().Then(content =>
            {
                if (content != null)
                {
                    content.CopyToAsync(stream)
                        .CopyResultToCompletionSource(writeTask, completionResult: null);
                }
                else
                {
                    writeTask.TrySetResult(null);
                }
            });
        }

        /// <summary>
        /// Serializes the HTTP request line.
        /// </summary>
        /// <param name="message">Where to write the request line.</param>
        /// <param name="httpRequest">The HTTP request.</param>
        private static void SerializeRequestLine(StringBuilder message, HttpRequestMessage httpRequest)
        {
            Contract.Assert(message != null, "message cannot be null");
            message.Append(httpRequest.Method + SP);
            message.Append(httpRequest.RequestUri.PathAndQuery + SP);
            message.Append(FormattingUtilities.HttpVersionToken + "/" + (httpRequest.Version != null ? httpRequest.Version.ToString(2) : "1.1") + CRLF);

            // Only insert host header if not already present.
            if (httpRequest.Headers.Host == null)
            {
                message.Append(FormattingUtilities.HttpHostHeader + ":" + SP + httpRequest.RequestUri.Authority + CRLF);
            }
        }

        /// <summary>
        /// Serializes the HTTP status line.
        /// </summary>
        /// <param name="message">Where to write the status line.</param>
        /// <param name="httpResponse">The HTTP response.</param>
        private static void SerializeStatusLine(StringBuilder message, HttpResponseMessage httpResponse)
        {
            Contract.Assert(message != null, "message cannot be null");
            message.Append(FormattingUtilities.HttpVersionToken + "/" + (httpResponse.Version != null ? httpResponse.Version.ToString(2) : "1.1") + SP);
            message.Append((int)httpResponse.StatusCode + SP);
            message.Append(httpResponse.ReasonPhrase + CRLF);
        }

        /// <summary>
        /// Serializes the header fields.
        /// </summary>
        /// <param name="message">Where to write the status line.</param>
        /// <param name="headers">The headers to write.</param>
        private static void SerializeHeaderFields(StringBuilder message, HttpHeaders headers)
        {
            Contract.Assert(message != null, "message cannot be null");
            if (headers != null)
            {
                foreach (KeyValuePair<string, IEnumerable<string>> header in headers)
                {
                    if (_singleValueHeaderFields.Contains(header.Key))
                    {
                        foreach (string value in header.Value)
                        {
                            message.Append(header.Key + ":" + SP + value + CRLF);
                        }
                    }
                    else
                    {
                        message.Append(header.Key + ":" + SP + String.Join(CommaSeparator, header.Value) + CRLF);
                    }
                }
            }
        }

        private Task<HttpContent> PrepareContentAsync()
        {
            if (Content == null)
            {
                return _nullContentTask;
            }

            return _streamTask.Value.Then(readStream =>
            {
                // If the content needs to be written to a target stream a 2nd time, then the stream must support
                // seeking (e.g. a FileStream), otherwise the stream can't be copied a second time to a target 
                // stream (e.g. a NetworkStream).
                if (_contentConsumed)
                {
                    if (readStream != null && readStream.CanRead)
                    {
                        readStream.Position = 0;
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            RS.Format(Properties.Resources.HttpMessageContentAlreadyRead,
                                      FormattingUtilities.HttpContentType.Name,
                                      HttpRequestMessage != null
                                          ? FormattingUtilities.HttpRequestMessageType.Name
                                          : FormattingUtilities.HttpResponseMessageType.Name));
                    }

                    _contentConsumed = true;
                }

                return Content;
            });
        }

        private byte[] SerializeHeader()
        {
            StringBuilder message = new StringBuilder(DefaultHeaderAllocation);
            HttpHeaders headers = null;
            HttpContent content = null;
            if (HttpRequestMessage != null)
            {
                SerializeRequestLine(message, HttpRequestMessage);
                headers = HttpRequestMessage.Headers;
                content = HttpRequestMessage.Content;
            }
            else
            {
                SerializeStatusLine(message, HttpResponseMessage);
                headers = HttpResponseMessage.Headers;
                content = HttpResponseMessage.Content;
            }

            SerializeHeaderFields(message, headers);
            if (content != null)
            {
                SerializeHeaderFields(message, content.Headers);
            }

            message.Append(CRLF);
            return Encoding.UTF8.GetBytes(message.ToString());
        }
    }
}
