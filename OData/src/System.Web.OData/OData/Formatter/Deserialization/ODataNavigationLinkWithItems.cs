// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData;

namespace System.Web.OData.Formatter.Deserialization
{
    /// <summary>
    /// Encapsulates an <see cref="ODataNestedResourceInfo"/> and the list of nested items.
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
        public ODataNavigationLinkWithItems(ODataNestedResourceInfo item)
            : base(item)
        {
            NestedItems = new List<ODataItemBase>();
        }

        /// <summary>
        /// Gets the wrapped <see cref="ODataNestedResourceInfo"/>.
        /// </summary>
        public ODataNestedResourceInfo NestedResourceInfo
        {
            get
            {
                return Item as ODataNestedResourceInfo;
            }
        }

        /// <summary>
        /// Gets the nested items that are part of this navigation link.
        /// </summary>
        public IList<ODataItemBase> NestedItems { get; private set; }
    }
}
