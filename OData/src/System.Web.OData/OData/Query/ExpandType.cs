// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace System.Web.OData.Query
{
    /// <summary>
    /// Represents the type of expand.
    /// </summary>
    public enum ExpandType
    {
        /// <summary>
        /// Allowed to be expanded.
        /// </summary>
        Allowed,

        /// <summary>
        /// Automatic expanded.
        /// </summary>
        Automatic,

        /// <summary>
        /// Disallowed to be expanded.
        /// </summary>
        Disabled
    }
}
