using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.OData.Extensions
{
    public static class HttpContextExtensions
    {
        public static ODataProperties ODataProperties(this HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw Error.ArgumentNull("httpContext");
            }

            return httpContext.RequestServices.GetRequiredService<ODataProperties>();
        }

        public static IUrlHelper UrlHelper(this HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw Error.ArgumentNull("httpContext");
            }

            return httpContext.RequestServices.GetRequiredService<IUrlHelper>();
        }
    }
}