// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http.Controllers;

namespace System.Web.OData.Routing.Conventions
{
    /// <summary>
    /// Provides helper methods for querying an action map.
    /// </summary>
    internal static class ActionMapExtensions
    {
        public static string FindMatchingAction(this ILookup<string, HttpActionDescriptor> actionMap, params string[] targetActionNames)
        {
            foreach (string targetActionName in targetActionNames)
            {
                if (actionMap.Contains(targetActionName))
                {
                    return targetActionName;
                }
            }

            return null;
        }
    }
}