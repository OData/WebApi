// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
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
#else
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.OData;
using Microsoft.OData.Edm;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;
#endif

namespace Microsoft.Test.AspNet.OData.Formatter
{
    internal static class FormatterTestHelper
    {
#if NETCORE
        internal static ODataOutputFormatter GetFormatter(
            ODataPayloadKind[] payload,
            HttpRequest request,
            IEdmModel model = null,
            string routeName = null,
            ODataPath path = null)
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

            var formatterContext = new OutputFormatterWriteContext(
                request.HttpContext,
                request.HttpContext.RequestServices.GetRequiredService<IHttpResponseStreamWriterFactory>().CreateWriter,
                objectType,
                content.Value);

            await content.Formatters[0].WriteAsync(formatterContext);

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
#else
        internal static ODataMediaTypeFormatter GetFormatter(
            ODataPayloadKind[] payload,
            HttpRequestMessage request,
            IEdmModel model = null,
            string routeName = null,
            ODataPath path = null)
        {
            ODataMediaTypeFormatter formatter;
            formatter = new ODataMediaTypeFormatter(payload);
            formatter.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(ODataMediaTypes.ApplicationJsonODataMinimalMetadata));
            formatter.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(ODataMediaTypes.ApplicationXml));

            if (model != null && routeName != null)
            {
                request.GetConfiguration().MapODataServiceRoute(routeName, null, model);
                request.EnableODataDependencyInjectionSupport(routeName);
            }
            else if (routeName != null)
            {
                request.EnableODataDependencyInjectionSupport(routeName);
            }
            else if (model != null)
            {
                request.GetConfiguration().EnableODataDependencyInjectionSupport(HttpRouteCollectionExtensions.RouteName, model);
                request.EnableODataDependencyInjectionSupport(model);
                request.GetConfiguration().Routes.MapFakeODataRoute();
            }
            else
            {
                request.GetConfiguration().EnableODataDependencyInjectionSupport(HttpRouteCollectionExtensions.RouteName);
                request.EnableODataDependencyInjectionSupport();
                request.GetConfiguration().Routes.MapFakeODataRoute();
            }

            if (path != null)
            {
                request.ODataProperties().Path = path;
            }

            formatter.Request = request;
            return formatter;
        }

        internal static ObjectContent<T> GetContent<T>(T content, ODataMediaTypeFormatter formatter, string mediaType)
        {
            return new ObjectContent<T>(content, formatter, MediaTypeHeaderValue.Parse(mediaType));
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
#endif
    }
}
