// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Web.WebPages.OAuth
{
    /// <summary>
    /// Represents built in OAuth clients.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "OAuth", Justification = "OAuth is a brand name.")]
    public enum BuiltInOAuthClient
    {
        /// <summary>
        /// Represents Twitter OAuth client
        /// </summary>
        Twitter,

        /// <summary>
        /// Represents Facebook OAuth client
        /// </summary>
        Facebook,

        /// <summary>
        /// Represents LinkedIn OAuth client
        /// </summary>
        LinkedIn,

        /// <summary>
        /// Represents WindowsLive OAuth client
        /// </summary>
        WindowsLive
    }
}