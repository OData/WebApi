﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.SxS2.ODataV3.Extensions;

namespace Microsoft.Test.E2E.AspNet.OData.SxS2.ODataV3
{
    public static class ODataV3WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            var odataRoute = config.Routes.MapODataServiceRoute(
                routeName: "SxSODataV3",
                routePrefix: "SxSOData",
                model: Microsoft.Test.E2E.AspNet.OData.SxS2.ODataV3.Models.ModelBuilder.GetEdmModel());

            var contraint = new ODataVersionRouteConstraint(new List<string>() { "OData-Version" });
            odataRoute.Constraints.Add("VersionContraintV1", contraint);
        }
    }
}
