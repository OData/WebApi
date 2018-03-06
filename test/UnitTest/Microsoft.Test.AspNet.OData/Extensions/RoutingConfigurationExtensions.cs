// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Test.AspNet.OData.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#else
using System.Web.Http;
using System.Web.Http.Routing;
#endif

namespace Microsoft.Test.AspNet.OData.Extensions
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
