// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Formatter.Deserialization
{
    /// <summary>
    /// Encapsulates an <see cref="ODataResourceSet"/>  or <see cref="ODataDeltaResourceSet"/>  and the <see cref="ODataResource"/>'s that are part of it.
    /// </summary>        
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1012:AbstractTypesShouldNotHaveConstructors")]
    public abstract class ODataResourceSetWrapperBase : ODataItemBase
    {
        /// <summary>
        /// To determine the type of Resource Set
        /// </summary>        
        internal abstract ResourceSetType ResourceSetType { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="ODataResourceSetWrapper"/>.
        /// </summary>
        /// <param name="item">The wrapped item.</param>
        public ODataResourceSetWrapperBase(ODataResourceSetBase item)
            : base(item)
        {
            Resources = new List<ODataResourceWrapper>();
            ResourceSetBase = item;
        }

        /// <summary>
        /// Gets the wrapped <see cref="ODataResourceSetBase"/>.
        /// </summary>
        public ODataResourceSetBase ResourceSetBase { get; }

        /// <summary>
        /// Gets the members of this ResourceSet.
        /// </summary>
        public IList<ODataResourceWrapper> Resources { get; }
    }
}
