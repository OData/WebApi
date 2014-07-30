// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.OData.Routing
{
    /// <summary>
    /// Provides the values of segment kinds for implementations of <see cref="ODataPathSegment" />.
    /// </summary>
    public static class ODataSegmentKinds
    {
        // need these as consts for using them in switch/case blocks.
        internal const string _ServiceBase = "~";
        internal const string _Batch = "$batch";
        internal const string _Ref = "$ref";
        internal const string _Metadata = "$metadata";
        internal const string _Value = "$value";
        internal const string _Action = "action";
        internal const string _Function = "function";
        internal const string _UnboundAction = "unboundaction";
        internal const string _UnboundFunction = "unboundfunction";
        internal const string _Cast = "cast";
        internal const string _EntitySet = "entityset";
        internal const string _Singleton = "singleton";
        internal const string _Key = "key";
        internal const string _Navigation = "navigation";
        internal const string _Property = "property";
        internal const string _Unresolved = "unresolved";

        /// <summary>
        /// Represents the service root segment (for OData service document).
        /// </summary>
        public static readonly string ServiceBase = _ServiceBase;

        /// <summary>
        /// Represents the OData $batch segment.
        /// </summary>
        public static readonly string Batch = _Batch;

        /// <summary>
        /// Represents the OData $ref segment.
        /// </summary>
        public static readonly string Ref = _Ref;

        /// <summary>
        /// Represents the OData $metadata segment.
        /// </summary>
        public static readonly string Metadata = _Metadata;

        /// <summary>
        /// Represents the OData $value segment.
        /// </summary>
        public static readonly string Value = _Value;

        /// <summary>
        /// Represents a segment indicating a bound OData action.
        /// </summary>
        public static readonly string Action = _Action;

        /// <summary>
        /// Represents a segment indicating a bound OData function.
        /// </summary>
        public static readonly string Function = _Function;

        /// <summary>
        /// Represents a segment indicating an unbound OData action.
        /// </summary>
        public static readonly string UnboundAction = _UnboundAction;

        /// <summary>
        /// Represents a segment indicating an unbound OData function.
        /// </summary>
        public static readonly string UnboundFunction = _UnboundFunction;

        /// <summary>
        /// Represents a segment indicating a type cast.
        /// </summary>
        public static readonly string Cast = _Cast;

        /// <summary>
        /// Represents a segment indicating an entity set.
        /// </summary>
        public static readonly string EntitySet = _EntitySet;

        /// <summary>
        /// Represents a segment indicating a singleton.
        /// </summary>
        public static readonly string Singleton = _Singleton;

        /// <summary>
        /// Represents a segment indicating an index by key operation.
        /// </summary>
        public static readonly string Key = _Key;

        /// <summary>
        /// Represents a segment indicating a navigation.
        /// </summary>
        public static readonly string Navigation = _Navigation;

        /// <summary>
        /// Represents a segment indicating a property access.
        /// </summary>
        public static readonly string Property = _Property;

        /// <summary>
        /// Represents a segment that is not understood.
        /// </summary>
        public static readonly string Unresolved = _Unresolved;
    }
}
