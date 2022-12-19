//-----------------------------------------------------------------------------
// <copyright file="ODataAPIResponseStatus.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Enum for Patch status.
    /// </summary>
    public enum ODataAPIResponseStatus
    {
        /// <summary>
        /// Success status.
        /// </summary>
        Success,
        /// <summary>
        /// Failure status.
        /// </summary>
        Failure,
        /// <summary>
        /// Resource not found.
        /// </summary>
        NotFound
    }
}
