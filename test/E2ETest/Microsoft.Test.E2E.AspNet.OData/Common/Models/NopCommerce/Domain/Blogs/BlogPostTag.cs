// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Nop.Core.Domain.Blogs
{
    /// <summary>
    /// Represents a blog post tag
    /// </summary>
    public partial class BlogPostTag
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Gets or sets the tagged product count
        /// </summary>
        public virtual int BlogPostCount { get; set; }
    }
}