//-----------------------------------------------------------------------------
// <copyright file="FormatterTestHelper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.OData;
using Moq;

namespace Microsoft.AspNet.OData.Test.Abstraction
{
    internal static class FormatterTestHelper
    {
        internal static ODataMediaTypeFormatter GetFormatter(ODataPayloadKind[] payload, HttpRequestMessage request, string mediaType = null)
        {
            ODataMediaTypeFormatter formatter;
            formatter = new ODataMediaTypeFormatter(payload);
            if (mediaType == null)
            {
                formatter.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(ODataMediaTypes.ApplicationJsonODataMinimalMetadata));
                formatter.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(ODataMediaTypes.ApplicationXml));
            }
            else
            {
                formatter.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(mediaType));
            }

            request.GetConfiguration().Routes.MapFakeODataRoute();
            formatter.Request = request;
            return formatter;
        }

        internal static ObjectContent<T> GetContent<T>(T content, ODataMediaTypeFormatter formatter, string mediaType)
        {
            return new ObjectContent<T>(content, formatter, MediaTypeHeaderValue.Parse(mediaType));
        }

        internal static ObjectContent GetContent(object value, Type type, ODataMediaTypeFormatter formatter, string mediaType)
        {
            return new ObjectContent(type, value, formatter, MediaTypeHeaderValue.Parse(mediaType));
        }

        internal static Task<string> GetContentResult(ObjectContent content, HttpRequestMessage request)
        {
            // request is not needed on AspNet.
            return content.ReadAsStringAsync();
        }

        internal static HttpContentHeaders GetContentHeaders(string contentType = null)
        {
            var headers = new StringContent(String.Empty).Headers;
            if (!string.IsNullOrEmpty(contentType))
            {
                headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
            }

            return headers;
        }

        internal static ODataMediaTypeFormatter GetInputFormatter(ODataPayloadKind[] payload, HttpRequestMessage request, string mediaType = null)
        {
            ODataMediaTypeFormatter formatter = new ODataMediaTypeFormatter(new ODataPayloadKind[] { ODataPayloadKind.Property })
            {
                Request = request
            };
            request.GetConfiguration().Routes.MapFakeODataRoute();
            return formatter;
        }

        internal static async Task<object> ReadAsync(ODataMediaTypeFormatter formatter, string entity, Type valueType, HttpRequestMessage request, string mediaType)
        {
            using (StringContent content = new StringContent(entity))
            {
                content.Headers.ContentType = MediaTypeHeaderValue.Parse(mediaType);

                using (Stream stream = await content.ReadAsStreamAsync())
                {
                    return await formatter.ReadFromStreamAsync(valueType, stream, content,
                        new Mock<IFormatterLogger>().Object);
                }
            }
        }
    }
}
