// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.SelfHost.Channels;
using System.Web.Http.SelfHost.Properties;
using System.Web.Http.SelfHost.ServiceModel.Channels;

namespace System.Web.Http.SelfHost
{
    /// <summary>
    /// Implementation of an <see cref="HttpServer"/> which listens directly to HTTP.
    /// </summary>
    public sealed class HttpSelfHostServer : HttpServer
    {
        private static readonly AsyncCallback _onOpenListenerComplete = new AsyncCallback(OnOpenListenerComplete);
        private static readonly AsyncCallback _onAcceptChannelComplete = new AsyncCallback(OnAcceptChannelComplete);
        private static readonly AsyncCallback _onOpenChannelComplete = new AsyncCallback(OnOpenChannelComplete);
        private static readonly AsyncCallback _onReceiveRequestContextComplete = new AsyncCallback(OnReceiveRequestContextComplete);
        private static readonly AsyncCallback _onReplyComplete = new AsyncCallback(OnReplyComplete);

        private static readonly AsyncCallback _onCloseListenerComplete = new AsyncCallback(OnCloseListenerComplete);
        private static readonly AsyncCallback _onCloseChannelComplete = new AsyncCallback(OnCloseChannelComplete);

        private static readonly TimeSpan _acceptTimeout = TimeSpan.MaxValue;
        private static readonly TimeSpan _receiveTimeout = TimeSpan.MaxValue;

        private ConcurrentBag<IReplyChannel> _channels = new ConcurrentBag<IReplyChannel>();
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private bool _disposed;
        private HttpSelfHostConfiguration _configuration;
        private IChannelListener<IReplyChannel> _listener;
        private TaskCompletionSource<bool> _openTaskCompletionSource;
        private TaskCompletionSource<bool> _closeTaskCompletionSource;

        // State: 0 = new, 1 = open, 2 = closed
        private int _state;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpSelfHostServer"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public HttpSelfHostServer(HttpSelfHostConfiguration configuration)
            : base(configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            _configuration = configuration;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpSelfHostServer"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="dispatcher">The dispatcher.</param>
        public HttpSelfHostServer(HttpSelfHostConfiguration configuration, HttpMessageHandler dispatcher)
            : base(configuration, dispatcher)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            _configuration = configuration;
        }

        /// <summary>
        /// Opens the current <see cref="HttpServer"/> instance.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous <see cref="HttpServer"/> open operation. Once this task completes successfully the server is running.</returns>
        public Task OpenAsync()
        {
            if (Interlocked.CompareExchange(ref _state, 1, 0) == 1)
            {
                throw Error.InvalidOperation(SRResources.HttpServerAlreadyRunning, typeof(HttpSelfHostServer).Name);
            }

            _openTaskCompletionSource = new TaskCompletionSource<bool>();
            BeginOpenListener(this);
            return _openTaskCompletionSource.Task;
        }

        /// <summary>
        /// Closes the current <see cref="HttpServer"/> instance.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous <see cref="HttpServer"/> close operation.</returns>
        public Task CloseAsync()
        {
            if (Interlocked.CompareExchange(ref _state, 2, 1) != 1)
            {
                return TaskHelpers.Completed();
            }

            _closeTaskCompletionSource = new TaskCompletionSource<bool>();

            // Cancel requests currently being processed
            _cancellationTokenSource.Cancel();

            BeginCloseListener(this);
            return _closeTaskCompletionSource.Task;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged SRResources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
                if (_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = null;
                }
            }

