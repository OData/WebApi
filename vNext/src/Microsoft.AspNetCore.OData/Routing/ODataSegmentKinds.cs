// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    /// Provides the values of segment kinds for implementations of <see cref="ODataPathSegment" />.
    /// </summary>
    public static class ODataSegmentKinds
    {
        /// <summary>
        /// Represents the service root segment (for OData service document).
        /// </summary>
        public const string ServiceBase = "~";

        /// <summary>
        /// Represents the OData $batch segment.
        /// </summary>
        public const string Batch = "$batch";

        /// <summary>
        /// Represents the OData $ref segment.
        /// </summary>
        public const string Ref = "$ref";

        /// <summary>
        /// Represents the OData $metadata segment.
        /// </summary>
        public const string Metadata = "$metadata";

        /// <summary>
        /// Represents the OData $value segment.
        /// </summary>
        public const string Value = "$value";

        /// <summary>
        /// Represents the OData $count segment.
        /// </summary>
        public const string Count = "$count";

        /// <summary>
        /// Represents a segment indicating a bound OData action.
        /// </summary>
        public const string Action = "action";

        /// <summary>
        /// Represents a segment indicating a bound OData function.
        /// </summary>
        public const string Function = "function";

        /// <summary>
        /// Represents a segment indicating an unbound OData action.
        /// </summary>
        public const string UnboundAction = "unboundaction";

        /// <summary>
        /// Represents a segment indicating an unbound OData function.
        /// </summary>
        public const string UnboundFunction = "unboundfunction";

        /// <summary>
        /// Represents a segment indicating a type cast.
        /// </summary>
        public const string Cast = "cast";

        /// <summary>
        /// Represents a segment indicating an entity set.
        /// </summary>
        public const string EntitySet = "entityset";

        /// <summary>
        /// Represents a segment indicating a singleton.
        /// </summary>
        public const string Singleton = "singleton";

        /// <summary>
        /// Represents a segment indicating an index by key operation.
        /// </summary>
        public const string Key = "key";

        /// <summary>
        /// Represents a segment indicating a navigation.
        /// </summary>
        public const string Navigation = "navigation";

        /// <summary>
        /// Represents a segment indicating a navigation link.
        /// </summary>
        public const string PathTemplate = "template";

        /// <summary>
        /// Represents a segment indicating a property access.
        /// </summary>
        public const string Property = "property";

        /// <summary>
        /// Represents a segment indicating an dynamic property access.
        /// </summary>
        public const string DynamicProperty = "dynamicproperty";

        /// <summary>
        /// Represents a segment that is not understood.
        /// </summary>
        public const string Unresolved = "unresolved";
    }
}
