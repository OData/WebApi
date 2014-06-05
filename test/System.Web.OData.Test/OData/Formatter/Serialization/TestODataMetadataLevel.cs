// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.OData.Formatter.Serialization
{
    /// <summary>
    /// The amount of metadata information to serialize in an OData response (for JSON).
    /// </summary>
    /// <remarks>
    /// This enum is a public copy of the internal <see cref="ODataMetadataLevel"/>. It allows using metadata levels
    /// in InlineData attributes on public test methods.
    /// </remarks>
    public enum TestODataMetadataLevel
    {
        /// <summary>
        /// JSON minimal metadata
        /// </summary>
        MinimalMetadata = 0,

        /// <summary>
        /// JSON full metadata
        /// </summary>
        FullMetadata = 1,

        /// <summary>
        /// JSON no metadata
        /// </summary>
        NoMetadata = 2
    }
}