            base.Dispose(disposing);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "ReplyContext and HttpResponseMessage are disposed later.")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We never want to fail here so we have to catch all exceptions.")]
        private static void ProcessRequestContext(ChannelContext channelContext, System.ServiceModel.Channels.RequestContext requestContext)
        {
            Contract.Assert(channelContext != null);
            Contract.Assert(requestContext != null);

            // Get the HTTP request from the WCF Message
            HttpRequestMessage request = requestContext.RequestMessage.ToHttpRequestMessage();
            if (request == null)
            {
                throw Error.InvalidOperation(SRResources.HttpMessageHandlerInvalidMessage, requestContext.RequestMessage.GetType());
            }

            // create principal information and add it the request for the windows auth case
            SecurityMessageProperty property = request.GetSecurityMessageProperty();
            if (property != null)
            {
                ServiceSecurityContext context = property.ServiceSecurityContext;
                if (context != null && context.PrimaryIdentity != null)
                {
                    WindowsIdentity windowsIdentity = context.PrimaryIdentity as WindowsIdentity;

                    if (windowsIdentity != null)
                    {
                        Thread.CurrentPrincipal = new WindowsPrincipal(windowsIdentity);
                    }
                }
            }

            // Submit request up the stack
            try
            {
                HttpResponseMessage responseMessage = null;

                channelContext.Server.SendAsync(request, channelContext.Server._cancellationTokenSource.Token)
                    .Then(response =>
                    {
                        responseMessage = response ?? request.CreateResponse(HttpStatusCode.OK);
                    })
                    .Catch(info =>
                    {
                        // REVIEW: Shouldn't the response contain the exception so it can be serialized?
                        responseMessage = request.CreateResponse(HttpStatusCode.InternalServerError);
                        return info.Handled();
                    })
                    .Finally(() =>
                    {
                        if (responseMessage == null) // No Then or Catch, must've been canceled
                        {
                            responseMessage = request.CreateResponse(HttpStatusCode.ServiceUnavailable);
                        }

                        Message reply = responseMessage.ToMessage();
                        BeginReply(new ReplyContext(channelContext, requestContext, reply));
                    });
            }
            catch
            {
                // REVIEW: Shouldn't the response contain the exception so it can be serialized?
                HttpResponseMessage response = request.CreateResponse(HttpStatusCode.InternalServerError);
                Message reply = response.ToMessage();
                BeginReply(new ReplyContext(channelContext, requestContext, reply));
            }
        }

        private static void CancelTask(TaskCompletionSource<bool> taskCompletionSource)
        {
            Contract.Assert(taskCompletionSource != null);
            taskCompletionSource.TrySetCanceled();
        }

        private static void FaultTask(TaskCompletionSource<bool> taskCompletionSource, Exception exception)
        {
            Contract.Assert(taskCompletionSource != null);
            taskCompletionSource.TrySetException(exception);
        }

