// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData;

namespace System.Web.OData.Formatter.Deserialization
{
    /// <summary>
    /// Encapsulates an <see cref="ODataResource"/> and the inner navigation links.
    /// </summary>
    public sealed class ODataEntryWithNavigationLinks : ODataItemBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataEntryWithNavigationLinks"/>.
        /// </summary>
        /// <param name="item">The wrapped item.</param>
        public ODataEntryWithNavigationLinks(ODataResource item)
            : base(item)
        {
            NavigationLinks = new List<ODataNavigationLinkWithItems>();
        }

        /// <summary>
        /// Gets the wrapped <see cref="ODataResource"/>.
        /// </summary>
        public ODataResource Resource
        {
            get
            {
                return Item as ODataResource;
            }
        }

        /// <summary>
        /// Gets the inner navigation links.
        /// </summary>
        public IList<ODataNavigationLinkWithItems> NavigationLinks { get; private set; }
    }
}
