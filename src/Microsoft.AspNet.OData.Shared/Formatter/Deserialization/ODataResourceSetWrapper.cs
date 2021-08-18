//-----------------------------------------------------------------------------
// <copyright file="ODataResourceSetWrapper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Formatter.Deserialization
{
    /// <summary>
    /// Encapsulates an <see cref="ODataResourceSet"/> and the <see cref="ODataResource"/>'s that are part of it.
    /// </summary>
    public sealed class ODataResourceSetWrapper : ODataItemBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataResourceSetWrapper"/>.
        /// </summary>
        /// <param name="item">The wrapped item.</param>
        public ODataResourceSetWrapper(ODataResourceSet item)
            : base(item)
        {
            Resources = new List<ODataResourceWrapper>();
        }

        /// <summary>
        /// Gets the wrapped <see cref="ODataResourceSet"/>.
        /// </summary>
        public ODataResourceSet ResourceSet
        {
            get
            {
                return Item as ODataResourceSet;
            }
        }

        /// <summary>
        /// Gets the nested resources of this ResourceSet.
        /// </summary>
        public IList<ODataResourceWrapper> Resources { get; private set; }
    }
}
