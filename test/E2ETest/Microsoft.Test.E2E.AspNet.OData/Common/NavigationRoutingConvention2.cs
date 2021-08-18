//-----------------------------------------------------------------------------
// <copyright file="NavigationRoutingConvention2.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.Test.E2E.AspNet.OData.Common
{
    public class NavigationRoutingConvention2 : TestEntitySetRoutingConvention
    {
        /// <inheritdoc/>
        protected override string SelectAction(string requestMethod, ODataPath odataPath, TestControllerContext controllerContext, IList<string> actionList)
        {
            if ((odataPath.PathTemplate == "~/entityset/key/navigation") || (odataPath.PathTemplate == "~/entityset/key/cast/navigation"))
            {
                NavigationPropertySegment segment = odataPath.Segments.Last<ODataPathSegment>() as NavigationPropertySegment;
                IEdmNavigationProperty navigationProperty = segment.NavigationProperty;
                IEdmEntityType declaringType = navigationProperty.DeclaringType as IEdmEntityType;
                if (declaringType != null)
                {
                    string prefix = ODataHelper.GetHttpPrefix(requestMethod);
                    if (string.IsNullOrEmpty(prefix))
                    {
                        return null;
                    }
                    KeySegment segment2 = odataPath.Segments[1] as KeySegment;
                    controllerContext.AddKeyValueToRouteData(segment2);
                    string key = prefix + navigationProperty.Name + "On" + declaringType.Name;
                    return (actionList.Contains(key) ? key : (prefix + navigationProperty.Name));
                }
            }
            return null;
        }
    }
}
