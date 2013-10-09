// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Properties;

namespace System.Web.Http.ExceptionHandling
{
    /// <summary>Represents an unhandled exception handler.</summary>
    public abstract class ExceptionHandler : IExceptionHandler
    {
        /// <inheritdoc />
        public Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (context.ExceptionContext == null)
            {
                throw new ArgumentException(Error.Format(SRResources.TypePropertyMustNotBeNull,
                    typeof(ExceptionHandlerContext).Name, "ExceptionContext"), "context");
            }

            if (context.ExceptionContext.Exception == null)
            {
                throw new ArgumentException(Error.Format(SRResources.TypePropertyMustNotBeNull,
                    typeof(ExceptionContext).Name, "Exception"), "context");
            }

            if (!ShouldHandle(context))
            {
                return Task.FromResult<object>(null);
            }

            return HandleAsyncCore(context, cancellationToken);
        }

        /// <summary>When overridden in a derived class, handles the exception asynchronously.</summary>
        /// <param name="context">The exception handler context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task.</returns>
        public virtual Task HandleAsyncCore(ExceptionHandlerContext context, CancellationToken cancellationToken)
        {
            HandleCore(context);
            return Task.FromResult<object>(null);
        }

        /// <summary>When overridden in a derived class, handles the exception synchronously.</summary>
        /// <param name="context">The exception handler context.</param>
        public virtual void HandleCore(ExceptionHandlerContext context)
        {
        }

        /// <summary>Determines whether the exception should be handled.</summary>
        /// <param name="context">The exception handler context.</param>
        /// <returns>
        /// <see langword="true"/> if the exception should be handled; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>The default decision is only to handle exceptions caught at top-level catch blocks.</remarks>
        public virtual bool ShouldHandle(ExceptionHandlerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            ExceptionContext exceptionContext = context.ExceptionContext;

            if (exceptionContext == null)
            {
                throw new ArgumentException(Error.Format(SRResources.TypePropertyMustNotBeNull,
                    typeof(ExceptionHandlerContext).Name, "ExceptionContext"), "context");
            }

            return exceptionContext.IsTopLevelCatchBlock;
        }
    }
}
