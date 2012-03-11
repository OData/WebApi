using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel.Channels;
using System.Web.Http.Common;
using System.Web.Http.SelfHost.Properties;

namespace System.Web.Http.SelfHost.Channels
{
    internal class HttpMessageEncodingRequestContext : RequestContext
    {
        private const string ContentLengthHeader = "Content-Length";
        private const string DefaultReasonPhrase = "OK";

        // TODO: Remove this list of content-type headers once the NCL team publicly exposes
        //       this list. Opened bug #50459 in DevDiv2 TFS on the NCL team.
        //       Opened #189321 in CSDmain to track  [randallt] 
        private static readonly HashSet<string> _httpContentHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Allow", "Content-Encoding", "Content-Language", "Content-Location", "Content-MD5",
            "Content-Range", "Expires", "Last-Modified", "Content-Type", ContentLengthHeader
        };

        private RequestContext _innerContext;
        private Message _configuredRequestMessage;
        private bool _isRequestConfigured;
        private object _requestConfigurationLock;

        public HttpMessageEncodingRequestContext(RequestContext innerContext)
        {
            Contract.Assert(innerContext != null, "The 'innerContext' parameter should not be null.");
            _innerContext = innerContext;
            _requestConfigurationLock = new object();
        }

        public override Message RequestMessage
        {
            get
            {
                if (!_isRequestConfigured)
                {
                    lock (_requestConfigurationLock)
                    {
                        if (!_isRequestConfigured)
                        {
                            Message innerMessage = _innerContext.RequestMessage;
                            _configuredRequestMessage = ConfigureRequestMessage(innerMessage);
                            _isRequestConfigured = true;
                        }
                    }
                }

                return _configuredRequestMessage;
            }
        }

        public override void Abort()
        {
            Cleanup();
            _innerContext.Abort();
        }

        public override IAsyncResult BeginReply(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            message = ConfigureResponseMessage(message);
            return _innerContext.BeginReply(message, timeout, callback, state);
        }

        public override IAsyncResult BeginReply(Message message, AsyncCallback callback, object state)
        {
            message = ConfigureResponseMessage(message);
            return _innerContext.BeginReply(message, callback, state);
        }

        public override void Close(TimeSpan timeout)
        {
            Cleanup();
            _innerContext.Close(timeout);
        }

        public override void Close()
        {
            Cleanup();
            _innerContext.Close();
        }

        public override void EndReply(IAsyncResult result)
        {
            _innerContext.EndReply(result);
        }

        public override void Reply(Message message, TimeSpan timeout)
        {
            message = ConfigureResponseMessage(message);
            _innerContext.Reply(message, timeout);
        }

