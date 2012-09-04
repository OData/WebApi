// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.Tracing
{
    /// <summary>
    /// Describes the kind of <see cref="TraceRecord"/> for an individual trace operation.
    /// </summary>
    public enum TraceKind
    {
        /// <summary>
        /// Single trace, not part of a Begin/End trace pair
        /// </summary>
        Trace = 0,

        /// <summary>
        /// Trace marking the beginning of some operation.
        /// </summary>
        Begin,

        /// <summary>
        /// Trace marking the end of some operation.
        /// </summary>
        End,
    }
}
