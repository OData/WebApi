// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Formatter.Deserialization
{
    /// <summary>
    /// Encapsulates an <see cref="ODataResource"/> and the inner nested resource infos.
    /// </summary>
    public class ODataResourceWrapper : ODataItemBase
    {       
        /// <summary>
        /// Initializes a new instance of <see cref="ODataResourceWrapper"/>.
        /// </summary>
        /// <param name="item">The wrapped item.</param>
        public ODataResourceWrapper(ODataResourceBase item)
            : base(item)
        {
            NestedResourceInfos = new List<ODataNestedResourceInfoWrapper>();
        }

  
        /// <summary>
        /// Gets the wrapped <see cref="ODataResourceBase"/>.
        /// </summary>
        public ODataResourceBase ResourceBase
        {
            get
            {
                return Item as ODataResourceBase;             
            }
        }

        /// <summary>
        /// Gets the wrapped <see cref="ODataResource"/>.
        /// </summary>
        [Obsolete("Please use ResourceBase instead")]
        public ODataResource Resource
        {
            get
            {
                return Item as ODataResource;
            }
        }

        /// <summary>
        /// Gets the inner nested resource infos.
        /// </summary>
        public IList<ODataNestedResourceInfoWrapper> NestedResourceInfos { get; private set; }
    }
}
