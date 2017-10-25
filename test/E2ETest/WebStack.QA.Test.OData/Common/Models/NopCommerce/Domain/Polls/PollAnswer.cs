// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Nop.Core.Domain.Polls
{
    /// <summary>
    /// Represents a poll answer
    /// </summary>
    public partial class PollAnswer : BaseEntity
    {
        private ICollection<PollVotingRecord> _pollVotingRecords;

        /// <summary>
        /// Gets or sets the poll identifier
        /// </summary>
        public virtual int PollId { get; set; }

        /// <summary>
        /// Gets or sets the poll answer name
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Gets or sets the current number of votes
        /// </summary>
        public virtual int NumberOfVotes { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public virtual int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the poll
        /// </summary>
        public virtual Poll Poll { get; set; }

        /// <summary>
        /// Gets or sets the poll voting records
        /// </summary>
        public virtual ICollection<PollVotingRecord> PollVotingRecords
        {
            get { return _pollVotingRecords ?? (_pollVotingRecords = new List<PollVotingRecord>()); }
            protected set { _pollVotingRecords = value; }
        }
    }
}