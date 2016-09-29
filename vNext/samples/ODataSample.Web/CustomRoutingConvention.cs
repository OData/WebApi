// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.OData.Routing.Conventions;

namespace ODataSample.Web
{
    public class CustomRoutingConvention : IODataRoutingConvention
    {
        public ActionDescriptor SelectAction(RouteContext routeContext)
        {
            Console.WriteLine("In CustomRoutingConvention !");
            return null;
        }
    }
}
