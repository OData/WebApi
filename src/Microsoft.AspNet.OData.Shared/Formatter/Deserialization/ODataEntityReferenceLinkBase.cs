//-----------------------------------------------------------------------------
// <copyright file="ODataEntityReferenceLinkBase.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData;

namespace Microsoft.AspNet.OData.Formatter.Deserialization
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
            EntityReferenceLink = item;
        }

        /// <summary>
        /// Gets the wrapped <see cref="ODataEntityReferenceLink"/>.
        /// </summary>
        public ODataEntityReferenceLink EntityReferenceLink { get; }

    }
}
