// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace WebStack.QA.Test.OData.Common
{
    public class LinkRoutingConvention2 : EntitySetRoutingConvention
    {
        public override string SelectAction(ODataPath odataPath, HttpControllerContext controllerContext, ILookup<string, HttpActionDescriptor> actionMap)
        {
            if (odataPath == null)
            {
                throw new ArgumentNullException("odataPath");
            }
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }
            if (actionMap == null)
            {
                throw new ArgumentNullException("actionMap");
            }
            HttpMethod requestMethod = controllerContext.Request.Method;
            if (odataPath.PathTemplate == "~/entityset/key/navigation/$ref"
                || odataPath.PathTemplate == "~/entityset/key/cast/navigation/$ref"
                || odataPath.PathTemplate == "~/entityset/key/navigation/key/$ref"
                || odataPath.PathTemplate == "~/entityset/key/cast/navigation/key/$ref")
            {
                var actionName = string.Empty;
                if ((requestMethod == HttpMethod.Post) || (requestMethod == HttpMethod.Put))
                {
                    actionName += "CreateRefTo";
                }
                else if (requestMethod == HttpMethod.Delete)
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
                    if (actionMap.Contains(actionCastName))
                    {
                        AddLinkInfoToRouteData(controllerContext, odataPath);
                        return actionCastName;
                    }
                }

                if (actionMap.Contains(actionName))
                {
                    AddLinkInfoToRouteData(controllerContext, odataPath);
                    return actionName;
                }
            }
            return null;
        }

        private static void AddLinkInfoToRouteData(HttpControllerContext controllerContext, ODataPath odataPath)
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
