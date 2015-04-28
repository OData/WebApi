using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Routing;

namespace WebStack.QA.Test.OData.Common.Controllers
{
    public static class ODataExtensions
    {
        public static TKey GetKeyValue<TKey>(this HttpConfiguration configuration, Uri uri)
        {
            IHttpRouteData data = configuration.Routes.GetRouteData(new HttpRequestMessage { RequestUri = uri });
            object oId = data.Values["Id"];

            // TODO: this needs to use ODataLib to convert from OData literal form into the appropriate primitive type instance.
            return (TKey)Convert.ChangeType(oId, typeof(TKey));
        }

        public static bool WantsResponseToIncludeUpdatedEntity(this HttpRequestMessage request)
        {
            // TODO: look at the prefer header and do the right thing.
            // see: http://www.odata.org/media/30002/OData.html#thepreferheader
            return false;
        }

        public static bool WantsResponseToExcludeCreatedEntity(this HttpRequestMessage request)
        {
            // TODO: look at the prefer header and do the right thing.
            // see: http://www.odata.org/media/30002/OData.html#thepreferheader
            return false;
        }
    }
}
