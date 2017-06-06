using System.Net.Http;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Nuwa;
using Xunit;

namespace WebStack.QA.Test.OData.QueryComposition.IsOf
{
    public class BillingCustomersController : ODataController
    {
        [EnableQuery]
        public IHttpActionResult Get()
        {
            object hostType;
            if (Request.GetConfiguration().Properties.TryGetValue("Nuwa.HostType", out hostType)
                && ((HostType)hostType) == HostType.KatanaSelf
                && RoutePrefixHelper.GetRoutePrefix(Request) == "EF")
            {
                return Ok(IsofDataSource.EfCustomers);
            }
            else
            {
                return Ok(IsofDataSource.InMemoryCustomers);
            }
        }
    }

    public class BillingsController : ODataController
    {
        [EnableQuery]
        public IHttpActionResult Get()
        {
            object hostType;
            if (Request.GetConfiguration().Properties.TryGetValue("Nuwa.HostType", out hostType)
                && ((HostType)hostType) == HostType.KatanaSelf
                && RoutePrefixHelper.GetRoutePrefix(Request) == "EF")
            {
                return Ok(IsofDataSource.EfBillings);
            }
            else
            {
                return Ok(IsofDataSource.InMemoryBillings);
            }
        }
    }

    public static class RoutePrefixHelper
    {
        public static string GetRoutePrefix(HttpRequestMessage request)
        {
            ODataRoute oDataRoute = request.GetRouteData().Route as ODataRoute;
            Assert.NotNull(oDataRoute);

            return oDataRoute.RoutePrefix;
        }
    }
}
