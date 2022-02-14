//-----------------------------------------------------------------------------
// <copyright file="ResponseFactory.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.IO;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNet.OData.Test.Abstraction
{
    /// <summary>
    /// A class to create HttpResponse[Message].
    /// </summary>
    public class ResponseFactory
    {
        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
        public static HttpResponse Create(HttpStatusCode statusCode, string content = null)
        {
            // Add the options services.
            IRouteBuilder config = RoutingConfigurationFactory.CreateWithRootContainer("OData");

            // Create a new context and assign the services.
            HttpContext context = new DefaultHttpContext();
            context.RequestServices = config.ServiceProvider;
            //context.ODataFeature().RequestContainer = provider;

            // Get response and return it.
            HttpResponse response = context.Response;
            response.StatusCode = (int)statusCode;

            // Add content
            if (!string.IsNullOrEmpty(content))
            {
                byte[] byteArray = Encoding.ASCII.GetBytes(content);
                using (MemoryStream contentStream = new MemoryStream(byteArray))
                {
                    contentStream.CopyTo(response.Body);
                }
            }

            return response;
        }
    }
}
