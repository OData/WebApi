// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http.Controllers;

namespace System.Web.OData.Routing.Conventions
{
    class MockNavigationSourceRoutingConvention : NavigationSourceRoutingConvention
    {
        public override string SelectAction(ODataPath odataPath, HttpControllerContext controllerContext,
            ILookup<string, HttpActionDescriptor> actionMap)
        {
            return null;
        }
    }
}
