// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Services;

namespace System.Web.Http.Tracing.Tracers
{
    /// <summary>Represents a tracer for an <see cref="IAuthenticationFilter"/>.</summary>
    internal class AuthenticationFilterTracer : FilterTracer, IAuthenticationFilter, IDecorator<IAuthenticationFilter>
    {
        private const string AuthenticateAsyncMethodName = "AuthenticateAsync";
        private const string ChallengeAsyncMethodName = "ChallengeAsync";

        private readonly IAuthenticationFilter _innerFilter;

        public AuthenticationFilterTracer(IAuthenticationFilter innerFilter, ITraceWriter traceWriter)
            : base(innerFilter, traceWriter)
        {
            _innerFilter = innerFilter;
        }

        public new IAuthenticationFilter Inner
        {
            get { return _innerFilter; }
        }

        public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            return TraceWriter.TraceBeginEndAsync(
                request: context != null ? context.Request : null,
                category: TraceCategories.FiltersCategory,
                level: TraceLevel.Info,
                operatorName: _innerFilter.GetType().Name,
                operationName: AuthenticateAsyncMethodName,
                beginTrace: null,
                execute: () => _innerFilter.AuthenticateAsync(context, cancellationToken),
                endTrace: null,
                errorTrace: null);
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            return TraceWriter.TraceBeginEndAsync(
                request: context != null ? context.Request : null,
                category: TraceCategories.FiltersCategory,
                level: TraceLevel.Info,
                operatorName: _innerFilter.GetType().Name,
                operationName: ChallengeAsyncMethodName,
                beginTrace: null,
                execute: () => _innerFilter.ChallengeAsync(context, cancellationToken),
                endTrace: null,
                errorTrace: null);
        }
    }
}
