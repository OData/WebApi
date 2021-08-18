//-----------------------------------------------------------------------------
// <copyright file="HttpControllerContextExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http.Controllers;
using Microsoft.OData;
using Microsoft.OData.UriParser;

namespace Microsoft.Test.E2E.AspNet.OData.Common
{
    public static class HttpControllerContextExtensions
    {
        public static void AddKeyValueToRouteData(this HttpControllerContext controllerContext, KeySegment segment, string keyName = "key")
        {
            Contract.Assert(controllerContext != null);
            Contract.Assert(segment != null);

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
                    controllerContext.RouteData.Values[keyName] = value;
                }
                else
                {
                    controllerContext.RouteData.Values[keyValuePair.Key] = value;
                }
            }
        }
    }
}
