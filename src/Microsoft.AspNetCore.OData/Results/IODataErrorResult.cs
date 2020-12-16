// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
