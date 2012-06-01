// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using System.Web.Http.WebHost.Properties;
using System.Web.Http.WebHost.Routing;
using System.Web.Routing;

namespace System.Web.Http.WebHost
{
    /// <summary>
    /// A <see cref="IHttpAsyncHandler"/> that passes ASP.NET requests into the <see cref="HttpServer"/>
    /// pipeline and write the result back.
    /// </summary>
    public class HttpControllerHandler : IHttpAsyncHandler
    {
        internal static readonly string HttpContextBaseKey = "MS_HttpContext";

        private static readonly Lazy<Action<HttpContextBase>> _suppressRedirectAction =
            new Lazy<Action<HttpContextBase>>(
                () =>
                {
                    // If the behavior is explicitly disabled, do nothing
                    if (!SuppressFormsAuthRedirectModule.GetEnabled(WebConfigurationManager.AppSettings))
                    {
                        return httpContext => { };
                    }

                    var srPropertyInfo = typeof(HttpResponseBase).GetProperty(SuppressFormsAuthRedirectModule.SuppressFormsAuthenticationRedirectPropertyName, BindingFlags.Instance | BindingFlags.Public);

                    // Use the property in .NET 4.5 if available
                    if (srPropertyInfo != null)
                    {
                        Action<HttpResponseBase, bool> setter = (Action<HttpResponseBase, bool>)Delegate.CreateDelegate(typeof(Action<HttpResponseBase, bool>), srPropertyInfo.GetSetMethod(), throwOnBindFailure: false);
                        return httpContext => setter(httpContext.Response, true);
                    }

                    // Use SuppressFormsAuthRedirectModule to revert the redirection on .NET 4.0
                    return httpContext => SuppressFormsAuthRedirectModule.DisableAuthenticationRedirect(httpContext);
                });

        private static readonly Lazy<HttpMessageInvoker> _server =
            new Lazy<HttpMessageInvoker>(
                () =>
                {
                    HttpServer server = new HttpServer(GlobalConfiguration.Configuration, GlobalConfiguration.DefaultHandler);
                    return new HttpMessageInvoker(server);
                });

        private static readonly Lazy<IHostBufferPolicySelector> _bufferPolicySelector =
            new Lazy<IHostBufferPolicySelector>(() => GlobalConfiguration.Configuration.Services.GetHostBufferPolicySelector());

        private static readonly Func<HttpRequestMessage, X509Certificate2> _retrieveClientCertificate = new Func<HttpRequestMessage, X509Certificate2>(RetrieveClientCertificate);

        private IHttpRouteData _routeData;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpControllerHandler"/> class.
        /// </summary>
        /// <param name="routeData">The route data.</param>
        public HttpControllerHandler(RouteData routeData)
        {
            if (routeData == null)
            {
                throw Error.ArgumentNull("routeData");
            }

            _routeData = new HostedHttpRouteData(routeData);
        }

        /// <summary>
        /// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler"/> instance.
        /// </summary>
        /// <returns>true if the <see cref="T:System.Web.IHttpHandler"/> instance is reusable; otherwise, false.</returns>
        bool IHttpHandler.IsReusable
        {
            get { return IsReusable; }
        }

        /// <summary>
        /// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler"/> instance.
        /// </summary>
        /// <returns>true if the <see cref="T:System.Web.IHttpHandler"/> instance is reusable; otherwise, false.</returns>
        protected virtual bool IsReusable
        {
            get { return false; }
        }

        /// <summary>
        /// Processes the request.
        /// </summary>
        /// <param name="httpContext">The HTTP context base.</param>
        void IHttpHandler.ProcessRequest(HttpContext httpContext)
        {
            ProcessRequest(new HttpContextWrapper(httpContext));
        }

        /// <summary>
        /// Begins processing the request.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="state">The state.</param>
        /// <returns>An <see cref="IAsyncResult"/> that contains information about the status of the process. </returns>
        IAsyncResult IHttpAsyncHandler.BeginProcessRequest(HttpContext httpContext, AsyncCallback callback, object state)
        {
            return BeginProcessRequest(new HttpContextWrapper(httpContext), callback, state);
        }

