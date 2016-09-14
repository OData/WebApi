// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Formatter.Deserialization
{
    /// <summary>
    /// Encapsulates an <see cref="ODataNestedResourceInfo"/> and the list of nested items.
    /// </summary>
    /// <remarks>
    /// A nested resource info for a singleton nested property can only contain one item - either ODataResource or ODataEntityReferenceLink.
    /// A nested resource info for a collection nested property can contain any number of items - each is either ODataResource or ODataEntityReferenceLink.
    /// </remarks>
    public sealed class ODataNestedResourceInfoWrapper : ODataItemBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataNestedResourceInfoWrapper"/>.
        /// </summary>
        /// <param name="item">The wrapped item.</param>
        public ODataNestedResourceInfoWrapper(ODataNestedResourceInfo item)
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
        /// Gets the nested items that are part of this nested resource info.
        /// </summary>
        public IList<ODataItemBase> NestedItems { get; private set; }
    }
}
