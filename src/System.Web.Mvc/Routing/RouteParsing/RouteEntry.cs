// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Routing;

namespace System.Web.Mvc.Routing
{
    /// <summary>
    /// Route information used to name and order routes for attribute routing. Applies ordering based on the prefix order
    /// and the attribute order first, then applies a default order that registers more specific routes earlier.
    /// </summary>
    internal class RouteEntry
    {
        public Route Route { get; set; }
        public string Name { get; set; }
        public string Template { get; set; }
    }
}
