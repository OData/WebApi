// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData;

namespace System.Web.OData.Formatter.Deserialization
{
    /// <summary>
    /// Encapsulates an <see cref="ODataResourceSet"/> and the <see cref="ODataResource"/>'s that are part of it.
    /// </summary>
    public sealed class ODataFeedWithEntries : ODataItemBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataFeedWithEntries"/>.
        /// </summary>
        /// <param name="item">The wrapped item.</param>
        public ODataFeedWithEntries(ODataResourceSet item)
            : base(item)
        {
            Entries = new List<ODataEntryWithNavigationLinks>();
        }

        /// <summary>
        /// Gets the wrapped <see cref="ODataResourceSet"/>.
        /// </summary>
        public ODataResourceSet Feed
        {
            get
            {
                return Item as ODataResourceSet;
            }
        }

        /// <summary>
        /// Gets the nested entries of this feed.
        /// </summary>
        public IList<ODataEntryWithNavigationLinks> Entries { get; private set; }
    }
}
