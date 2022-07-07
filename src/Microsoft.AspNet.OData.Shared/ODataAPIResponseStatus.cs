//-----------------------------------------------------------------------------
// <copyright file="ODataAPIResponseStatus.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Enum for Patch Status
    /// </summary>
    public enum ODataAPIResponseStatus
    {
        /// <summary>
        /// Success Status
        /// </summary>
        Success,
        /// <summary>
        /// Failure Status
        /// </summary>
        Failure,
        /// <summary>
        /// Resource Not Found
        /// </summary>
        NotFound
    }
}
