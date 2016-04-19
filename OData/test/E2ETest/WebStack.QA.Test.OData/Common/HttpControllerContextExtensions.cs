using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.OData.Extensions;
using Microsoft.OData.Client;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Core.UriParser.Semantic;

namespace WebStack.QA.Test.OData.Common
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
