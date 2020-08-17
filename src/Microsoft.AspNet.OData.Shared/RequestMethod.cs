// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// An enumeration for request methods.
    /// </summary>
    internal enum ODataRequestMethod
    {
        /// <summary>
        /// An unknown method.
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// "Get"
        /// </summary>
        Get = 0,

        /// <summary>
        /// "Delete"
        /// </summary>
        Delete,

        /// <summary>
        /// "Merge"
        /// </summary>
        Merge,

        /// <summary>
        /// "Patch"
        /// </summary>
        Patch,

        /// <summary>
        /// "Post"
        /// </summary>
        Post,

        /// <summary>
        /// "Put"
        /// </summary>
        Put,

        /// <summary>
        /// "Head"
        /// </summary>
        Head,
    }
}
