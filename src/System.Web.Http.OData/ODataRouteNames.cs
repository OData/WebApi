// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http
{
    /// <summary>
    /// This class contains route names for some of the standard OData routes.
    /// </summary>
    public static class ODataRouteNames
    {
        /// <summary>
        /// Route name for OData $metadata.
        /// </summary>
        public static readonly string Metadata = "OData.$metadata";

        /// <summary>
        /// Route name for OData service document.
        /// </summary>
        public static readonly string ServiceDocument = "OData.servicedoc";

        /// <summary>
        /// Route name for route used for addressing first level properties on an entity.
        /// <remarks>The route template for this route should be '{controller}({parentId})/{navigationProperty}'.</remarks>
        /// </summary>
        public static readonly string PropertyNavigation = "OData.PropertyNavigation";

        /// <summary>
        /// Route name for route used for addressing first level properties on an entity and includes the cast segment.
        /// <remarks>The route template for this route should be '{controller}({parentId})/{entityType}/{navigationProperty}'.</remarks>
        /// </summary>
        public static readonly string PropertyNavigationWithCast = "OData.PropertyNavigationWithCast";

        /// <summary>
        /// Route name for route used for manipulating links, the code allows people to create and delete relationships between entities.
        /// </summary>
        public static readonly string Link = "OData.Link";

        /// <summary>
        /// Route name for route used for addressing entities by their keys.
        /// <remarks>The route template for this route should be '{controller}({id})'.</remarks>
        /// </summary>
        public static readonly string GetById = "OData.GetById";

        /// <summary>
        /// Route name for route used for addressing entities by their keys and includes the cast segment.
        /// <remarks>The route template for this route should be '{controller}({id})/{entityType}'.</remarks>
        /// </summary>
        public static readonly string GetByIdWithCast = "OData.GetByIdWithCast";

        /// <summary>
        /// Route name for the route used for addressing root level entity sets.
        /// </summary>
        public static readonly string Default = "OData.Default";

        /// <summary>
        /// Route name for the route used for addressing root level entity sets (with parentheses to support WCF dataservices client).
        /// </summary>
        public static readonly string DefaultWithParentheses = "OData.DefaultWithParentheses";

        /// <summary>
        /// Route name for the route used for addressing an action bound to an entity's editlink.
        /// <remarks>The route template for this route should be '{controller}({boundId})/{odataAction}'.</remarks>
        /// </summary>
        public static readonly string InvokeBoundAction = "OData.InvokeBoundAction";

        /// <summary>
        /// Route name for the route used for addressing an action bound to an entity's editlink and includes the cast segment.
        /// <remarks>The route template for this route should be '{controller}({boundId})/{entityType}/{odataAction}'.</remarks>
        /// </summary>
        public static readonly string InvokeBoundActionWithCast = "OData.InvokeBoundActionWithCast";
    }
}
