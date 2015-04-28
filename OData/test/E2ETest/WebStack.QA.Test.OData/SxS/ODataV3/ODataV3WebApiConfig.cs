using System.Web.Http;
using System.Web.Http.OData.Extensions;
using WebStack.QA.Test.OData.SxS.ODataV3.Extensions;

namespace WebStack.QA.Test.OData.SxS.ODataV3
{
    public static class ODataV3WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            var odataRoute = config.Routes.MapODataServiceRoute(
                routeName: "SxSODataV3",
                routePrefix: "SxSOData",
                model: WebStack.QA.Test.OData.SxS.ODataV3.Models.ModelBuilder.GetEdmModel());

            var contraint = new ODataVersionRouteConstraint(new { v = "1" });
            odataRoute.Constraints.Add("VersionContraintV1", contraint);
        }
    }
}
