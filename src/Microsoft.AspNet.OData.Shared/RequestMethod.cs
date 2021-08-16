//-----------------------------------------------------------------------------
// <copyright file="RequestMethod.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
        
        /// <summary>
        /// "Options"
        /// </summary>
        Options
    }
}
