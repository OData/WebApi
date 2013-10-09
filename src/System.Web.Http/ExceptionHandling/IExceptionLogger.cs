// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace System.Web.Http.ExceptionHandling
{
    /// <summary>Defines an unhandled exception logger.</summary>
    public interface IExceptionLogger
    {
        /// <summary>Logs an unhandled exception.</summary>
        /// <param name="context">The exception logger context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task.</returns>
        Task LogAsync(ExceptionLoggerContext context, CancellationToken cancellationToken);
    }
}
