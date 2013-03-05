// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    /// <summary>
    /// Encapuslates an <see cref="ODataEntry"/> and the inner navigation links.
    /// </summary>
    public sealed class ODataEntryWithNavigationLinks : ODataItemBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataEntryWithNavigationLinks"/>.
        /// </summary>
        /// <param name="item">The wrapped item.</param>
        public ODataEntryWithNavigationLinks(ODataEntry item)
            : base(item)
        {
            NavigationLinks = new List<ODataNavigationLinkWithItems>();
        }

        /// <summary>
        /// Gets the wrapped <see cref="ODataEntry"/>.
        /// </summary>
        public ODataEntry Entry
        {
            get
            {
                return Item as ODataEntry;
            }
        }

        /// <summary>
        /// Gets the inner navigation links.
        /// </summary>
        public IList<ODataNavigationLinkWithItems> NavigationLinks { get; private set; }
    }
}