        /// <summary>
        /// Provides an asynchronous process End method when the process ends.
        /// </summary>
        /// <param name="result">An <see cref="T:System.IAsyncResult"/> that contains information about the status of the process.</param>
        void IHttpAsyncHandler.EndProcessRequest(IAsyncResult result)
        {
            EndProcessRequest(result);
        }

        /// <summary>
        /// Processes the request.
        /// </summary>
        /// <param name="httpContextBase">The HTTP context base.</param>
        protected virtual void ProcessRequest(HttpContextBase httpContextBase)
        {
            throw Error.NotSupported(SRResources.ProcessRequestNotSupported, typeof(HttpControllerHandler));
        }

        /// <summary>
        /// Begins the process request.
        /// </summary>
        /// <param name="httpContextBase">The HTTP context base.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="state">The state.</param>
        /// <returns>An <see cref="IAsyncResult"/> that contains information about the status of the process. </returns>
        [SuppressMessage("Microsoft.WebAPI", "CR4001:DoNotCallProblematicMethodsOnTask", Justification = "This is commented in great details.")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Object gets passed to a task")]
        protected virtual IAsyncResult BeginProcessRequest(HttpContextBase httpContextBase, AsyncCallback callback, object state)
        {
            HttpRequestMessage request = httpContextBase.GetHttpRequestMessage() ?? ConvertRequest(httpContextBase);

            // Add route data
            request.Properties[HttpPropertyKeys.HttpRouteDataKey] = _routeData;

            Task responseBodyTask = _server.Value.SendAsync(request, CancellationToken.None)
                .Then(response => ConvertResponse(httpContextBase, response, request));

            TaskWrapperAsyncResult result = new TaskWrapperAsyncResult(responseBodyTask, state);

            if (callback != null)
            {
                if (result.IsCompleted)
                {
                    // If the underlying task is already finished, from our caller's perspective this is just
                    // a synchronous completion. See also DevDiv #346170.
                    result.CompletedSynchronously = true;
                    callback(result);
                }
                else
                {
                    // If the underlying task isn't yet finished, from our caller's perspective this will be
                    // an asynchronous completion. We'll use ContinueWith instead of Finally for two reasons:
                    //
                    // - Finally propagates the antecedent Task's exception, which we don't need to do here.
                    //   Out caller will eventually call EndProcessRequest, which correctly observes the
                    //   antecedent Task's exception anyway if it faulted.
                    //
                    // - Finally invokes the callback on the captured SynchronizationContext, which is
                    //   unnecessary when using APM (Begin / End). APM assumes that the callback is invoked
                    //   on an arbitrary ThreadPool thread with no SynchronizationContext set up, so
                    //   ContinueWith gets us closer to the desired semantic.
                    //
                    // There is still a race here: the Task might complete after the IsCompleted check above,
                    // so the callback might be invoked on another thread concurrently with the original
                    // thread's call to BeginProcessRequest. But we shouldn't concern ourselves with that;
                    // the caller has to be prepared for that possibility and do the right thing. We also
                    // don't need to worry about the callback throwing since the caller should give us a
                    // callback which is well-behaved.
                    result.CompletedSynchronously = false;
                    responseBodyTask.ContinueWith(_ =>
                    {
                        callback(result);
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// Provides an asynchronous process End method when the process ends.
        /// </summary>
        /// <param name="result">An <see cref="T:System.IAsyncResult"/> that contains information about the status of the process.</param>
        protected virtual void EndProcessRequest(IAsyncResult result)
        {
            TaskWrapperAsyncResult asyncResult = (TaskWrapperAsyncResult)result;
            Contract.Assert(asyncResult != null);
            Task task = asyncResult.Task;

            // Check task result and unwrap any exceptions
            if (task.IsCanceled)
            {
                throw Error.OperationCanceled();
            }
            else if (task.IsFaulted)
            {
                throw task.Exception.GetBaseException();
            }
        }

        private static void CopyHeaders(HttpHeaders from, HttpContextBase to)
        {
            Contract.Assert(from != null);
            Contract.Assert(to != null);

            foreach (var header in from)
            {
                string name = header.Key;
                foreach (var value in header.Value)
                {
                    to.Response.AppendHeader(name, value);
                }
            }
        }

        private static void AddHeaderToHttpRequestMessage(HttpRequestMessage httpRequestMessage, string headerName, string[] headerValues)
        {
            Contract.Assert(httpRequestMessage != null);
            Contract.Assert(headerName != null);
            Contract.Assert(headerValues != null);

            if (!httpRequestMessage.Headers.TryAddWithoutValidation(headerName, headerValues))
            {
                httpRequestMessage.Content.Headers.TryAddWithoutValidation(headerName, headerValues);
            }
        }

        /// <summary>
        /// Converts a <see cref="HttpResponseMessage"/> to an <see cref="HttpResponseBase"/> and disposes the 
        /// <see cref="HttpResponseMessage"/> and <see cref="HttpRequestMessage"/> upon completion.
        /// </summary>
        /// <param name="httpContextBase">The HTTP context base.</param>
        /// <param name="response">The response to convert.</param>
        /// <param name="request">The request (which will be disposed).</param>
        /// <returns>A <see cref="Task"/> representing the conversion of an <see cref="HttpResponseMessage"/> to an <see cref="HttpResponseBase"/>
        /// including writing out any entity body.</returns>
        internal static Task ConvertResponse(HttpContextBase httpContextBase, HttpResponseMessage response, HttpRequestMessage request)
        {
            Contract.Assert(httpContextBase != null);
            Contract.Assert(request != null);

            // A null response creates a 500 with no content
            if (response == null)
            {
                CreateEmptyErrorResponse(httpContextBase.Response);
                return TaskHelpers.Completed();
            }

            CopyResponseStatusAndHeaders(httpContextBase, response);

            CacheControlHeaderValue cacheControl = response.Headers.CacheControl;

            // TODO 335085: Consider this when coming up with our caching story
            if (cacheControl == null)
            {
                // DevDiv2 #332323. ASP.NET by default always emits a cache-control: private header.
                // However, we don't want requests to be cached by default.
                // If nobody set an explicit CacheControl then explicitly set to no-cache to override the
                // default behavior. This will cause the following response headers to be emitted:
                //     Cache-Control: no-cache
                //     Pragma: no-cache
                //     Expires: -1
                httpContextBase.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            }

            // Asynchronously write the response body.  If there is no body, we use
            // a completed task to share the Finally() below.
            // The response-writing task will not fault -- it handles errors internally.
            Task writeResponseContentTask = (response.Content == null)
                                        ? TaskHelpers.Completed()
                                        : WriteResponseContentAsync(httpContextBase, response, request);

            return writeResponseContentTask.Finally(() =>
            {
                request.DisposeRequestResources();
                request.Dispose();
                response.Dispose();
            });
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller becomes owner")]
        internal static HttpRequestMessage ConvertRequest(HttpContextBase httpContextBase)
        {
            Contract.Assert(httpContextBase != null);

            HttpRequestBase requestBase = httpContextBase.Request;
            HttpMethod method = HttpMethodHelper.GetHttpMethod(requestBase.HttpMethod);
            Uri uri = requestBase.Url;
            HttpRequestMessage request = new HttpRequestMessage(method, uri);

            // Choose a buffered or bufferless input stream based on user's policy
            IHostBufferPolicySelector policySelector = _bufferPolicySelector.Value;
            bool isInputBuffered = policySelector == null ? true : policySelector.UseBufferedInputStream(httpContextBase);
            Stream inputStream = isInputBuffered
                                    ? requestBase.InputStream
                                    : httpContextBase.ApplicationInstance.Request.GetBufferlessInputStream();

            request.Content = new StreamContent(inputStream);
            foreach (string headerName in requestBase.Headers)
            {
                string[] values = requestBase.Headers.GetValues(headerName);
                AddHeaderToHttpRequestMessage(request, headerName, values);
            }

            // Add context to enable route lookup later on
            request.Properties.Add(HttpContextBaseKey, httpContextBase);

            // Add the retrieve client certificate delegate to the property bag to enable lookup later on
            request.Properties.Add(HttpPropertyKeys.RetrieveClientCertificateDelegateKey, _retrieveClientCertificate);

            // Add information about whether the request is local or not
            request.Properties.Add(HttpPropertyKeys.IsLocalKey, new Lazy<bool>(() => requestBase.IsLocal));

            // Add information about whether custom errors are enabled for this request or not
            request.Properties.Add(HttpPropertyKeys.IncludeErrorDetailKey, new Lazy<bool>(() => !httpContextBase.IsCustomErrorEnabled));

            return request;
        }

        /// <summary>
        /// Prevents the <see cref="T:System.Web.Security.FormsAuthenticationModule"/> from altering a 401 response to 302 by 
        /// setting <see cref="P:System.Web.HttpResponseBase.SuppressFormsAuthenticationRedirect" /> to <c>true</c> if available.
        /// </summary>
        /// <param name="httpContextBase">The HTTP context base.</param>
        internal static void EnsureSuppressFormsAuthenticationRedirect(HttpContextBase httpContextBase)
        {
            Contract.Assert(httpContextBase != null);

            // Only if the response is status code is 401
            if (httpContextBase.Response.StatusCode == (int)HttpStatusCode.Unauthorized)
            {
                _suppressRedirectAction.Value(httpContextBase);
            }
        }

        /// <summary>
        /// Asynchronously writes the response content to the ASP.NET output stream
        /// and sets the content headers.
        /// </summary>
        /// <remarks>
        /// This method returns only non-faulted tasks.  Any error encountered
        /// writing the response will be handled within the task returned by this method.
        /// </remarks>
        /// <param name="httpContextBase">The context base.</param>
        /// <param name="response">The response being written.</param>
        /// <param name="request">The original request.</param>
        /// <returns>The task that will write the response content.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "unused", Justification = "unused variable necessary to call getter")]
        internal static Task WriteResponseContentAsync(HttpContextBase httpContextBase, HttpResponseMessage response, HttpRequestMessage request)
        {
            Contract.Assert(httpContextBase != null);
            Contract.Assert(response != null);
            Contract.Assert(request != null);
            Contract.Assert(response.Content != null);

            HttpResponseBase httpResponseBase = httpContextBase.Response;
            HttpContent responseContent = response.Content;

            // Copy the response content headers only after ensuring they are complete.
            // We ask for Content-Length first because HttpContent lazily computes this
            // and only afterwards writes the value into the content headers.
            var unused = response.Content.Headers.ContentLength;
            CopyHeaders(response.Content.Headers, httpContextBase);

            // Select output buffering based on the user-controlled buffering policy
            bool isBuffered = _bufferPolicySelector.Value != null ? _bufferPolicySelector.Value.UseBufferedOutputStream(response) : true;
            httpResponseBase.BufferOutput = isBuffered;

            return isBuffered
                    ? WriteBufferedResponseContentAsync(httpContextBase, responseContent, request)
                    : WriteStreamedResponseContentAsync(httpResponseBase, responseContent);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "All exceptions caught here become error responses")]
        internal static Task WriteStreamedResponseContentAsync(HttpResponseBase httpResponseBase, HttpContent responseContent)
        {
            Contract.Assert(httpResponseBase != null);
            Contract.Assert(responseContent != null);

            Task writeResponseContentTask = null;

            try
            {
                // Copy the HttpContent into the output stream asynchronously.
                writeResponseContentTask = responseContent.CopyToAsync(httpResponseBase.OutputStream);
            }
            catch
            {
                // Streamed content may have been written and cannot be recalled.
                // Our only choice is to abort the connection.
                AbortConnection(httpResponseBase);
                return TaskHelpers.Completed();
            }

            return writeResponseContentTask
                .Catch((info) =>
                {
                    // Streamed content may have been written and cannot be recalled.
                    // Our only choice is to abort the connection.
                    AbortConnection(httpResponseBase);
                    return info.Handled();
                });
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "continuation task owned by caller")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "All exceptions caught here become error responses")]
        internal static Task WriteBufferedResponseContentAsync(HttpContextBase httpContextBase, HttpContent responseContent, HttpRequestMessage request)
        {
            Contract.Assert(httpContextBase != null);
            Contract.Assert(responseContent != null);
            Contract.Assert(request != null);

            HttpResponseBase httpResponseBase = httpContextBase.Response;
            Task writeResponseContentTask = null;

            try
            {
                // Copy the HttpContent into the output stream asynchronously.
                writeResponseContentTask = responseContent.CopyToAsync(httpResponseBase.OutputStream);
            }
            catch (Exception ex)
            {
                // Immediate exception requires an error response.
                // Create a faulted task to share the code below
                writeResponseContentTask = TaskHelpers.FromError(ex);
            }

            // Return a task that writes the response body asynchronously.
            // We guarantee we will handle all error responses internally
            // and always return a non-faulted task.
            return writeResponseContentTask
                .Catch((info) =>
                {
                    // If we were using a buffered stream, we can still set the headers and status
                    // code, and we can create an error response with the exception.
                    // We create a continuation task to write an error response that will run after
                    // returning from this Catch() but before other continuations the caller appends to this task.
                    // The error response writing task handles errors internally and will not show as faulted.
                    Task writeErrorResponseTask = CreateErrorResponseAsync(httpContextBase, responseContent, request, info.Exception);
                    return info.Task(writeErrorResponseTask);
                });
        }

        /// <summary>
        /// Asynchronously creates an error response.
        /// </summary>
        /// <remarks>
        /// This method returns a task that will set the headers and status code appropriately
        /// for an error response.  If possible, it will also write the exception as an
        /// <see cref="HttpError"/> into the response body.
        /// <para>
        /// Any errors during the creation of the error response itself will be handled
        /// internally.  The task returned from this method will not show as faulted.
        /// </para>
        /// </remarks>
        /// <param name="httpContextBase">The HTTP context.</param>
        /// <param name="responseContent">The original response content we could not write.</param>
        /// <param name="request">The original request.</param>
        /// <param name="exception">The exception caught attempting to write <paramref name="responseContent"/>.</param>
        /// <returns>A task that will create the error response.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "unused", Justification = "unused variable necessary to call getter")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "All exceptions caught here become error responses")]
        internal static Task CreateErrorResponseAsync(HttpContextBase httpContextBase, HttpContent responseContent, HttpRequestMessage request, Exception exception)
        {
            Contract.Assert(httpContextBase != null);
            Contract.Assert(responseContent != null);
            Contract.Assert(exception != null);
            Contract.Assert(request != null);

            HttpResponseBase httpResponseBase = httpContextBase.Response;
            HttpResponseMessage errorResponse = null;
            HttpResponseException responseException = exception as HttpResponseException;

            // Ensure all headers and content are cleared to eliminate any partial results.
            ClearContentAndHeaders(httpResponseBase);

            // If the exception we are handling is HttpResponseException,
            // that becomes the error response.
            if (responseException != null)
            {
                errorResponse = responseException.Response;
            }
            else
            {
                // The exception is not HttpResponseException.
                // Create a 500 response with content containing an explanatory message and
                // stack trace, subject to content negotiation and policy for error details.
                try
                {
                    MediaTypeHeaderValue mediaType = responseContent.Headers.ContentType;
                    string messageDetails = (mediaType != null)
                                                ? Error.Format(
                                                    SRResources.Serialize_Response_Failed_MediaType,
                                                    responseContent.GetType().Name,
                                                    mediaType)
                                                : Error.Format(
                                                    SRResources.Serialize_Response_Failed,
                                                    responseContent.GetType().Name);

                    errorResponse = request.CreateErrorResponse(
                                                HttpStatusCode.InternalServerError,
                                                new InvalidOperationException(messageDetails, exception));

                    // CreateErrorResponse will choose 406 if it cannot find a formatter,
                    // but we want our default error response to be 500 always
                    errorResponse.StatusCode = HttpStatusCode.InternalServerError;
                }
                catch
                {
                    // Failed creating an HttpResponseMessage for the error response.
                    // This can happen for missing config, missing conneg service, etc.
                    // Create an empty error response and return a non-faulted task.
                    CreateEmptyErrorResponse(httpResponseBase);
                    return TaskHelpers.Completed();
                }
            }

            Contract.Assert(errorResponse != null);
            CopyResponseStatusAndHeaders(httpContextBase, errorResponse);

            // The error response may return a null content if content negotiation
            // fails to find a formatter, or this may be an HttpResponseException without
            // content.  In either case, cleanup and return a completed task.

            if (errorResponse.Content == null)
            {
                errorResponse.Dispose();
                return TaskHelpers.Completed();
            }

            // Copy the headers from the newly generated HttpResponseMessage.
            // We must ask the content for its content length because Content-Length
            // is lazily computed and added to the headers.
            var unused = errorResponse.Content.Headers.ContentLength;
            CopyHeaders(errorResponse.Content.Headers, httpContextBase);

            Task writeErrorResponseTask = null;
            try
            {
                // Asynchronously write the content of the new error HttpResponseMessage
                writeErrorResponseTask = errorResponse.Content.CopyToAsync(httpResponseBase.OutputStream);
            }
            catch (Exception ex)
            {
                // Failed creating the error response task.  Likely cause is a formatter exception
                // before the write task could be created.  Create a faulted task to share
                // the Catch() and Finally() below
                writeErrorResponseTask = TaskHelpers.FromError(ex);
            }

            return writeErrorResponseTask
                .Catch((info) =>
                {
                    // Failure writing the error response.  Likely cause is a formatter
                    // serialization exception.  Create empty error response and
                    // return a non-faulted task.
                    CreateEmptyErrorResponse(httpResponseBase);
                    return info.Handled();
                })
                .Finally(() =>
                {
                    // Dispose the temporary HttpResponseMessage carrying the error response
                    errorResponse.Dispose();
                });
        }

        private static void CopyResponseStatusAndHeaders(HttpContextBase httpContextBase, HttpResponseMessage response)
        {
            Contract.Assert(httpContextBase != null);
            HttpResponseBase httpResponseBase = httpContextBase.Response;
            httpResponseBase.StatusCode = (int)response.StatusCode;
            httpResponseBase.StatusDescription = response.ReasonPhrase;
            httpResponseBase.TrySkipIisCustomErrors = true;
            EnsureSuppressFormsAuthenticationRedirect(httpContextBase);
            CopyHeaders(response.Headers, httpContextBase);
        }

        private static void ClearContentAndHeaders(HttpResponseBase httpResponseBase)
        {
            httpResponseBase.Clear();
            httpResponseBase.ClearHeaders();
        }

        private static void CreateEmptyErrorResponse(HttpResponseBase httpResponseBase)
        {
            ClearContentAndHeaders(httpResponseBase);
            httpResponseBase.StatusCode = (int)HttpStatusCode.InternalServerError;
            httpResponseBase.SuppressContent = true;
        }

        private static void AbortConnection(HttpResponseBase httpResponseBase)
        {
            // TODO: DevDiv bug #381233 -- call HttpResponse.Abort when it becomes available in 4.5
            httpResponseBase.Close();
        }

        private static X509Certificate2 RetrieveClientCertificate(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            X509Certificate2 result = null;

            HttpContextBase httpContextBase;
            if (request.Properties.TryGetValue(HttpControllerHandler.HttpContextBaseKey, out httpContextBase))
            {
                if (httpContextBase.Request.ClientCertificate.Certificate != null && httpContextBase.Request.ClientCertificate.Certificate.Length > 0)
                {
                    result = new X509Certificate2(httpContextBase.Request.ClientCertificate.Certificate);
                }
            }

            return result;
        }
    }
}
