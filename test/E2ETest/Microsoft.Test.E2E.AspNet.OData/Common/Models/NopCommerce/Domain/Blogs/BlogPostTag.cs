//-----------------------------------------------------------------------------
// <copyright file="BlogPostTag.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
