// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Extensions
{
    internal static class HttpRouteCollectionExtensions
    {
        public static void MapODataServiceRoute(this HttpRouteCollection routes, IEdmModel model)
        {
            routes.MapODataServiceRoute("IgnoredRouteName", null, model);
        }
    }
}
