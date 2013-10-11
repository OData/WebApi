// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.WebHost.Routing;

namespace System.Web.Http.WebHost
{
    /// <summary>Provides labels for catch blocks used within this assembly.</summary>
    public static class WebHostExceptionCatchBlocks
    {
        /// <summary>
        /// Gets the label for the catch block in
        /// <see cref="HttpControllerHandler"/>.WriteBufferedResponseContentAsync.
        /// </summary>
        /// <remarks>
        /// This catch block handles exceptions when writing the HttpContent under an IHostBufferPolicySelector that
        /// buffers.
        /// </remarks>
        public static string HttpControllerHandlerWriteBufferedResponseContentAsync
        {
            get { return typeof(HttpControllerHandler).Name + ".WriteBufferedResponseContentAsync"; }
        }

        /// <summary>
        /// Gets the label for the catch block in <see cref="HttpControllerHandler"/>.WriteErrorResponseContentAsync.
        /// </summary>
        /// <remarks>
        /// This catch block handles exceptions when writing the HttpContent of the error response itself (after
        /// <see cref="HttpControllerHandlerWriteBufferedResponseContentAsync"/> or <see cref="HttpWebRoute"/>).
        /// </remarks>
        public static string HttpControllerHandlerWriteErrorResponseContentAsync
        {
            get { return typeof(HttpControllerHandler).Name + ".WriteErrorResponseContentAsync"; }
        }

        /// <summary>
        /// Gets the label for the catch block in
        /// <see cref="HttpControllerHandler"/>.WriteStreamedResponseContentAsync.
        /// </summary>
        /// <remarks>
        /// This catch block handles exceptions when writing the HttpContent under an IHostBufferPolicySelector that
        /// does not buffer.
        /// </remarks>
        public static string HttpControllerHandlerWriteStreamedResponseContentAsync
        {
            get { return typeof(HttpControllerHandler).Name + ".WriteStreamedResponseContentAsync"; }
        }

        /// <summary>Gets the label for the catch block in <see cref="HttpWebRoute"/>.GetRouteData.</summary>
        public static string HttpWebRoute
        {
            get { return typeof(HttpWebRoute).Name; }
        }
    }
}
