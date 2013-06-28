// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.Routing
{
    /// <summary>
    /// Provides keys for looking up route values and data tokens.
    /// </summary>
    internal static class RouteKeys
    {
        // Used to provide the action and controller name in the route values
        public const string ActionKey = "action";
        public const string ControllerKey = "controller";

        // Used to provide the action descriptors to consider in the route data tokens
        public const string ActionsDataTokenKey = "actions";
    }
}