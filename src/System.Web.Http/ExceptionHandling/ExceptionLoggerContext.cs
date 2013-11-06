// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.ExceptionHandling
{
    /// <summary>Represents the context within which unhandled exception logging occurs.</summary>
    public class ExceptionLoggerContext
    {
        private readonly ExceptionContext _exceptionContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionLoggerContext"/> class using the values provided.
        /// </summary>
        /// <param name="exceptionContext">The exception context.</param>
        public ExceptionLoggerContext(ExceptionContext exceptionContext)
        {
            if (exceptionContext == null)
            {
                throw new ArgumentNullException("exceptionContext");
            }

            _exceptionContext = exceptionContext;
        }

        /// <summary>Gets or sets the exception context providing the exception and related data.</summary>
        public ExceptionContext ExceptionContext
        {
            get { return _exceptionContext; }
        }
    }
}
