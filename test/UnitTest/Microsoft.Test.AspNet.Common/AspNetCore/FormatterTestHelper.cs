// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.Test.AspNet.OData
{
    internal static class FormatterTestHelper
    {
        internal static ODataOutputFormatter GetFormatter(ODataPayloadKind[] payload, HttpRequest request)
        {
            // request is not needed on AspNetCore.
            ODataOutputFormatter formatter;
            formatter = new ODataOutputFormatter(payload);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataMinimalMetadata);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationXml);
            formatter.SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
            formatter.SupportedEncodings.Add(new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true));

            return formatter;
        }

        internal static ObjectResult GetContent<T>(T content, ODataOutputFormatter formatter, string mediaType)
        {
            ObjectResult objectResult = new ObjectResult(content);
            objectResult.Formatters.Add(formatter);
            objectResult.ContentTypes.Add(mediaType);

            return objectResult;
        }

        internal static async Task<string> GetContentResult(ObjectResult content, HttpRequest request)
        {
            var objectType = content.DeclaredType;
            if (objectType == null || objectType == typeof(object))
            {
                objectType = content.Value?.GetType();
            }

            MemoryStream ms = new MemoryStream();
            request.HttpContext.Response.Body = ms;

            var formatterContext = new OutputFormatterWriteContext(
                request.HttpContext,
                request.HttpContext.RequestServices.GetRequiredService<IHttpResponseStreamWriterFactory>().CreateWriter,
                objectType,
                content.Value);

            await content.Formatters[0].WriteAsync(formatterContext);

            ms.Flush();
            ms.Position = 0;
            StreamReader reader = new StreamReader(request.HttpContext.Response.Body);
            return reader.ReadToEnd();
        }

        internal static IHeaderDictionary GetContentHeaders(string contentType = null)
        {
            IHeaderDictionary headers = RequestFactory.Create().Headers;
            if (!string.IsNullOrEmpty(contentType))
            {
                headers["Content-Type"] = contentType;
            }

            return headers;
        }
    }
}
