// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Formatter.Deserialization
{
    /// <summary>
    /// Encapsulates an <see cref="ODataDeltaLinkBase"/> .
    /// </summary>
    public sealed class ODataDeltaLinkWrapper : ODataItemBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataDeltaLinkWrapper"/>.
        /// </summary>
        /// <param name="item">The wrapped item.</param>
        public ODataDeltaLinkWrapper(ODataDeltaLinkBase item)
            : base(item)
        {
           
        }

        /// <summary>
        /// Gets the wrapped <see cref="ODataDeltaLinkBase"/>.
        /// </summary>
        public ODataDeltaLinkBase DeltaLink
        {
            get
            {
                return Item as ODataDeltaLinkBase;                        
            }
        }          
    }
}
