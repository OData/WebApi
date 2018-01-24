// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Nop.Core.Domain.Seo
{
    /// <summary>
    /// Represents an URL record
    /// </summary>
    public partial class UrlRecord : BaseEntity
    {
        /// <summary>
        /// Gets or sets the entity identifier
        /// </summary>
        public virtual int EntityId { get; set; }

        /// <summary>
        /// Gets or sets the entity name
        /// </summary>
        public virtual string EntityName { get; set; }

        /// <summary>
        /// Gets or sets the slug
        /// </summary>
        public virtual string Slug { get; set; }

        /// <summary>
        /// Gets or sets the language identifier
        /// </summary>
        public virtual int LanguageId { get; set; }
    }
}
