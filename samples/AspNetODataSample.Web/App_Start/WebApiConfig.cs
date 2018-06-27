// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Web.Http;
using AspNetODataSample.Web.Models;
using Microsoft.AspNet.OData.Extensions;

namespace AspNetODataSample.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            var model = EdmModelBuilder.GetEdmModel();
            config.MapODataServiceRoute("odata", "odata", model);
        }
    }
}
