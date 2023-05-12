//-----------------------------------------------------------------------------
// <copyright file="CompatibilityOptions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Runtime.CompilerServices;

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
        ThrowExceptionAfterLoggingModelStateError = 0x4,

        /// <summary>
        /// Disable the reuse of the ODataQueryOptions instance generated during model binding in EnableQueryAttribute.
        /// </summary>
        DisableODataQueryOptionsReuse = 0x8,
    }

    /// <summary>
    /// Extension methods for <see cref="CompatibilityOptions"/>.
    /// </summary>
    public static class CompatibilityOptionsExtensions
    {
        /// <summary>
        /// Determines whether the provided option is set.
        /// </summary>
        /// <param name="options">The set options.</param>
        /// <param name="option">The option to check.</param>
        /// <returns>True if the option is set, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasOption(this CompatibilityOptions options, CompatibilityOptions option)
        {
            return (options & option) == option;
        }
    }
}
