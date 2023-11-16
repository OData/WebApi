//-----------------------------------------------------------------------------
// <copyright file="IODataDeltaVersionProvider.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// An interface for customer to provide the delta version.
    /// </summary>
    public interface IODataDeltaVersionProvider
    {
        /// <summary>
        /// Gets the OData version for delta payload.
        /// </summary>
        ODataVersion Version { get; }
    }
}
