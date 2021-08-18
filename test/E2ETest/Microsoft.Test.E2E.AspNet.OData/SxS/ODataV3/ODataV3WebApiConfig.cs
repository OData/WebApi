//-----------------------------------------------------------------------------
// <copyright file="ODataV3WebApiConfig.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Web.Http;
using System.Web.Http.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.SxS.ODataV3.Extensions;

namespace Microsoft.Test.E2E.AspNet.OData.SxS.ODataV3
{
    public static class ODataV3WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            var odataRoute = config.Routes.MapODataServiceRoute(
                routeName: "SxSODataV3",
                routePrefix: "SxSOData",
                model: Microsoft.Test.E2E.AspNet.OData.SxS.ODataV3.Models.ModelBuilder.GetEdmModel());

            var contraint = new ODataVersionRouteConstraint(new { v = "1" });
            odataRoute.Constraints.Add("VersionContraintV1", contraint);
        }
    }
}
