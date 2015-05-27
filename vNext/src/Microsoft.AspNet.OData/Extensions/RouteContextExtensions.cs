using Microsoft.AspNet.Http;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.OData.Extensions
{
    public static class RouteContextExtensions
    {
        /// <summary>
        /// Gets the <see cref="ODataProperties"/> instance containing OData methods and properties
        /// for given <see cref="RouteContext"/>.
        /// </summary>
        /// <param name="routeContext">The route context of interest.</param>
        /// <returns>
        /// An object through which OData methods and properties for given <paramref name="routeContext"/> are available.
        /// </returns>
        public static ODataProperties ODataProperties(this RouteContext routeContext)
        {
            if (routeContext == null)
            {
                throw Error.ArgumentNull("routeContext");
            }

            return routeContext.HttpContext.RequestServices.GetRequiredService<ODataProperties>();
        }
    }
}