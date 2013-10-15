// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;

namespace System.Web.Http.WebHost.Routing
{
    /// <summary>Represents a handler that asynchronously handles an unhandled exception from routing.</summary>
    internal class HttpRouteExceptionHandler : HttpTaskAsyncHandler
    {
        private readonly ExceptionDispatchInfo _exceptionInfo;
        private readonly IExceptionLogger _exceptionLogger;
        private readonly IExceptionHandler _exceptionHandler;

        public HttpRouteExceptionHandler(ExceptionDispatchInfo exceptionInfo)
            : this(exceptionInfo, ExceptionServices.GetLogger(GlobalConfiguration.Configuration),
            ExceptionServices.GetHandler(GlobalConfiguration.Configuration))
        {
        }

        internal HttpRouteExceptionHandler(ExceptionDispatchInfo exceptionInfo,
            IExceptionLogger exceptionLogger, IExceptionHandler exceptionHandler)
        {
            Contract.Assert(exceptionInfo != null);
            Contract.Assert(exceptionLogger != null);
            Contract.Assert(exceptionHandler != null);

            _exceptionInfo = exceptionInfo;
            _exceptionLogger = exceptionLogger;
            _exceptionHandler = exceptionHandler;
        }

        internal ExceptionDispatchInfo ExceptionInfo
        {
            get { return _exceptionInfo; }
        }

        internal IExceptionLogger ExceptionLogger
        {
            get { return _exceptionLogger; }
        }

        internal IExceptionHandler ExceptionHandler
        {
            get { return _exceptionHandler; }
        }

        public override Task ProcessRequestAsync(HttpContext context)
        {
            return ProcessRequestAsync(new HttpContextWrapper(context));
        }

        internal async Task ProcessRequestAsync(HttpContextBase context)
        {
            Exception exception = _exceptionInfo.SourceException;
            Contract.Assert(exception != null);

            CancellationToken cancellationToken = CancellationToken.None;

            HttpResponseException responseException = exception as HttpResponseException;

            if (responseException != null)
            {
                // This method call is hardend and designed not to throw exceptions (since they won't be caught and
                // handled further by its callers).
                await HttpControllerHandler.CopyResponseAsync(context, context.GetOrCreateHttpRequestMessage(),
                    responseException.Response, cancellationToken);
            }
            else
            {
                // This method call is hardend and designed not to throw exceptions (since they won't be caught and
                // handled further by its callers).
                bool handled = await HttpControllerHandler.CopyErrorResponseAsync(
                    WebHostExceptionCatchBlocks.HttpWebRoute, context, context.GetOrCreateHttpRequestMessage(),
                    null, _exceptionInfo.SourceException, cancellationToken, _exceptionLogger, _exceptionHandler);

                if (!handled)
                {
                    _exceptionInfo.Throw();
                }
            }
        }
    }
}
