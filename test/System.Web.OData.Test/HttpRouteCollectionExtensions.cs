// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http;
using Microsoft.OData.Edm;

namespace System.Web.OData.Extensions
{
    internal static class HttpRouteCollectionExtensions
    {
        public static void MapODataServiceRoute(this HttpRouteCollection routes, IEdmModel model)
        {
            routes.MapODataServiceRoute("IgnoredRouteName", null, model);
        }
    }
}
