// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.ExceptionHandling
{
    /// <summary>Represents the context within which unhandled exception logging occurs.</summary>
    public class ExceptionLoggerContext
    {
        /// <summary>Gets or sets the exception context providing the exception and related data.</summary>
        public ExceptionContext ExceptionContext { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the exception can subsequently be handled by an
        /// <see cref="IExceptionHandler"/> to produce a new response message.
        /// </summary>
        /// <remarks>
        /// Some exceptions are caught after a response is already partially sent, which prevents sending a new
        /// response to handle the exception. In such cases, <see cref="IExceptionLogger"/> will be called to log the
        /// exception, but the <see cref="IExceptionHandler"/> will not be called.
        /// </remarks>
        public bool CanBeHandled { get; set; }
    }
}
