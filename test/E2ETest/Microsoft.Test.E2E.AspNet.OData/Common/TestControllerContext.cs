//-----------------------------------------------------------------------------
// <copyright file="TestControllerContext.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData;
using Microsoft.OData.UriParser;
#else
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Controllers;
using Microsoft.OData;
using Microsoft.OData.UriParser;

#endif

namespace Microsoft.Test.E2E.AspNet.OData.Common
{
    /// <summary>
    /// Adapter class to convert RouteContext/HttpControllerContext to RouteData.
    /// </summary>
    public class TestControllerContext
    {
#if NETCORE
        private RouteContext innerContext;

        public TestControllerContext(RouteContext routeContext)
        {
            innerContext = routeContext;
        }
#else
        private HttpControllerContext innerContext;

        public TestControllerContext(HttpControllerContext controllerContext)
        {
            innerContext = controllerContext;
        }
#endif
        /// <summary>
        /// Add a key value to the route data.
        /// </summary>

        public void AddKeyValueToRouteData(KeySegment segment, string keyName = "key")
        {
            foreach (var keyValuePair in segment.Keys)
            {
                object value = keyValuePair.Value;
                ConstantNode node = value as ConstantNode;
                if (node != null)
                {
                    ODataEnumValue enumValue = node.Value as ODataEnumValue;
                    if (enumValue != null)
                    {
                        value = ODataUriUtils.ConvertToUriLiteral(enumValue, ODataVersion.V4);
                    }
                }

                if (segment.Keys.Count() == 1)
                {
                    RouteData[keyName] = value;
                }
                else
                {
                    RouteData[keyValuePair.Key] = value;
                }
            }
        }

        /// <summary>
        /// Gets the route data.
        /// </summary>
        public IDictionary<string, object> RouteData
        {
            get { return this.innerContext.RouteData.Values; }
        }
    }
}
