//-----------------------------------------------------------------------------
// <copyright file="SelectExpandType.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNet.OData.Query
{
    /// <summary>
    /// Represents the type of expand and select.
    /// </summary>
    public enum SelectExpandType
    {
        /// <summary>
        /// Allowed to be expanded and selected.
        /// </summary>
        Allowed,

        /// <summary>
        /// Automatic expanded and selected.
        /// </summary>
        Automatic,

        /// <summary>
        /// Disallowed to be expanded and selected.
        /// </summary>
        Disabled
    }
}
