// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Properties;

namespace System.Web.Http.Hosting
{
    /// <summary>
    /// Initializing a <see cref="DelegatingHandler"/> pipeline.
    /// </summary>
    internal static class HttpPipelineFactory
    {
        /// <summary>
        /// Creates an instance of an <see cref="HttpMessageHandler"/> using the <see cref="DelegatingHandler"/> instances
        /// provided by <paramref name="handlers"/>.
        /// </summary>
        /// <param name="handlers">An ordered list of <see cref="DelegatingHandler"/> instances to be invoked as an 
        /// <see cref="HttpRequestMessage"/> travels up the stack and an <see cref="HttpResponseMessage"/> travels down.</param>
        /// <param name="innerChannel">The inner channel represents the destination of the HTTP message channel.</param>
        /// <returns>The HTTP message channel.</returns>
        public static HttpMessageHandler Create(IEnumerable<DelegatingHandler> handlers, HttpMessageHandler innerChannel)
        {
            if (innerChannel == null)
            {
                throw Error.ArgumentNull("innerChannel");
            }

            if (handlers == null)
            {
                return innerChannel;
            }

            // Wire handlers up
            HttpMessageHandler pipeline = innerChannel;
            foreach (DelegatingHandler handler in handlers)
            {
                if (handler == null)
                {
                    throw Error.Argument("handlers", SRResources.DelegatingHandlerArrayContainsNullItem, typeof(DelegatingHandler).Name);
                }

                if (handler.InnerHandler != null)
                {
                    throw Error.Argument("handlers", SRResources.DelegatingHandlerArrayHasNonNullInnerHandler, typeof(DelegatingHandler).Name, "InnerHandler", handler.GetType().Name);
                }

                handler.InnerHandler = pipeline;
                pipeline = handler;
            }

            return pipeline;
        }
    }
}
