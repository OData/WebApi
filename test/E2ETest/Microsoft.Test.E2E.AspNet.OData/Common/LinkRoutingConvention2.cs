//-----------------------------------------------------------------------------
// <copyright file="LinkRoutingConvention2.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.Test.E2E.AspNet.OData.Common
{
    public class LinkRoutingConvention2 : TestEntitySetRoutingConvention
    {
        /// <inheritdoc/>
        protected override string SelectAction(string requestMethod, ODataPath odataPath, TestControllerContext controllerContext, IList<string> actionList)
        {
            if (odataPath.PathTemplate == "~/entityset/key/navigation/$ref"
                || odataPath.PathTemplate == "~/entityset/key/cast/navigation/$ref"
                || odataPath.PathTemplate == "~/entityset/key/navigation/key/$ref"
                || odataPath.PathTemplate == "~/entityset/key/cast/navigation/key/$ref")
            {
                var actionName = string.Empty;
                if ((requestMethod == "POST") || (requestMethod == "PUT"))
                {
                    actionName += "CreateRefTo";
                }
                else if (requestMethod == "DELETE")
                {
                    actionName += "DeleteRefTo";
                }
                else
                {
                    return null;
                }
                var navigationSegment = odataPath.Segments.OfType<NavigationPropertyLinkSegment>().Last();
                actionName += navigationSegment.NavigationProperty.Name;

                var castSegment = odataPath.Segments[2] as TypeSegment;
                
                if (castSegment != null)
                {
                    IEdmType elementType = castSegment.EdmType;
                    if (castSegment.EdmType.TypeKind == EdmTypeKind.Collection)
                    {
                        elementType = ((IEdmCollectionType)castSegment.EdmType).ElementType.Definition;
                    }

                    var actionCastName = string.Format("{0}On{1}", actionName, ((IEdmEntityType)elementType).Name);
                    if (actionList.Contains(actionCastName))
                    {
                        AddLinkInfoToRouteData(controllerContext, odataPath);
                        return actionCastName;
                    }
                }

                if (actionList.Contains(actionName))
                {
                    AddLinkInfoToRouteData(controllerContext, odataPath);
                    return actionName;
                }
            }
            return null;
        }

        private static void AddLinkInfoToRouteData(TestControllerContext controllerContext, ODataPath odataPath)
        {
            KeySegment keyValueSegment = odataPath.Segments.OfType<KeySegment>().First();
            controllerContext.AddKeyValueToRouteData(keyValueSegment);

            KeySegment relatedKeySegment = odataPath.Segments.Last() as KeySegment;
            if (relatedKeySegment != null)
            {
                controllerContext.AddKeyValueToRouteData(relatedKeySegment, ODataRouteConstants.RelatedKey);
            }
        }
    }
}
