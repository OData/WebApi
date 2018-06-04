// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Nop.Core.Domain.Customers;

namespace Nop.Core.Domain.Polls
{
    /// <summary>
    /// Represents a poll voting record
    /// </summary>
    public partial class PollVotingRecord : CustomerContent
    {
        /// <summary>
        /// Gets or sets the poll answer identifier
        /// </summary>
        public virtual int PollAnswerId { get; set; }

        /// <summary>
        /// Gets or sets the poll answer
        /// </summary>
        public virtual PollAnswer PollAnswer { get; set; }
    }
}