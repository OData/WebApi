// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.OData.Builder
{
    /// <summary>
    /// Enumerates the navigation type can apply on navigation restrictions.
    /// </summary>
    internal enum CapabilitiesNavigationType
    {
        /// <summary>
        /// Navigation properties can be recursively navigated.
        /// </summary>
        Recursive,

        /// <summary>
        /// Navigation properties can be navigated to a single level.
        /// </summary>
        Single,

        /// <summary>
        /// Navigation properties are not navigable.
        /// </summary>
        None
    }
}