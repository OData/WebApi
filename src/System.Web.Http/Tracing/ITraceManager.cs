// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.Tracing
{
    /// <summary>
    /// Interface to initialize the tracing layer.
    /// </summary>
    /// <remarks>
    /// This is an extensibility interface that may be inserted into
    /// <see cref="HttpConfiguration.Services"/> to provide a replacement for the
    /// entire tracing layer.
    /// </remarks>
    public interface ITraceManager
    {
        void Initialize(HttpConfiguration configuration);
    }
}
