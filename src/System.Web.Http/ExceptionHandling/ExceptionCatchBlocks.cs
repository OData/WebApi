// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Batch;
using System.Web.Http.Dispatcher;
using System.Web.Http.Filters;

namespace System.Web.Http.ExceptionHandling
{
    /// <summary>Provides labels for catch blocks used within this assembly.</summary>
    public static class ExceptionCatchBlocks
    {
        /// <summary>Gets the label for the catch block in <see cref="HttpBatchHandler"/>.SendAsync.</summary>
        public static string HttpBatchHandler
        {
            get { return typeof(HttpBatchHandler).Name; }
        }

        /// <summary>Gets the label for the catch block in <see cref="HttpControllerDispatcher"/>.SendAsync.</summary>
        public static string HttpControllerDispatcher
        {
            get { return typeof(HttpControllerDispatcher).Name; }
        }

        /// <summary>Gets the label for the catch block in <see cref="HttpServer"/>.SendAsync</summary>
        public static string HttpServer
        {
            get { return typeof(HttpServer).Name; }
        }

        /// <summary>
        /// Gets the label for the catch block in <see cref="ApiController"/>.ExecuteAsync when using
        /// <see cref="IExceptionFilter"/>.
        /// </summary>
        public static string IExceptionFilter
        {
            get { return typeof(IExceptionFilter).Name; }
        }
    }
}
