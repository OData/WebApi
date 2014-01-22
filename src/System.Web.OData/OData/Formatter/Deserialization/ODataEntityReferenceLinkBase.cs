// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.OData.Core;

namespace System.Web.OData.Formatter.Deserialization
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
