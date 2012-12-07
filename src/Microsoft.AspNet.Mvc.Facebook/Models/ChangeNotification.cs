// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.Facebook.Models
{
    /// <summary>
    /// Notification from Facebook as part of Realtime Updates.
    /// </summary>
    public class ChangeNotification
    {
        /// <summary>
        /// Gets or sets the object that has been updated.
        /// </summary>
        /// <value>
        /// The Facebook object.
        /// </value>
        public string Object { get; set; }

        /// <summary>
        /// Gets or sets the change entry.
        /// </summary>
        /// <value>
        /// The change entry.
        /// </value>
        public IEnumerable<ChangeEntry> Entry { get; set; }
    }
}