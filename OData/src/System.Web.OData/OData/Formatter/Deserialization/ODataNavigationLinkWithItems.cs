// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData.Core;

namespace System.Web.OData.Formatter.Deserialization
{
    /// <summary>
    /// Encapsulates an <see cref="ODataNavigationLink"/> and the list of nested items.
    /// </summary>
    /// <remarks>
    /// A navigation link for a singleton navigation property can only contain one item - either ODataEntry or ODataEntityReferenceLink.
    /// A navigation link for a collection navigation property can contain any number of items - each is either ODataFeed or ODataEntityReferenceLink.
    /// </remarks>
    public sealed class ODataNavigationLinkWithItems : ODataItemBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataNavigationLinkWithItems"/>.
        /// </summary>
        /// <param name="item">The wrapped item.</param>
        public ODataNavigationLinkWithItems(ODataNavigationLink item)
            : base(item)
        {
            NestedItems = new List<ODataItemBase>();
        }

        /// <summary>
        /// Gets the wrapped <see cref="ODataNavigationLink"/>.
        /// </summary>
        public ODataNavigationLink NavigationLink
        {
            get
            {
                return Item as ODataNavigationLink;
            }
        }

        /// <summary>
        /// Gets the nested items that are part of this navigation link.
        /// </summary>
        public IList<ODataItemBase> NestedItems { get; private set; }
    }
}
