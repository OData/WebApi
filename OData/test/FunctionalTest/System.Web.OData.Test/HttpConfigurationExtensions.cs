// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
