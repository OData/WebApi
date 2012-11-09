// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.OData.Routing
{
    /// <summary>
    /// Provides the values of segment kinds for implementations of <see cref="ODataPathSegment" />.
    /// </summary>
    public static class ODataSegmentKinds
    {
        public static readonly string ServiceBase = "~";
        public static readonly string Batch = "$batch";
        public static readonly string Links = "$links";
        public static readonly string Metadata = "$metadata";
        public static readonly string Value = "$value";
        public static readonly string Action = "action";
        public static readonly string Cast = "cast";
        public static readonly string EntitySet = "entityset";
        public static readonly string Key = "key";
        public static readonly string Navigation = "navigation";
        public static readonly string Property = "property";
        public static readonly string Unresolved = "unresolved";
    }
}
