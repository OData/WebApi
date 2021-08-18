//-----------------------------------------------------------------------------
// <copyright file="ODataV4WebApiConfig.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Web.Http;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.Test.E2E.AspNet.OData.SxS2.ODataV4.Extensions;

namespace Microsoft.Test.E2E.AspNet.OData.SxS2.ODataV4
{
    public static class ODataV4WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            var model = Microsoft.Test.E2E.AspNet.OData.SxS2.ODataV4.Models.ModelBuilder.GetEdmModel();
            var conventions = ODataRoutingConventions.CreateDefaultWithAttributeRouting("SxSODataV4", config);

            var odataRoute = config.MapODataServiceRoute(
                routeName: "SxSODataV4",
                routePrefix: "SxSOData",
                model: model,
                pathHandler: new DefaultODataPathHandler(),
                routingConventions: conventions);

            var constraint = new ODataVersionRouteConstraint(new List<string>() { "DataServiceVersion" });
            odataRoute.Constraints.Add("VersionConstraintV2", constraint);
        }
    }
}
