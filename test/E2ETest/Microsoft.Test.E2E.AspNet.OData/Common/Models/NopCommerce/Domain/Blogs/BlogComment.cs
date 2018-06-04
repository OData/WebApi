// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Nop.Core.Domain.Customers;

namespace Nop.Core.Domain.Blogs
{
    /// <summary>
    /// Represents a blog comment
    /// </summary>
    public partial class BlogComment : CustomerContent
    {
        /// <summary>
        /// Gets or sets the comment text
        /// </summary>
        public virtual string CommentText { get; set; }

        /// <summary>
        /// Gets or sets the blog post identifier
        /// </summary>
        public virtual int BlogPostId { get; set; }

        /// <summary>
        /// Gets or sets the blog post
        /// </summary>
        public virtual BlogPost BlogPost { get; set; }
    }
}