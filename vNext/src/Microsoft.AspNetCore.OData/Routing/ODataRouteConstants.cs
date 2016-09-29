// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.OData.Routing
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
        /// "Get"
        /// </summary>
        public const string HttpGet = "GET";

        /// <summary>
        /// "Delete"
        /// </summary>
        public const string HttpDelete = "DELETE";

        /// <summary>
        /// "Patch"
        /// </summary>
        public const string HttpPatch = "PATCH";

        /// <summary>
        /// "Post"
        /// </summary>
        public const string HttpPost = "POST";

        /// <summary>
        /// "Put"
        /// </summary>
        public const string HttpPut = "PUT";
    }
}
