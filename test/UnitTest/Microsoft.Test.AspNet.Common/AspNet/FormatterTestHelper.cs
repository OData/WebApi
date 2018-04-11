// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Routing;
using Microsoft.OData;
using Microsoft.OData.Edm;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.Test.AspNet.OData
{
    internal static class FormatterTestHelper
    {
        internal static ODataMediaTypeFormatter GetFormatter(ODataPayloadKind[] payload, HttpRequestMessage request,
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
                if (!request.GetConfiguration().Routes.Any(r =>
                    {
                        ODataRoute route = r as ODataRoute;
                        return route == null ? false : route.PathRouteConstraint.RouteName == routeName;
                    }))
                {
                    request.GetConfiguration().MapODataServiceRoute(routeName, null, model);
                    request.EnableODataDependencyInjectionSupport(routeName);
                }
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
    }
}
