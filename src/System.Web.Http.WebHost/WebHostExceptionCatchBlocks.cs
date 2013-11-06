// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.ExceptionHandling;
using System.Web.Http.WebHost.Routing;

namespace System.Web.Http.WebHost
{
    /// <summary>Provides the catch blocks used within this assembly.</summary>
    public static class WebHostExceptionCatchBlocks
    {
        private static readonly ExceptionContextCatchBlock _httpControllerHandlerBufferContent =
            new ExceptionContextCatchBlock(typeof(HttpControllerHandler).Name + ".BufferContent", isTopLevel: true,
                callsHandler: true);
        private static readonly ExceptionContextCatchBlock _httpControllerHandlerBufferError =
            new ExceptionContextCatchBlock(typeof(HttpControllerHandler).Name + ".BufferError", isTopLevel: true,
                callsHandler: false);
        private static readonly ExceptionContextCatchBlock _httpControllerHandlerStreamContent =
            new ExceptionContextCatchBlock(typeof(HttpControllerHandler).Name + ".StreamContent", isTopLevel: true,
                callsHandler: false);
        private static readonly ExceptionContextCatchBlock _httpWebRoute =
            new ExceptionContextCatchBlock(typeof(HttpWebRoute).Name, isTopLevel: true, callsHandler: true);

        /// <summary>
        /// Gets the label for the catch block in
        /// <see cref="HttpControllerHandler"/>.WriteBufferedResponseContentAsync.
        /// </summary>
        /// <remarks>
        /// This catch block handles exceptions when writing the HttpContent under an IHostBufferPolicySelector that
        /// buffers.
        /// </remarks>
        public static ExceptionContextCatchBlock HttpControllerHandlerBufferContent
        {
            get { return _httpControllerHandlerBufferContent; }
        }

        /// <summary>
        /// Gets the label for the catch block in <see cref="HttpControllerHandler"/>.WriteErrorResponseContentAsync.
        /// </summary>
        /// <remarks>
        /// This catch block handles exceptions when writing the HttpContent of the error response itself (after
        /// <see cref="HttpControllerHandlerBufferContent"/> or <see cref="HttpWebRoute"/>).
        /// </remarks>
        public static ExceptionContextCatchBlock HttpControllerHandlerBufferError
        {
            get { return _httpControllerHandlerBufferError; }
        }

        /// <summary>
        /// Gets the label for the catch block in
        /// <see cref="HttpControllerHandler"/>.WriteStreamedResponseContentAsync.
        /// </summary>
        /// <remarks>
        /// This catch block handles exceptions when writing the HttpContent under an IHostBufferPolicySelector that
        /// does not buffer.
        /// </remarks>
        public static ExceptionContextCatchBlock HttpControllerHandlerStreamContent
        {
            get { return _httpControllerHandlerStreamContent; }
        }

        /// <summary>Gets the label for the catch block in <see cref="HttpWebRoute"/>.GetRouteData.</summary>
        public static ExceptionContextCatchBlock HttpWebRoute
        {
            get { return _httpWebRoute; }
        }
    }
}
