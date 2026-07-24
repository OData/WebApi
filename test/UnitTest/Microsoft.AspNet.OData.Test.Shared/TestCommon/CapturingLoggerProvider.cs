//-----------------------------------------------------------------------------
// <copyright file="CapturingLoggerProvider.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNet.OData.Test.Common
{
    /// <summary>
    /// An <see cref="ILoggerProvider"/> that captures every log entry it receives so tests can assert on the
    /// category, level, formatted message, and the individual structured fields that were logged.
    /// </summary>
    public sealed class CapturingLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentQueue<CapturedLogEntry> _entries = new ConcurrentQueue<CapturedLogEntry>();

        /// <summary>
        /// Gets a snapshot of the log entries captured so far.
        /// </summary>
        public IReadOnlyList<CapturedLogEntry> Entries => _entries.ToArray();

        /// <summary>
        /// Removes all captured entries.
        /// </summary>
        public void Clear()
        {
            while (_entries.TryDequeue(out _))
            {
            }
        }

        /// <inheritdoc/>
        public ILogger CreateLogger(string categoryName)
        {
            return new CapturingLogger(categoryName, _entries);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        private sealed class CapturingLogger : ILogger
        {
            private readonly string _categoryName;
            private readonly ConcurrentQueue<CapturedLogEntry> _entries;

            public CapturingLogger(string categoryName, ConcurrentQueue<CapturedLogEntry> entries)
            {
                _categoryName = categoryName;
                _entries = entries;
            }

            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                Dictionary<string, object> fields = new Dictionary<string, object>();
                if (state is IReadOnlyList<KeyValuePair<string, object>> structuredState)
                {
                    foreach (KeyValuePair<string, object> pair in structuredState)
                    {
                        fields[pair.Key] = pair.Value;
                    }
                }

                CapturedLogEntry entry = new CapturedLogEntry
                {
                    Category = _categoryName,
                    Level = logLevel,
                    EventId = eventId,
                    Message = formatter != null ? formatter(state, exception) : null,
                    Exception = exception,
                    Fields = fields
                };

                _entries.Enqueue(entry);
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

    /// <summary>
    /// A single captured log entry.
    /// </summary>
    public sealed class CapturedLogEntry
    {
        /// <summary>
        /// Gets or sets the logger category name.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the log level.
        /// </summary>
        public LogLevel Level { get; set; }

        /// <summary>
        /// Gets or sets the event id.
        /// </summary>
        public EventId EventId { get; set; }

        /// <summary>
        /// Gets or sets the formatted message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the exception associated with the entry, if any.
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Gets or sets the structured fields captured from the log state.
        /// </summary>
        public IReadOnlyDictionary<string, object> Fields { get; set; }

        /// <summary>
        /// Gets the string value of a structured field, or <c>null</c> when the field is not present.
        /// </summary>
        /// <param name="name">The field name.</param>
        /// <returns>The field value as a string, or <c>null</c>.</returns>
        public string GetFieldValue(string name)
        {
            if (Fields != null && Fields.TryGetValue(name, out object value))
            {
                return value?.ToString();
            }

            return null;
        }
    }
}
#endif
