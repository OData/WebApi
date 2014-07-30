// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.OData.Routing
{
    /// <summary>
    /// Provides the values of segment kinds for implementations of <see cref="ODataPathSegment" />.
    /// </summary>
    public static class ODataSegmentKinds
    {
        /// <summary>
        /// Represents the service root segment (for OData service document).
        /// </summary>
        public static readonly string ServiceBase = "~";

        /// <summary>
        /// Represents the OData $batch segment.
        /// </summary>
        public static readonly string Batch = "$batch";

        /// <summary>
        /// Represents the OData $links segment.
        /// </summary>
        public static readonly string Links = "$links";

        /// <summary>
        /// Represents the OData $metadata segment.
        /// </summary>
        public static readonly string Metadata = "$metadata";

        /// <summary>
        /// Represents the OData $value segment.
        /// </summary>
        public static readonly string Value = "$value";

        /// <summary>
        /// Represents a segment indicating an OData action.
        /// </summary>
        public static readonly string Action = "action";

        /// <summary>
        /// Represents a segment indicating a type cast.
        /// </summary>
        public static readonly string Cast = "cast";

        /// <summary>
        /// Represents a segment indicating an entity set.
        /// </summary>
        public static readonly string EntitySet = "entityset";

        /// <summary>
        /// Represents a segment indicating an index by key operation.
        /// </summary>
        public static readonly string Key = "key";

        /// <summary>
        /// Represents a segment indicating a navigation.
        /// </summary>
        public static readonly string Navigation = "navigation";

        /// <summary>
        /// Represents a segment indicating a property access.
        /// </summary>
        public static readonly string Property = "property";

        /// <summary>
        /// Represents a segment that is not understood.
        /// </summary>
        public static readonly string Unresolved = "unresolved";
    }
}
