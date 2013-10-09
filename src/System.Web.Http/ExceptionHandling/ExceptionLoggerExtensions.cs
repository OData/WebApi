// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace System.Web.Http.ExceptionHandling
{
    /// <summary>Provides extension methods for <see cref="IExceptionLogger"/>.</summary>
    public static class ExceptionLoggerExtensions
    {
        /// <summary>Calls an exception logger.</summary>
        /// <param name="logger">The unhandled exception logger.</param>
        /// <param name="context">The exception context.</param>
        /// <param name="canBeHandled">A value indicating whether the exception can subsequently be handled.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task.</returns>
        public static Task LogAsync(this IExceptionLogger logger, ExceptionContext context, bool canBeHandled,
            CancellationToken cancellationToken)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            ExceptionLoggerContext loggerContext = new ExceptionLoggerContext
            {
                ExceptionContext = context,
                CanBeHandled = canBeHandled
            };

            return logger.LogAsync(loggerContext, cancellationToken);
        }
    }
}
