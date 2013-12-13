// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.OData.Edm;

namespace System.Web.Http.OData
{
    internal static class HttpRouteCollectionExtensions
    {
        public static void MapODataRoute(this HttpRouteCollection routes, IEdmModel model)
        {
            routes.MapODataRoute("IgnoredRouteName", null, model);
        }
    }
}
