// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Controllers;

namespace System.Web.Http.Routing
{
    public interface IHttpRouteProvider : IActionHttpMethodProvider
    {
        string RouteName { get; }
        string RouteTemplate { get; }
    }
}
