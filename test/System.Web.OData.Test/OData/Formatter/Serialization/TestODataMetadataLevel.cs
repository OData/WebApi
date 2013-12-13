// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.OData.Formatter.Serialization
{
    /// <summary>
    /// The amount of metadata information to serialize in an OData response (for JSON light).
    /// </summary>
    /// <remarks>
    /// This enum is a public copy of the internal <see cref="ODataMetadataLevel"/>. It allows using metadata levels
    /// in InlineData attributes on public test methods.
    /// </remarks>
    public enum TestODataMetadataLevel
    {
        /// <summary>
        /// Normal metadata; used for anything other than JSON light (Atom/XML, JSON verbose)
        /// </summary>
        Default = 0,

        /// <summary>
        /// JSON light full metadata
        /// </summary>
        FullMetadata = 1,

        /// <summary>
        /// JSON light minimal metadata
        /// </summary>
        MinimalMetadata = 2,

        /// <summary>
        /// JSON light no metadata
        /// </summary>
        NoMetadata = 3
    }
}
