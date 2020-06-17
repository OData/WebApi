// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.
using System;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Contains bitmasks for features that need backward compatibilty.
    /// </summary>
    [Flags]
    public enum CompatibilityOptions
    {
        /// <summary>
        /// No compatibility options are selected. 
        /// </summary>
        None = 0x0,

        /// <summary>
        ///  Generate nextlink even if the top value specified in request is less than page size when the request extension method is directly called.
        /// </summary>
        AllowNextLinkWithNonPositiveTopValue = 0x1
    }
}