        private static void CompleteTask(TaskCompletionSource<bool> taskCompletionSource)
        {
            Contract.Assert(taskCompletionSource != null);
            taskCompletionSource.TrySetResult(true);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We never want to fail here so we have to catch all exceptions.")]
        private static void BeginOpenListener(HttpSelfHostServer server)
        {
            Contract.Assert(server != null);

            try
            {
                // Create WCF HTTP transport channel
                HttpBinding binding = new HttpBinding();

                // Get it configured
                BindingParameterCollection bindingParameters = server._configuration.ConfigureBinding(binding);
                if (bindingParameters == null)
                {
                    bindingParameters = new BindingParameterCollection();
                }

                // Build channel listener
                server._listener = binding.BuildChannelListener<IReplyChannel>(server._configuration.BaseAddress, bindingParameters);
                if (server._listener == null)
                {
                    throw Error.InvalidOperation(SRResources.InvalidChannelListener, typeof(IChannelListener).Name, typeof(IReplyChannel).Name);
                }

                IAsyncResult result = server._listener.BeginOpen(_onOpenListenerComplete, server);
                if (result.CompletedSynchronously)
                {
                    OpenListenerComplete(result);
                }
            }
            catch (Exception e)
            {
                FaultTask(server._openTaskCompletionSource, e);
            }
        }

        private static void OnOpenListenerComplete(IAsyncResult result)
        {
            Contract.Assert(result != null);

            if (result.CompletedSynchronously)
            {
                return;
            }

            OpenListenerComplete(result);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We never want to fail here so we have to catch all exceptions.")]
        private static void OpenListenerComplete(IAsyncResult result)
        {
            HttpSelfHostServer server = (HttpSelfHostServer)result.AsyncState;
            Contract.Assert(server != null);
            Contract.Assert(server._listener != null);

            try
            {
                server._listener.EndOpen(result);

                // Start accepting channel
                BeginAcceptChannel(server);
            }
            catch (Exception e)
            {
                FaultTask(server._openTaskCompletionSource, e);
            }
        }

        private static void BeginAcceptChannel(HttpSelfHostServer server)
        {
            Contract.Assert(server != null);
            Contract.Assert(server._listener != null);

            IAsyncResult result = BeginTryAcceptChannel(server, _onAcceptChannelComplete);
            if (result.CompletedSynchronously)
            {
                AcceptChannelComplete(result);
            }
        }

        private static void OnAcceptChannelComplete(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            AcceptChannelComplete(result);
        }

        private static void AcceptChannelComplete(IAsyncResult result)
        {
            Contract.Assert(result != null);

            HttpSelfHostServer server = (HttpSelfHostServer)result.AsyncState;
            Contract.Assert(server != null, "host cannot be null");
            Contract.Assert(server._listener != null, "host.listener cannot be null");

            IReplyChannel channel;
            if (EndTryAcceptChannel(result, out channel))
            {
                // If we didn't get a channel then we stop accepting new channels
                if (channel != null)
                {
                    server._channels.Add(channel);
                    BeginOpenChannel(new ChannelContext(server, channel));
                }
                else
                {
                    CancelTask(server._openTaskCompletionSource);
                }
            }
            else
            {
                // Start accepting next channel
                BeginAcceptChannel(server);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We never want to fail here so we have to catch all exceptions.")]
        private static IAsyncResult BeginTryAcceptChannel(HttpSelfHostServer server, AsyncCallback callback)
        {
            Contract.Assert(server != null);
            Contract.Assert(server._listener != null);
            Contract.Assert(callback != null);

            try
            {
                return server._listener.BeginAcceptChannel(_acceptTimeout, callback, server);
            }
            catch (CommunicationObjectAbortedException)
            {
                return new CompletedAsyncResult<bool>(true, callback, server);
            }
            catch (CommunicationObjectFaultedException)
            {
                return new CompletedAsyncResult<bool>(true, callback, server);
            }
            catch (TimeoutException)
            {
                return new CompletedAsyncResult<bool>(false, callback, server);
            }
            catch (CommunicationException)
            {
                return new CompletedAsyncResult<bool>(false, callback, server);
            }
            catch
            {
                return new CompletedAsyncResult<bool>(false, callback, server);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We never want to fail here so we have to catch all exceptions.")]
        private static bool EndTryAcceptChannel(IAsyncResult result, out IReplyChannel channel)
        {
            Contract.Assert(result != null);

            CompletedAsyncResult<bool> handlerResult = result as CompletedAsyncResult<bool>;
            if (handlerResult != null)
            {
                channel = null;
                return CompletedAsyncResult<bool>.End(handlerResult);
            }
            else
            {
                try
                {
                    HttpSelfHostServer server = (HttpSelfHostServer)result.AsyncState;
                    Contract.Assert(server != null);
                    Contract.Assert(server._listener != null);
                    channel = server._listener.EndAcceptChannel(result);
                    return true;
                }
                catch (CommunicationObjectAbortedException)
                {
                    channel = null;
                    return true;
                }
                catch (CommunicationObjectFaultedException)
                {
                    channel = null;
                    return true;
                }
                catch (TimeoutException)
                {
                    channel = null;
                    return false;
                }
                catch (CommunicationException)
                {
                    channel = null;
                    return false;
                }
                catch
                {
                    channel = null;
                    return false;
                }
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We never want to fail here so we have to catch all exceptions.")]
        private static void BeginOpenChannel(ChannelContext channelContext)
        {
            Contract.Assert(channelContext != null);
            Contract.Assert(channelContext.Channel != null);

            try
            {
                IAsyncResult result = channelContext.Channel.BeginOpen(_onOpenChannelComplete, channelContext);
                if (result.CompletedSynchronously)
                {
                    OpenChannelComplete(result);
                }
            }
            catch (Exception e)
            {
                FaultTask(channelContext.Server._openTaskCompletionSource, e);
            }
        }

        private static void OnOpenChannelComplete(IAsyncResult result)
        {
            Contract.Assert(result != null);

            if (result.CompletedSynchronously)
            {
                return;
            }

            OpenChannelComplete(result);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We never want to fail here so we have to catch all exceptions.")]
        private static void OpenChannelComplete(IAsyncResult result)
        {
            ChannelContext channelContext = (ChannelContext)result.AsyncState;
            Contract.Assert(channelContext != null);
            Contract.Assert(channelContext.Channel != null);

            try
            {
                channelContext.Channel.EndOpen(result);

                // The channel is open and we can complete the open task
                CompleteTask(channelContext.Server._openTaskCompletionSource);

                // Start pumping messages
                for (int index = 0; index < channelContext.Server._configuration.MaxConcurrentRequests; index++)
                {
                    BeginReceiveRequestContext(channelContext);
                }

                // Start accepting next channel
                BeginAcceptChannel(channelContext.Server);
            }
            catch (Exception e)
            {
                FaultTask(channelContext.Server._openTaskCompletionSource, e);
            }
        }

        private static void BeginReceiveRequestContext(ChannelContext context)
        {
            Contract.Assert(context != null);

            if (context.Channel.State != CommunicationState.Opened)
            {
                return;
            }

            IAsyncResult result = BeginTryReceiveRequestContext(context, _onReceiveRequestContextComplete);
            if (result.CompletedSynchronously)
            {
                ReceiveRequestContextComplete(result);
            }
        }

        private static void OnReceiveRequestContextComplete(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            ReceiveRequestContextComplete(result);
        }

        private static void ReceiveRequestContextComplete(IAsyncResult result)
        {
            Contract.Assert(result != null);

            ChannelContext channelContext = (ChannelContext)result.AsyncState;
            Contract.Assert(channelContext != null);

            System.ServiceModel.Channels.RequestContext requestContext;
            if (EndTryReceiveRequestContext(result, out requestContext))
            {
                if (requestContext != null)
                {
                    ProcessRequestContext(channelContext, requestContext);
                }
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We never want to fail here so we have to catch all exceptions.")]
        private static IAsyncResult BeginTryReceiveRequestContext(ChannelContext channelContext, AsyncCallback callback)
        {
            Contract.Assert(channelContext != null);
            Contract.Assert(callback != null);

            try
            {
                return channelContext.Channel.BeginTryReceiveRequest(_receiveTimeout, callback, channelContext);
            }
            catch (CommunicationObjectAbortedException)
            {
                return new CompletedAsyncResult<bool>(true, callback, channelContext);
            }
            catch (CommunicationObjectFaultedException)
            {
                return new CompletedAsyncResult<bool>(true, callback, channelContext);
            }
            catch (CommunicationException)
            {
                return new CompletedAsyncResult<bool>(false, callback, channelContext);
            }
            catch (TimeoutException)
            {
                return new CompletedAsyncResult<bool>(false, callback, channelContext);
            }
            catch
            {
                return new CompletedAsyncResult<bool>(false, callback, channelContext);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We never want to fail here so we have to catch all exceptions.")]
        private static bool EndTryReceiveRequestContext(IAsyncResult result, out System.ServiceModel.Channels.RequestContext requestContext)
        {
            Contract.Assert(result != null);

            CompletedAsyncResult<bool> handlerResult = result as CompletedAsyncResult<bool>;
            if (handlerResult != null)
            {
                requestContext = null;
                return CompletedAsyncResult<bool>.End(handlerResult);
            }
            else
            {
                try
                {
                    ChannelContext channelContext = (ChannelContext)result.AsyncState;
                    Contract.Assert(channelContext != null, "context cannot be null");
                    return channelContext.Channel.EndTryReceiveRequest(result, out requestContext);
                }
                catch (CommunicationObjectAbortedException)
                {
                    requestContext = null;
                    return true;
                }
                catch (CommunicationObjectFaultedException)
                {
                    requestContext = null;
                    return true;
                }
                catch (CommunicationException)
                {
                    requestContext = null;
                    return false;
                }
                catch (TimeoutException)
                {
                    requestContext = null;
                    return false;
                }
                catch
                {
                    requestContext = null;
                    return false;
                }
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We never want to fail here so we have to catch all exceptions.")]
        private static void BeginReply(ReplyContext replyContext)
        {
            Contract.Assert(replyContext != null);

            try
            {
                IAsyncResult result = replyContext.RequestContext.BeginReply(replyContext.Reply, _onReplyComplete, replyContext);
                if (result.CompletedSynchronously)
                {
                    ReplyComplete(result);
                }
            }
            catch
            {
                BeginReceiveRequestContext(replyContext.ChannelContext);
                replyContext.Dispose();
            }
        }

        private static void OnReplyComplete(IAsyncResult result)
        {
            Contract.Assert(result != null);

            if (result.CompletedSynchronously)
            {
                return;
            }

            ReplyComplete(result);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We never want to fail here so we have to catch all exceptions.")]
        private static void ReplyComplete(IAsyncResult result)
        {
            ReplyContext replyContext = (ReplyContext)result.AsyncState;
            Contract.Assert(replyContext != null);

            try
            {
                replyContext.RequestContext.EndReply(result);
            }
            catch
            {
            }
            finally
            {
                BeginReceiveRequestContext(replyContext.ChannelContext);
                replyContext.Dispose();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We never want to fail here so we have to catch all exceptions.")]
        private static void BeginCloseListener(HttpSelfHostServer server)
        {
            Contract.Assert(server != null);

            try
            {
                if (server._listener != null)
                {
                    IAsyncResult result = server._listener.BeginClose(_onCloseListenerComplete, server);
                    if (result.CompletedSynchronously)
                    {
                        CloseListenerComplete(result);
                    }
                }
            }
            catch
            {
                CloseNextChannel(server);
            }
        }

        private static void OnCloseListenerComplete(IAsyncResult result)
        {
            Contract.Assert(result != null);

            if (result.CompletedSynchronously)
            {
                return;
            }

            CloseListenerComplete(result);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We never want to fail here so we have to catch all exceptions.")]
        private static void CloseListenerComplete(IAsyncResult result)
        {
            HttpSelfHostServer server = (HttpSelfHostServer)result.AsyncState;
            Contract.Assert(server != null);
            Contract.Assert(server._listener != null);

            try
            {
                server._listener.EndClose(result);
            }
            catch
            {
            }
            finally
            {
                CloseNextChannel(server);
            }
        }

        private static void CloseNextChannel(HttpSelfHostServer server)
        {
            Contract.Assert(server != null);

            IReplyChannel channel;
            if (server._channels.TryTake(out channel))
            {
                BeginCloseChannel(new ChannelContext(server, channel));
            }
            else
            {
                CompleteTask(server._closeTaskCompletionSource);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We never want to fail here so we have to catch all exceptions.")]
        private static void BeginCloseChannel(ChannelContext channelContext)
        {
            Contract.Assert(channelContext != null);

            try
            {
                IAsyncResult result = channelContext.Channel.BeginClose(_onCloseChannelComplete, channelContext);
                if (result.CompletedSynchronously)
                {
                    CloseChannelComplete(result);
                }
            }
            catch
            {
                CloseNextChannel(channelContext.Server);
            }
        }

        private static void OnCloseChannelComplete(IAsyncResult result)
        {
            Contract.Assert(result != null);

            if (result.CompletedSynchronously)
            {
                return;
            }

            CloseChannelComplete(result);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We never want to fail here so we have to catch all exceptions.")]
        private static void CloseChannelComplete(IAsyncResult result)
        {
            ChannelContext channelContext = (ChannelContext)result.AsyncState;
            Contract.Assert(channelContext != null);

            try
            {
                channelContext.Channel.EndClose(result);
            }
            catch
            {
            }
            finally
            {
                CloseNextChannel(channelContext.Server);
            }
        }

        /// <summary>
        /// Provides context for receiving an <see cref="System.ServiceModel.Channels.RequestContext"/> instance asynchronously.
        /// </summary>
        private class ChannelContext
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ChannelContext"/> class.
            /// </summary>
            /// <param name="server">The host to associate with this context.</param>
            /// <param name="channel">The channel to associate with this channel.</param>
            public ChannelContext(HttpSelfHostServer server, IReplyChannel channel)
            {
                Contract.Assert(server != null);
                Contract.Assert(channel != null);
                Server = server;
                Channel = channel;
            }

            /// <summary>
            /// Gets the <see cref="HttpSelfHostServer"/> instance.
            /// </summary>
            /// <value>
            /// The <see cref="HttpSelfHostServer"/> instance.
            /// </value>
            public HttpSelfHostServer Server { get; private set; }

            /// <summary>
            /// Gets the <see cref="IReplyChannel"/> instance.
            /// </summary>
            /// <value>
            /// The <see cref="System.ServiceModel.Channels.RequestContext"/> instance.
            /// </value>
            public IReplyChannel Channel { get; private set; }
        }

        /// <summary>
        /// Provides context for sending a <see cref="System.ServiceModel.Channels.Message"/> instance asynchronously in response
        /// to a request.
        /// </summary>
        private class ReplyContext : IDisposable
        {
            private bool _disposed;

            /// <summary>
            /// Initializes a new instance of the <see cref="ChannelContext"/> class.
            /// </summary>
            /// <param name="channelContext">The channel context to associate with this reply context.</param>
            /// <param name="requestContext">The request context to associate with this reply context.</param>
            /// <param name="reply">The reply to associate with this reply context.</param>
            public ReplyContext(ChannelContext channelContext, RequestContext requestContext, Message reply)
            {
                Contract.Assert(channelContext != null);
                Contract.Assert(requestContext != null);
                Contract.Assert(reply != null);

                ChannelContext = channelContext;
                RequestContext = requestContext;
                Reply = reply;
            }

            /// <summary>
            /// Gets the <see cref="ChannelContext"/> instance.
            /// </summary>
            /// <value>
            /// The <see cref="ChannelContext"/> instance.
            /// </value>
            public ChannelContext ChannelContext { get; private set; }

            /// <summary>
            /// Gets the <see cref="System.ServiceModel.Channels.RequestContext"/> instance.
            /// </summary>
            /// <value>
            /// The <see cref="System.ServiceModel.Channels.RequestContext"/> instance.
            /// </value>
            public RequestContext RequestContext { get; private set; }

            /// <summary>
            /// Gets the reply <see cref="System.ServiceModel.Channels.Message"/> instance.
            /// </summary>
            /// <value>
            /// The reply <see cref="System.ServiceModel.Channels.Message"/> instance.
            /// </value>
            public Message Reply { get; private set; }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged SRResources.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Releases unmanaged and - optionally - managed resources
            /// </summary>
            /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged SRResources.</param>
            protected virtual void Dispose(bool disposing)
            {
                if (!_disposed)
                {
                    if (disposing)
                    {
                        RequestContext.Close();
                        Reply.Close();
                    }

                    _disposed = true;
                }
            }
        }
    }
}
