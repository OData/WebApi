//-----------------------------------------------------------------------------
// <copyright file="RoutingConfigurationExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
#else
using System.Web.Http;
using System.Web.Http.Routing;
#endif

namespace Microsoft.AspNet.OData.Test.Extensions
{
#if NETCORE
    /// <summary>
    /// Extensions for IRouteBuilder.
    /// </summary>
    public static class IRouteBuilderExtensions
    {
        /// <summary>
        /// Maps the specified route template and sets default route values.
        /// </summary>
        /// <param name="builder">The IRouteBuilder.</param>
        /// <param name="name">The name of the route to map.</param>
        /// <param name="routeTemplate">The route template for the route.</param>
        /// <param name="defaults">An object that contains default route values.</param>
        /// <returns> A reference to the IRouteBuilder.</returns>
        public static IRouteBuilder MapNonODataRoute(this IRouteBuilder builder, string name, string routeTemplate, object defaults)
        {
            return builder.MapRoute(name, routeTemplate, defaults);
        }
    }
#else
    /// <summary>
    /// Extensions for HttpConfiguration.
    /// </summary>
    public static class HttpConfigurationExtensions
    {
        /// <summary>
        /// Maps the specified route template and sets default route values.
        /// </summary>
        /// <param name="config">The HttpConfiguration.</param>
        /// <param name="name">The name of the route to map.</param>
        /// <param name="routeTemplate">The route template for the route.</param>
        /// <param name="defaults">An object that contains default route values.</param>
        /// <returns> A reference to the mapped route.</returns>
        public static IHttpRoute MapNonODataRoute(this HttpConfiguration config, string name, string routeTemplate, object defaults)
        {
            return config.Routes.MapHttpRoute(name, routeTemplate, defaults);
        }
    }
#endif
}
