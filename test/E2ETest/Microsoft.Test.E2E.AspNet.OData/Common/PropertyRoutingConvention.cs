// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http.Controllers;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.Test.E2E.AspNet.OData.Common
{
    public class PropertyRoutingConvention : EntitySetRoutingConvention
    {
        public override string SelectAction(ODataPath odataPath, HttpControllerContext controllerContext, ILookup<string, HttpActionDescriptor> actionMap)
        {
            if (odataPath.PathTemplate == "~/entityset/key/property" || odataPath.PathTemplate == "~/entityset/key/cast/property")
            {
                var segment = odataPath.Segments.Last() as PropertySegment;
                var property = segment.Property;
                var declareType = property.DeclaringType as IEdmEntityType;
                if (declareType != null)
                {
                    var key = odataPath.Segments[1] as KeySegment;
                    controllerContext.AddKeyValueToRouteData(key);
                    string prefix = ODataHelper.GetHttpPrefix(controllerContext.Request.Method.ToString());
                    if (string.IsNullOrEmpty(prefix))
                    {
                        return null;
                    }
                    string action = prefix + property.Name + "From" + declareType.Name;
                    return actionMap.Contains(action) ? action : prefix + property.Name;
                }
            }

            return null;
        }
    }
}
