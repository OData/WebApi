// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;

namespace System.Web.OData
{
    /// <summary>
    /// Content negotiator that uses per-request formatters to run the content negotiation.
    /// </summary>
    internal class PerRequestContentNegotiator : IContentNegotiator
    {
        private IContentNegotiator _innerContentNegotiator;

        public PerRequestContentNegotiator(IContentNegotiator innerContentNegotiator)
        {
            if (innerContentNegotiator == null)
            {
                throw Error.ArgumentNull("innerContentNegotiator");
            }

            _innerContentNegotiator = innerContentNegotiator;
        }

        public ContentNegotiationResult Negotiate(Type type, HttpRequestMessage request, IEnumerable<MediaTypeFormatter> formatters)
        {
            MediaTypeHeaderValue mediaType = request.Content == null ? null : request.Content.Headers.ContentType;

            List<MediaTypeFormatter> perRequestFormatters = new List<MediaTypeFormatter>();
            foreach (MediaTypeFormatter formatter in formatters)
            {
                if (formatter != null)
                {
                    perRequestFormatters.Add(formatter.GetPerRequestFormatterInstance(type, request, mediaType));
                }
            }
            return _innerContentNegotiator.Negotiate(type, request, perRequestFormatters);
        }
    }
}
