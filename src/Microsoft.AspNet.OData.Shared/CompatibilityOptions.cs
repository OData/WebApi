//-----------------------------------------------------------------------------
// <copyright file="CompatibilityOptions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
        AllowNextLinkWithNonPositiveTopValue = 0x1,

        /// <summary>
        /// Disable case-insensitive request property binding.
        /// </summary>
        DisableCaseInsensitiveRequestPropertyBinding = 0x2,

        /// <summary>
        /// Throw exception after logging ModelState error.
        /// </summary>
        ThrowExceptionAfterLoggingModelStateError = 0x4
    }
}
