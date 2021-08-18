//-----------------------------------------------------------------------------
// <copyright file="WebApiConfig.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
