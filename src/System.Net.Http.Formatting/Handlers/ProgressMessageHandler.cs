// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Http.Handlers
{
    /// <summary>
    /// The <see cref="ProgressMessageHandler"/> provides a mechanism for getting progress event notifications
    /// when sending and receiving data in connection with exchanging HTTP requests and responses.
    /// Register event handlers for the events <see cref="HttpSendProgress"/> and <see cref="HttpReceiveProgress"/>
    /// to see events for data being sent and received.
    /// </summary>
    public class ProgressMessageHandler : DelegatingHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressMessageHandler"/> class.
        /// </summary>
        public ProgressMessageHandler()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressMessageHandler"/> class.
        /// </summary>
        /// <param name="innerHandler">The inner handler to which this handler submits requests.</param>
        public ProgressMessageHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        /// <summary>
        /// Occurs every time the client sending data is making progress.
        /// </summary>
        public event EventHandler<HttpProgressEventArgs> HttpSendProgress;

        /// <summary>
        /// Occurs every time the client receiving data is making progress.
        /// </summary>
        public event EventHandler<HttpProgressEventArgs> HttpReceiveProgress;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            AddRequestProgress(request);
            return base.SendAsync(request, cancellationToken).Then(response => AddResponseProgress(request, response));
        }

        /// <summary>
        /// Raises the <see cref="HttpSendProgress"/> event.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="e">The <see cref="HttpProgressEventArgs"/> instance containing the event data.</param>
        protected internal virtual void OnHttpRequestProgress(HttpRequestMessage request, HttpProgressEventArgs e)
        {
            if (HttpSendProgress != null)
            {
                HttpSendProgress(request, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="HttpReceiveProgress"/> event.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="e">The <see cref="HttpProgressEventArgs"/> instance containing the event data.</param>
        protected internal virtual void OnHttpResponseProgress(HttpRequestMessage request, HttpProgressEventArgs e)
        {
            if (HttpReceiveProgress != null)
            {
                HttpReceiveProgress(request, e);
            }
        }

        private void AddRequestProgress(HttpRequestMessage request)
        {
            if (HttpSendProgress != null && request != null && request.Content != null)
            {
                HttpContent progressContent = new ProgressContent(request.Content, this, request);
                request.Content = progressContent;
            }
        }

        private Task<HttpResponseMessage> AddResponseProgress(HttpRequestMessage request, HttpResponseMessage response)
        {
            Task<HttpResponseMessage> responseTask;
            if (HttpReceiveProgress != null && response != null && response.Content != null)
            {
                responseTask = response.Content.ReadAsStreamAsync().Then(
                    stream =>
                    {
                        ProgressStream progressStream = new ProgressStream(stream, this, request, response);
                        HttpContent progressContent = new StreamContent(progressStream);
                        response.Content.Headers.CopyTo(progressContent.Headers);
                        response.Content = progressContent;
                        return response;
                    }, runSynchronously: true);
            }
            else
            {
                responseTask = TaskHelpers.FromResult(response);
            }

            return responseTask;
        }
    }
}
