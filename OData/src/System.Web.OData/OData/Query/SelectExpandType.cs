// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace System.Web.OData.Query
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
