// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Test.Formatter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
#if NETCOREAPP2_0
    using Microsoft.AspNetCore.Mvc.Internal;
#endif
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Test.Abstraction
{
    internal static class FormatterTestHelper
    {
        internal static ODataOutputFormatter GetFormatter(ODataPayloadKind[] payload, HttpRequest request, string mediaType = null)
        {
            // request is not needed on AspNetCore.
            ODataOutputFormatter formatter;
            formatter = new ODataOutputFormatter(payload);
            formatter.SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
            formatter.SupportedEncodings.Add(new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true));

            if (mediaType != null)
            {
                formatter.SupportedMediaTypes.Add(mediaType);
            }
            else
            {
                formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataMinimalMetadata);
                formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationXml);
            }
            return formatter;
        }

        internal static ObjectResult GetContent<T>(T content, ODataOutputFormatter formatter, string mediaType)
        {
            ObjectResult objectResult = new ObjectResult(content);
            objectResult.Formatters.Add(formatter);
            objectResult.ContentTypes.Add(mediaType);

            return objectResult;
        }

        internal static ObjectResult GetContent(object content, Type type, ODataOutputFormatter formatter, string mediaType)
        {
            ObjectResult objectResult = new ObjectResult(content);
            objectResult.DeclaredType = type;
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
                //request.HttpContext.RequestServices.GetRequiredService<IHttpResponseStreamWriterFactory>().CreateWriter,
                new ODataMediaTypeFormattersTests.TestHttpResponseStreamWriterFactory().CreateWriter,
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

        internal static ODataInputFormatter GetInputFormatter(ODataPayloadKind[] payload, HttpRequest request, string mediaType = null)
        {
            ODataInputFormatter inputFormatter = new ODataInputFormatter(payload);
            inputFormatter.SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
            inputFormatter.SupportedEncodings.Add(new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true));
            return inputFormatter;
        }

        internal static async Task<object> ReadAsync(ODataInputFormatter formatter, string entity, Type valueType, HttpRequest request, string mediaType)
        {
            StringContent content = new StringContent(entity);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse(mediaType);

            Stream stream = await content.ReadAsStreamAsync();

            Func<Stream, Encoding, TextReader> readerFactor = (s, e) =>
            {
                return new StreamReader(stream);
            };

            request.Body = stream;

            ModelStateDictionary modelState = new ModelStateDictionary();
            IModelMetadataProvider provider = request.HttpContext.RequestServices.GetService<IModelMetadataProvider>();
            ModelMetadata metaData = provider.GetMetadataForType(valueType);
            InputFormatterContext context = new InputFormatterContext(request.HttpContext, "Any", modelState, metaData, readerFactor);

            InputFormatterResult result = await formatter.ReadAsync(context);
            return result.Model;
        }
    }
}
