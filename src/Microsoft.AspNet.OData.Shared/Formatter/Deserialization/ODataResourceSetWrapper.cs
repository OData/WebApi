//-----------------------------------------------------------------------------
// <copyright file="ODataResourceSetWrapper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Formatter.Deserialization
{
    /// <summary>
    /// Encapsulates an <see cref="ODataResourceSet"/> and the <see cref="ODataResource"/>'s that are part of it.
    /// </summary>
    public sealed class ODataResourceSetWrapper : ODataResourceSetWrapperBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataResourceSetWrapper"/>.
        /// </summary>
        /// <param name="item">The wrapped item.</param>        
        public ODataResourceSetWrapper(ODataResourceSet item)
            : base(item)
        {
            ResourceSet = item;                                
        }

        internal override ResourceSetType ResourceSetType => ResourceSetType.ResourceSet;

        /// <summary>
        /// Gets the wrapped <see cref="ODataResourceSet"/>.
        /// </summary>
        public ODataResourceSet ResourceSet { get; }

    }
}
