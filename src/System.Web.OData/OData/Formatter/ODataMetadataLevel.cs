// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.OData.Formatter
{
    /// <summary>
    /// The amount of metadata information to serialize in an OData response (for JSON).
    /// </summary>
    public enum ODataMetadataLevel
    {
        /// <summary>
        /// JSON minimal metadata.
        /// </summary>
        MinimalMetadata = 0,

        /// <summary>
        /// JSON full metadata.
        /// </summary>
        FullMetadata = 1,

        /// <summary>
        /// JSON no metadata.
        /// </summary>
        NoMetadata = 2
    }
}
