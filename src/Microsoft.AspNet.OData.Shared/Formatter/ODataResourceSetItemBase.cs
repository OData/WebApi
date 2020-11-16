// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Formatter.Deserialization
{
    /// <summary>
    /// Encapsulates an <see cref="ODataResourceSetItemBase"/> .
    /// </summary>
    public abstract class ODataResourceSetItemBase : ODataItemBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataResourceSetItemBase"/>.
        /// </summary>
        /// <param name="item">The wrapped item.</param>
        public ODataResourceSetItemBase(ODataItem item)
            : base(item)
        {
           
        }         
    }
}
