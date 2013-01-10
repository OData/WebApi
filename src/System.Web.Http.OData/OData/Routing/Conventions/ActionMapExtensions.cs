// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http.Controllers;

namespace System.Web.Http.OData.Routing.Conventions
{
    /// <summary>
    /// Provides helper methods for querying an action map.
    /// </summary>
    internal static class ActionMapExtensions
    {
        public static string FindMatchingAction(this ILookup<string, HttpActionDescriptor> actionMap, string targetActionName, string fallbackActionName)
        {
            if (actionMap.Contains(targetActionName))
            {
                return targetActionName;
            }
            else if (actionMap.Contains(fallbackActionName))
            {
                return fallbackActionName;
            }

            return null;
        }
    }
}