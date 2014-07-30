// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.OData.Formatter
{
    /// <summary>
    /// The amount of metadata information to serialize in an OData response (for JSON light).
    /// </summary>
    public enum ODataMetadataLevel
    {
        /// <summary>
        /// Normal metadata; used for anything other than JSON light (Atom/XML, JSON verbose).
        /// </summary>
        Default = 0,

        /// <summary>
        /// JSON light full metadata.
        /// </summary>
        FullMetadata = 1,

        /// <summary>
        /// JSON light minimal metadata.
        /// </summary>
        MinimalMetadata = 2,

        /// <summary>
        /// JSON light no metadata.
        /// </summary>
        NoMetadata = 3
    }
}
