//-----------------------------------------------------------------------------
// <copyright file="ThrowingLoggerProvider.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNet.OData.Test.Common
{
    /// <summary>
    /// An <see cref="ILoggerProvider"/> whose logger throws when it writes an entry, so tests can verify that a
    /// misbehaving logging sink never changes the outcome of the request. By default it throws for every category;
    /// pass a category name to throw only for that category and act as a no-op for all others, which keeps the rest
    /// of the framework's logging working while still failing the write under test.
    /// </summary>
    public sealed class ThrowingLoggerProvider : ILoggerProvider
    {
        private readonly string _throwForCategory;
        private readonly string _message;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThrowingLoggerProvider"/> class.
        /// </summary>
        /// <param name="throwForCategory">
        /// The category the logger throws for, or <c>null</c> to throw for every category.
        /// </param>
        /// <param name="message">The message of the thrown exception.</param>
        public ThrowingLoggerProvider(string throwForCategory = null, string message = "Simulated logging sink failure.")
        {
            _throwForCategory = throwForCategory;
            _message = message;
        }

        /// <inheritdoc/>
        public ILogger CreateLogger(string categoryName)
        {
            bool shouldThrow = _throwForCategory == null || string.Equals(categoryName, _throwForCategory, StringComparison.Ordinal);
            return new ThrowingLogger(_message, shouldThrow);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        private sealed class ThrowingLogger : ILogger
        {
            private readonly string _message;
            private readonly bool _shouldThrow;

            public ThrowingLogger(string message, bool shouldThrow)
            {
                _message = message;
                _shouldThrow = shouldThrow;
            }

            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (_shouldThrow)
                {
                    throw new InvalidOperationException(_message);
                }
            }

            private sealed class NullScope : IDisposable
            {
                public static readonly NullScope Instance = new NullScope();

                public void Dispose()
                {
                }
            }
        }
    }
}
#endif
