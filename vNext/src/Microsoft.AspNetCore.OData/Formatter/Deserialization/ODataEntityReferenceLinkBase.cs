// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Formatter.Deserialization
{
    /// <summary>
    /// Encapuslates an <see cref="ODataEntityReferenceLink"/>.
    /// </summary>
    public class ODataEntityReferenceLinkBase : ODataItemBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataEntityReferenceLinkBase"/>.
        /// </summary>
        /// <param name="item">The wrapped item.</param>
        public ODataEntityReferenceLinkBase(ODataEntityReferenceLink item)
            : base(item)
        {
        }

        /// <summary>
        /// Gets the wrapped <see cref="ODataEntityReferenceLink"/>.
        /// </summary>
        public ODataEntityReferenceLink EntityReferenceLink
        {
            get
            {
                return Item as ODataEntityReferenceLink;
            }
        }
    }
}
