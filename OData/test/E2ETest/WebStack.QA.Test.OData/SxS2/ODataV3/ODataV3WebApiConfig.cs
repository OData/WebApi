using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.OData.Extensions;
using WebStack.QA.Test.OData.SxS2.ODataV3.Extensions;

namespace WebStack.QA.Test.OData.SxS2.ODataV3
{
    public static class ODataV3WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            var odataRoute = config.Routes.MapODataServiceRoute(
                routeName: "SxSODataV3",
                routePrefix: "SxSOData",
                model: WebStack.QA.Test.OData.SxS2.ODataV3.Models.ModelBuilder.GetEdmModel())
                .SetODataVersionConstraint(true);

            var contraint = new ODataVersionRouteConstraint(new List<string>() { "OData-Version" });
            odataRoute.Constraints.Add("VersionContraintV1", contraint);
        }
    }
}
