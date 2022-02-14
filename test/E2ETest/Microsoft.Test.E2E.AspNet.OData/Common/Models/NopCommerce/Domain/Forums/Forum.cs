//-----------------------------------------------------------------------------
// <copyright file="Forum.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace Nop.Core.Domain.Forums
{
    /// <summary>
    /// Represents a forum
    /// </summary>
    public partial class Forum : BaseEntity
    {
        /// <summary>
        /// Gets or sets the forum group identifier
        /// </summary>
        public virtual int ForumGroupId { get; set; }

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Gets or sets the description
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// Gets or sets the number of topics
        /// </summary>
        public virtual int NumTopics { get; set; }

        /// <summary>
        /// Gets or sets the number of posts
        /// </summary>
        public virtual int NumPosts { get; set; }

        /// <summary>
        /// Gets or sets the last topic identifier
        /// </summary>
        public virtual int LastTopicId { get; set; }

        /// <summary>
        /// Gets or sets the last post identifier
        /// </summary>
        public virtual int LastPostId { get; set; }

        /// <summary>
        /// Gets or sets the last post customer identifier
        /// </summary>
        public virtual int LastPostCustomerId { get; set; }

        /// <summary>
        /// Gets or sets the last post date and time
        /// </summary>
        public virtual DateTimeOffset? LastPostTime { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public virtual int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public virtual DateTimeOffset CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance update
        /// </summary>
        public virtual DateTimeOffset UpdatedOnUtc { get; set; }

        /// <summary>
        /// Gets the ForumGroup
        /// </summary>
        public virtual ForumGroup ForumGroup { get; set; }
    }
}