        public override void Reply(Message message)
        {
            ConfigureResponseMessage(message);
            _innerContext.Reply(message);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Cleanup exceptions are ignored as they are not fatal to the host.")]
        private void Cleanup()
        {
            if (_configuredRequestMessage != null)
            {
                try
                {
                    _configuredRequestMessage.Close();
                }
                catch
                {
                }
            }
        }

        private static void AddHeaderToHttpRequestMessageAndHandleExceptions(HttpRequestMessage httpRequestMessage, string headerName, string headerValue)
        {
            try
            {
                AddHeaderToHttpRequestMessage(httpRequestMessage, headerName, headerValue);
            }
            catch (FormatException)
            {
            }
            catch (InvalidOperationException)
            {
            }
        }

        private static void AddHeaderToHttpRequestMessage(HttpRequestMessage httpRequestMessage, string headerName, string headerValue)
        {
            if (_httpContentHeaders.Contains(headerName))
            {
                // Only set the content-length header if it is not already set
                if (String.Equals(headerName, ContentLengthHeader, StringComparison.Ordinal) &&
                    httpRequestMessage.Content.Headers.ContentLength != null)
                {
                    return;
                }

                httpRequestMessage.Content.Headers.Add(headerName, headerValue);
            }
            else
            {
                httpRequestMessage.Headers.Add(headerName, headerValue);
            }
        }

        private static void CopyHeadersToNameValueCollection(HttpHeaders headers, NameValueCollection nameValueCollection)
        {
            foreach (KeyValuePair<string, IEnumerable<string>> header in headers)
            {
                foreach (string value in header.Value)
                {
                    nameValueCollection.Add(header.Key, value);
                }
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "disposed later.")]
        private static Message ConfigureRequestMessage(Message message)
        {
            if (message == null)
            {
                return null;
            }

            HttpRequestMessageProperty requestProperty;
            if (!message.Properties.TryGetValue(HttpRequestMessageProperty.Name, out requestProperty))
            {
                throw Error.InvalidOperation(
                    SRResources.RequestMissingHttpRequestMessageProperty,
                    HttpRequestMessageProperty.Name,
                    typeof(HttpRequestMessageProperty).Name);
            }

            Uri uri = message.Headers.To;
            if (uri == null)
            {
                throw Error.InvalidOperation(SRResources.RequestMissingToHeader);
            }

            HttpRequestMessage httpRequestMessage = message.ToHttpRequestMessage();
            if (httpRequestMessage == null)
            {
                if (!message.IsEmpty)
                {
                    throw Error.InvalidOperation(SRResources.NonHttpMessageMustBeEmpty, HttpMessageExtensions.ToHttpRequestMessageMethodName, typeof(HttpMessage).Name);
                }

                httpRequestMessage = new HttpRequestMessage();
                Message oldMessage = message;
                message = httpRequestMessage.ToMessage();
                message.Properties.CopyProperties(oldMessage.Properties);
                oldMessage.Close();
            }
            else
            {
                // Clear headers but not properties.
                message.Headers.Clear();
            }

            // Copy message properties to HttpRequestMessage. While it does have the
            // risk of allowing properties to get out of sync they in virtually all cases are
            // read-only so the risk is low. The downside to not doing it is that it isn't
            // possible to access anything from HttpRequestMessage (or OperationContent.Current)
            // which is worse.
            foreach (KeyValuePair<string, object> kv in message.Properties)
            {
                httpRequestMessage.Properties.Add(kv.Key, kv.Value);
            }

            if (httpRequestMessage.Content == null)
            {
                httpRequestMessage.Content = new ByteArrayContent(new byte[0]);
            }
            else
            {
                httpRequestMessage.Content.Headers.Clear();
            }

            message.Headers.To = uri;

            httpRequestMessage.RequestUri = uri;
            httpRequestMessage.Method = HttpMethodHelper.GetHttpMethod(requestProperty.Method);

            foreach (var headerName in requestProperty.Headers.AllKeys)
            {
                AddHeaderToHttpRequestMessageAndHandleExceptions(
                    httpRequestMessage,
                    headerName,
                    requestProperty.Headers[headerName]);
            }

            return message;
        }

        private static Message ConfigureResponseMessage(Message message)
        {
            if (message == null)
            {
                return null;
            }

            HttpResponseMessageProperty responseProperty = new HttpResponseMessageProperty();

            HttpResponseMessage httpResponseMessage = message.ToHttpResponseMessage();
            if (httpResponseMessage == null)
            {
                responseProperty.StatusCode = HttpStatusCode.InternalServerError;
                responseProperty.SuppressEntityBody = true;
            }
            else
            {
                responseProperty.StatusCode = httpResponseMessage.StatusCode;
                if (httpResponseMessage.ReasonPhrase != null &&
                    httpResponseMessage.ReasonPhrase != DefaultReasonPhrase)
                {
                    responseProperty.StatusDescription = httpResponseMessage.ReasonPhrase;
                }

                CopyHeadersToNameValueCollection(httpResponseMessage.Headers, responseProperty.Headers);
                HttpContent content = httpResponseMessage.Content;
                if (content != null)
                {
                    CopyHeadersToNameValueCollection(httpResponseMessage.Content.Headers, responseProperty.Headers);
                }
                else
                {
                    responseProperty.SuppressEntityBody = true;
                }
            }

            message.Properties.Clear();
            message.Headers.Clear();

            message.Properties.Add(HttpResponseMessageProperty.Name, responseProperty);

            return message;
        }
    }
}
