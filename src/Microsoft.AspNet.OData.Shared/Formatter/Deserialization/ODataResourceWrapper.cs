//-----------------------------------------------------------------------------
// <copyright file="ODataResourceWrapper.cs" company=".NET Foundation">
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
    /// Encapsulates an <see cref="ODataResource"/> and the inner nested resource infos.
    /// </summary>
    public sealed class ODataResourceWrapper : ODataItemBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataResourceWrapper"/>.
        /// </summary>
        /// <param name="item">The wrapped item.</param>
        public ODataResourceWrapper(ODataResourceBase item)
            : base(item)
        {
            NestedResourceInfos = new List<ODataNestedResourceInfoWrapper>();
            ResourceBase = item;
        }

        /// <summary>
        /// Gets the wrapped <see cref="ODataResourceBase"/>.
        /// </summary>
        public ODataResourceBase ResourceBase { get; }

        /// <summary>
        /// Gets the wrapped <see cref="ODataResource"/>. This will return null for deleted resources.
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
