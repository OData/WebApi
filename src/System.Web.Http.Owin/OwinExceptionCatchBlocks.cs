// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.ExceptionHandling;

namespace System.Web.Http.Owin
{
    /// <summary>Provides the catch blocks used within this assembly.</summary>
    public static class OwinExceptionCatchBlocks
    {
        private static readonly ExceptionContextCatchBlock _httpMessageHandlerAdapterBufferContent =
            new ExceptionContextCatchBlock(typeof(HttpMessageHandlerAdapter).Name + ".BufferContent",
                isTopLevel: true, callsHandler: true);
        private static readonly ExceptionContextCatchBlock _httpMessageHandlerAdapterBufferError =
            new ExceptionContextCatchBlock(typeof(HttpMessageHandlerAdapter).Name + ".BufferError", isTopLevel: true,
                callsHandler: false);
        private static readonly ExceptionContextCatchBlock _httpMessageHandlerAdapterStreamContent =
            new ExceptionContextCatchBlock(typeof(HttpMessageHandlerAdapter).Name + ".StreamContent",
                isTopLevel: true, callsHandler: false);

        /// <summary>Gets the catch block in <see cref="HttpMessageHandlerAdapter"/>.BufferContent.</summary>
        public static ExceptionContextCatchBlock HttpMessageHandlerAdapterBufferContent
        {
            get
            {
                return _httpMessageHandlerAdapterBufferContent;
            }
        }

        /// <summary>Gets the catch block in <see cref="HttpMessageHandlerAdapter"/>.BufferError.</summary>
        public static ExceptionContextCatchBlock HttpMessageHandlerAdapterBufferError
        {
            get
            {
                return _httpMessageHandlerAdapterBufferError;
            }
        }

        /// <summary>Gets the catch block in <see cref="HttpMessageHandlerAdapter"/>.StreamContent.</summary>
        public static ExceptionContextCatchBlock HttpMessageHandlerAdapterStreamContent
        {
            get
            {
                return _httpMessageHandlerAdapterStreamContent;
            }
        }
    }
}
