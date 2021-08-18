//-----------------------------------------------------------------------------
// <copyright file="PollVotingRecord.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
