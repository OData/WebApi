//-----------------------------------------------------------------------------
// <copyright file="ForumSearchType.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Nop.Core.Domain.Forums
{
    /// <summary>
    /// Represents a forum search type
    /// </summary>
    public enum ForumSearchType
    {
        /// <summary>
        /// Topic titles and post text
        /// </summary>
        All = 0,
        /// <summary>
        /// Topic titles only
        /// </summary>
        TopicTitlesOnly = 10,
        /// <summary>
        /// Post text only
        /// </summary>
        PostTextOnly = 20,
    }
}
