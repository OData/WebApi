//-----------------------------------------------------------------------------
// <copyright file="HttpRequestMessageExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;

namespace Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight.Metadata.Extensions
{
    internal static class HttpRequestMessageExtensions
    {
        public static HttpResponseMessage CreateResponse(this HttpRequestMessage request, HttpStatusCode statusCode, object value, Type type)
        {
            var configuration = request.GetConfiguration();
            IContentNegotiator contentNegotiator = configuration.Services.GetContentNegotiator();
            IEnumerable<MediaTypeFormatter> formatters = configuration.Formatters;

            // Run content negotiation
            ContentNegotiationResult result = contentNegotiator.Negotiate(type, request, formatters);

            if (result == null)
            {
                // no result from content negotiation indicates that 406 should be sent.
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotAcceptable,
                    RequestMessage = request,
                };
            }
            else
            {
                MediaTypeHeaderValue mediaType = result.MediaType;
                return new HttpResponseMessage
                {
                    // At this point mediaType should be a cloned value (the content negotiator is responsible for returning a new copy)
                    Content = new ObjectContent(type, value, result.Formatter, mediaType),
                    StatusCode = statusCode,
                    RequestMessage = request
                };
            }
        }
    }
}
