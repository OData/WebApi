// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Web.Http;

namespace Microsoft.AspNet.Mvc.Facebook.Models
{
    /// <summary>
    /// Subscription verification data from Facebook as part of Realtime Updates.
    /// </summary>
    [FromUri(Name = "hub")]
    public class SubscriptionVerification
    {
        /// <summary>
        /// Gets or sets the mode.
        /// </summary>
        /// <value>
        /// The mode.
        /// </value>
        public string Mode { get; set; }

        /// <summary>
        /// Gets or sets the verify_token.
        /// </summary>
        /// <value>
        /// The verify_token.
        /// </value>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "This is a shipped API")]
        public string Verify_Token { get; set; }

        /// <summary>
        /// Gets or sets the challenge string.
        /// </summary>
        /// <value>
        /// The challenge string.
        /// </value>
        public string Challenge { get; set; }
    }
}