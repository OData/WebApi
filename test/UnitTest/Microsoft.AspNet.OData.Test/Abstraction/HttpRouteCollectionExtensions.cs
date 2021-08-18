//-----------------------------------------------------------------------------
// <copyright file="HttpRouteCollectionExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Routing;
using Moq;

namespace Microsoft.AspNet.OData.Test.Abstraction
{
    internal static class HttpRouteCollectionExtensions
    {
        public static string RouteName
        {
            get
            {
                return "OData";
            }
        }

        public static void MapFakeODataRoute(this HttpRouteCollection routes)
        {
            Mock<IHttpRoute> mockRoute = new Mock<IHttpRoute>();
            Mock<IHttpVirtualPathData> mockVirtualPath = new Mock<IHttpVirtualPathData>();
            mockVirtualPath.Setup(p => p.Route).Returns(mockRoute.Object);
            mockRoute.Setup(r => r.RouteTemplate).Returns("http://localhost");
            mockRoute.Setup(v => v.GetVirtualPath(
                It.IsAny<HttpRequestMessage>(), It.IsAny<Dictionary<string, object>>())).Returns(
                mockVirtualPath.Object);
            routes.Add(RouteName, mockRoute.Object);
        }
    }
}
