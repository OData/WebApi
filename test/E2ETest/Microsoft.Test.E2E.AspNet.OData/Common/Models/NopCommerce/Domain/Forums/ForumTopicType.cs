//-----------------------------------------------------------------------------
// <copyright file="ForumTopicType.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Nop.Core.Domain.Forums
{
    /// <summary>
    /// Represents a forum topic type
    /// </summary>
    public enum ForumTopicType
    {
        /// <summary>
        /// Normal
        /// </summary>
        Normal = 10,
        /// <summary>
        /// Sticky
        /// </summary>
        Sticky = 15,
        /// <summary>
        /// Announcement
        /// </summary>
        Announcement = 20,
    }
}
