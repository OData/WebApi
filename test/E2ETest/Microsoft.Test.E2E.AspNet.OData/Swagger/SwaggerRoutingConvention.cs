// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;

namespace Microsoft.Test.E2E.AspNet.OData.Swagger
{
    public class SwaggerRoutingConvention : IODataRoutingConvention
    {
        public string SelectController(ODataPath odataPath, HttpRequestMessage request)
        {
            if (odataPath != null && odataPath.PathTemplate == "~/$swagger")
            {
                return "Swagger";
            }

            return null;
        }

        public string SelectAction(ODataPath odataPath, HttpControllerContext controllerContext, ILookup<string, HttpActionDescriptor> actionMap)
        {
            if (odataPath != null && odataPath.PathTemplate == "~/$swagger")
            {
                return "GetSwagger";
            }

            return null;
        }
    }
}
