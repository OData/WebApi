// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Formatter.Deserialization
{
    /// <summary>
    /// Encapsulates an <see cref="ODataResource"/> and the inner nested resource infos.
    /// </summary>
    public sealed class ODataDeltaLinkWrapper : ODataItemBase
    {
        /// <summary>
        /// To check delta
        /// </summary>
        public bool IsDeleted { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="ODataDeltaLinkWrapper"/>.
        /// </summary>
        /// <param name="item">The wrapped item.</param>
        public ODataDeltaLinkWrapper(ODataDeltaLinkBase item)
            : base(item)
        {
           
        }

        /// <summary>
        /// Encapsulates an <see cref="ODataResource"/> and the inner nested resource infos., overloaded for delta.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="isDeleted"></param>
        public ODataDeltaLinkWrapper(ODataDeltaLinkBase item, bool isDeleted)
           : base(item)
        {
            IsDeleted = isDeleted;
        }

        /// <summary>
        /// Gets the wrapped <see cref="ODataResource"/>.
        /// </summary>
        public ODataDeltaLinkBase DeltaLink
        {
            get
            {
                if (IsDeleted)
                {
                    return Item as ODataDeltaDeletedLink;
                }

                return Item as ODataDeltaLink;             
            }
        }
          
    }
}
