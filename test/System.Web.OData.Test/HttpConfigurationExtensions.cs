// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http;
using Microsoft.OData.Edm;

namespace System.Web.OData.Extensions
{
    internal static class HttpConfigurationExtensions
    {
        public static void MapODataServiceRoute(this HttpConfiguration configuration, IEdmModel model)
        {
            configuration.MapODataServiceRoute("IgnoredRouteName", null, model);
        }
    }
}
