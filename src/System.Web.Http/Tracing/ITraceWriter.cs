// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;

namespace System.Web.Http.Tracing
{
    /// <summary>
    /// Interface to write <see cref="TraceRecord"/> instances.
    /// </summary>
    public interface ITraceWriter
    {
        /// <summary>
        /// Determines whether tracing is currently enabled for the given <paramref name="category"/>
        /// and <paramref name="level"/>.
        /// </summary>
        /// <param name="category">The trace category.</param>
        /// <param name="level">The <see cref="TraceLevel"/></param>
        /// <returns>Returns <c>true</c> if tracing is currently enabled for the category and level,
        /// otherwise returns <c>false</c>.</returns>
        bool IsEnabled(string category, TraceLevel level);

        /// <summary>
        /// Invokes the specified <paramref name="traceAction"/> to allow setting values in
        /// a new <see cref="TraceRecord"/> if and only if tracing is permitted at the given
        /// <paramref name="category"/> and <paramref name="level"/>.
        /// </summary>
        /// <remarks>
        /// If tracing is permitted at the given category and level, the <see cref="ITraceWriter"/>
        /// will construct a <see cref="TraceRecord"/> and invoke the caller's action to allow
        /// it to set values in the <see cref="TraceRecord"/> provided to it.   
        /// When the caller's action returns, the <see cref="TraceRecord"/>
        /// will be recorded.   If tracing is not enabled, <paramref name="traceAction"/> will not be called.
        /// </remarks>
        /// <param name="request">The current <see cref="HttpRequestMessage"/>.  
        /// It may be <c>null</c> but doing so will prevent subsequent trace analysis 
        /// from correlating the trace to a particular request.</param>
        /// <param name="category">The logical category for the trace.  Users can define their own.</param>
        /// <param name="level">The <see cref="TraceLevel"/> at which to write this trace.</param>
        /// <param name="traceAction">The action to invoke if tracing is enabled.  The caller is expected
        /// to fill in the fields of the given <see cref="TraceRecord"/> in this action.</param>
        void Trace(HttpRequestMessage request, string category, TraceLevel level, Action<TraceRecord> traceAction);
    }
}
