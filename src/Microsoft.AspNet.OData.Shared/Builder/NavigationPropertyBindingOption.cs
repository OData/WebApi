//-----------------------------------------------------------------------------
// <copyright file="NavigationPropertyBindingOption.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNet.OData.Builder
{
    /// <summary>
    /// Options for navigation property binding.
    /// </summary>
    public enum NavigationPropertyBindingOption
    {
        /// <summary>
        /// Default behavior. It won't auto create the navigation property binding if multiple navigation sources.
        /// </summary>
        None = 0,

        /// <summary>
        /// Auto binding behavior. It will automatically pick the first one of target navigation sources.
        /// </summary>
        Auto = 1,
    }
}
