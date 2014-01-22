// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData.Core;

namespace System.Web.OData.Formatter.Deserialization
{
    /// <summary>
    /// Encapsulates an <see cref="ODataFeed"/> and the <see cref="ODataEntry"/>'s that are part of it.
    /// </summary>
    public sealed class ODataFeedWithEntries : ODataItemBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataFeedWithEntries"/>.
        /// </summary>
        /// <param name="item">The wrapped item.</param>
        public ODataFeedWithEntries(ODataFeed item)
            : base(item)
        {
            Entries = new List<ODataEntryWithNavigationLinks>();
        }

        /// <summary>
        /// Gets the wrapped <see cref="ODataFeed"/>.
        /// </summary>
        public ODataFeed Feed
        {
            get
            {
                return Item as ODataFeed;
            }
        }

        /// <summary>
        /// Gets the nested entries of this feed.
        /// </summary>
        public IList<ODataEntryWithNavigationLinks> Entries { get; private set; }
    }
}
