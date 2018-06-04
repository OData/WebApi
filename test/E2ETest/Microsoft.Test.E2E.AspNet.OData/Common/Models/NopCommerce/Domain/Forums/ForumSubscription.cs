﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Nop.Core.Domain.Customers;

namespace Nop.Core.Domain.Forums
{
    /// <summary>
    /// Represents a forum subscription item
    /// </summary>
    public partial class ForumSubscription : BaseEntity
    {
        /// <summary>
        /// Gets or sets the forum subscription identifier
        /// </summary>
        public virtual Guid SubscriptionGuid { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        public virtual int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the forum identifier
        /// </summary>
        public virtual int ForumId { get; set; }

        /// <summary>
        /// Gets or sets the topic identifier
        /// </summary>
        public virtual int TopicId { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public virtual DateTimeOffset CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets the customer
        /// </summary>
        public virtual Customer Customer { get; set; }
    }
}
