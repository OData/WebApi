//-----------------------------------------------------------------------------
// <copyright file="ODataDeltaResourceSetWrapper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Formatter.Deserialization
{
    /// <summary>
    /// Encapsulates an <see cref="ODataDeltaResourceSet"/> and the <see cref="ODataResourceBase"/>'s that are part of it.
    /// </summary>
    public sealed class ODataDeltaResourceSetWrapper : ODataResourceSetWrapperBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataDeltaResourceSetWrapper"/>.
        /// </summary>
        /// <param name="item">The wrapped item.</param>
        public ODataDeltaResourceSetWrapper(ODataDeltaResourceSet item)
            : base(item)
        {
            
        }

        internal override ResourceSetType ResourceSetType => ResourceSetType.DeltaResourceSet;
    }
}
