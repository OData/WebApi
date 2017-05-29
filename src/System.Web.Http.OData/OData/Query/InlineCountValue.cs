// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace System.Web.Http.OData.Query
{
    /// <summary>
    /// Defines an enumeration for $inlinecount query option values.
    /// </summary>
    public enum InlineCountValue
    {
        /// <summary>
        /// Corresponds to the 'none' $inlinecount query option value.
        /// </summary>
        None = 0,

        /// <summary>
        /// Corresponds to the 'allpages' $inlinecount query option value.
        /// </summary>
        AllPages = 1
    }
}
