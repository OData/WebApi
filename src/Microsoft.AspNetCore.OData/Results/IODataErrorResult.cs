//-----------------------------------------------------------------------------
// <copyright file="IODataErrorResult.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData;

namespace Microsoft.AspNet.OData.Results
{
    /// <summary>
    /// Provide the interface for the details of a given OData Error result.
    /// </summary>
    public interface IODataErrorResult
    {
        /// <summary>
        /// OData Error.
        /// </summary>
        ODataError Error { get; }
    }
}
