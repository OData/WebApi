// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNet.OData.Routing
{
    /// <summary>
    /// This class contains route constants for OData.
    /// </summary>
    public static class ODataRouteConstants
    {
        /// <summary>
        /// Route variable name for the OData path.
        /// </summary>
        public static readonly string ODataPath = "odataPath";

        /// <summary>
        /// Wildcard route template for the OData path route variable.
        /// </summary>
        public static readonly string ODataPathTemplate = "{*" + ODataRouteConstants.ODataPath + "}";

        /// <summary>
        /// Parameter name to use for the OData path route constraint.
        /// </summary>
        public static readonly string ConstraintName = "ODataConstraint";

        /// <summary>
        /// Parameter name to use for the OData version route constraint.
        /// </summary>
        public static readonly string VersionConstraintName = "ODataVersionConstraint";

        /// <summary>
        /// Route data key for the action name.
        /// </summary>
        public static readonly string Action = "action";

        /// <summary>
        /// Route data key for the controller name.
        /// </summary>
        public static readonly string Controller = "controller";

        /// <summary>
        /// Route data key for entity keys.
        /// </summary>
        public static readonly string Key = "key";

        /// <summary>
        /// Route data key for the related key when deleting links.
        /// </summary>
        public static readonly string RelatedKey = "relatedKey";

        /// <summary>
        /// Route data key for the navigation property name when manipulating links.
        /// </summary>
        public static readonly string NavigationProperty = "navigationProperty";

        /// <summary>
        /// Route template suffix for OData batch.
        /// </summary>
        public static readonly string Batch = "$batch";

        /// <summary>
        /// Route data key for the dynamic property name when manipulating open type.
        /// </summary>
        public static readonly string DynamicProperty = "dynamicProperty";

        /// <summary>
        /// Route data key for the OData optional parameters.
        /// </summary>
        public static readonly string OptionalParameters = typeof(ODataOptionalParameter).FullName;

        /// <summary>
        /// Route data key that tracks the number of key segments,
        /// navigation properties and operation parameters in the request URI
        /// </summary>
        public static readonly string KeyCount = "ODataRouteKeyCount";
    }
}