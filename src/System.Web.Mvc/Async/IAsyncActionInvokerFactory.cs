// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Mvc.Async
{
    /// <summary>
    /// Used to create an <see cref="IAsyncActionInvoker"/> instance for the current request.
    /// </summary>
    public interface IAsyncActionInvokerFactory
    {
        /// <summary>
        /// Creates an instance of async action invoker for the current request.
        /// </summary>
        /// <returns>The created <see cref="IAsyncActionInvoker"/>.</returns>
        IAsyncActionInvoker CreateInstance();
    }
}
