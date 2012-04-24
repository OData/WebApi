// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Hosting;

namespace System.Web.Http.WebHost
{
    /// <summary>
    /// Provides an implementation of <see cref="IHostBufferPolicySelector"/> suited for use
    /// in an ASP.NET environment which provides direct support for input and output buffering.
    /// </summary>
    public class WebHostBufferPolicySelector : IHostBufferPolicySelector
    {
        public virtual bool UseBufferedOutputStream(HttpResponseMessage response)
        {
            if (response == null)
            {
                throw Error.ArgumentNull("response");
            }

            // Any HttpContent that knows its length is presumably already buffered internally.
            HttpContent content = response.Content;
            if (content != null)
            {
                long? contentLength = content.Headers.ContentLength;
                if (contentLength.HasValue && contentLength.Value >= 0)
                {
                    return false;
                }

                // Content length is null or -1 (meaning not known).  
                // Buffer any HttpContent except StreamContent and PushStreamContent
                return !(content is StreamContent || content is PushStreamContent);
            }

            return false;
        }
    }
}
