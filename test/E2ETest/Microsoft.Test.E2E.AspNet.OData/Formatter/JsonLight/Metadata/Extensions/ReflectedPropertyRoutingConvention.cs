//-----------------------------------------------------------------------------
// <copyright file="ReflectedPropertyRoutingConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.Test.E2E.AspNet.OData.Common;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight.Metadata.Extensions
{
    public class ReflectedPropertyRoutingConvention : TestEntitySetRoutingConvention
    {
        /// <inheritdoc/>
        protected override string SelectAction(string requestMethod, ODataPath odataPath, TestControllerContext controllerContext, IList<string> actionList)
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
                    controllerContext.RouteData.Add("property", property.Name);
                    string prefix = ODataHelper.GetHttpPrefix(requestMethod);
                    if (string.IsNullOrEmpty(prefix))
                    {
                        return null;
                    }
                    string action = prefix + "Property" + "From" + declareType.Name;
                    return actionList.Contains(action) ? action : prefix + "Property";
                }
            }

            return null;
        }
    }
}
