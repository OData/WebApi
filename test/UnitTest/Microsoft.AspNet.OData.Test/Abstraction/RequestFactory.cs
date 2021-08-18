//-----------------------------------------------------------------------------
// <copyright file="RequestFactory.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Test.Abstraction
{
    /// <summary>
    /// A class to create HttpRequest[Message].
    /// </summary>
    public class RequestFactory
    {
        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
        public static HttpRequestMessage Create(HttpConfiguration config = null, string routeName = null)
        {
            var request = new HttpRequestMessage();

            if (config != null)
            {
                request.SetConfiguration(config);
            }

            if (!string.IsNullOrEmpty(routeName))
            {
                request.EnableODataDependencyInjectionSupport(routeName);
            }
            else
            {
                request.EnableODataDependencyInjectionSupport();
            }

            return request;
        }

        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
        public static HttpRequestMessage Create(HttpMethod method, string uri)
        {
            var request = new HttpRequestMessage(method, uri);
            request.EnableODataDependencyInjectionSupport();
            return request;
        }

        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
        public static HttpRequestMessage Create(HttpMethod method, string uri, HttpConfiguration config, string routeName = null, ODataPath path = null)
        {
            var request = new HttpRequestMessage(method, uri);
            request.SetConfiguration(config);

            if (!string.IsNullOrEmpty(routeName))
            {
                request.EnableODataDependencyInjectionSupport(routeName);
            }

            if (path != null)
            {
                request.ODataProperties().Path = path;
            }

            return request;
        }

        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
        public static HttpRequestMessage CreateFromModel(IEdmModel model, string uri = "http://localhost", string routeName = "Route", ODataPath path = null)
        {
            var configuration = RoutingConfigurationFactory.Create();
            configuration.MapODataServiceRoute(routeName, null, model);

            var request = RequestFactory.Create(HttpMethod.Get, uri, configuration, routeName);

            if (path != null)
            {
                request.ODataProperties().Path = path;
            }

            return request;
        }
    }
}
