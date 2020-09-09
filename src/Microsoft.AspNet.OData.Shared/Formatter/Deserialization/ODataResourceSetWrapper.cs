// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
        /// To determine if its a delta resource set
        /// </summary>
        public bool IsDelta { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="ODataResourceSetWrapper"/>.
        /// </summary>
        /// <param name="item">The wrapped item.</param>
        public ODataResourceSetWrapper(ODataResourceSetBase item)
            : base(item)
        {
            Resources = new List<ODataResourceWrapper>();            
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ODataResourceSetWrapper"/>.Overloaded constructor for delta
        /// </summary>
        /// <param name="item"></param>
        /// <param name="isDelta"></param>
        public ODataResourceSetWrapper(ODataResourceSetBase item, bool isDelta)
        : base(item)
        {
            Resources = new List<ODataResourceWrapper>();
            DeltaLinks = new List<ODataDeltaLinkWrapper>();
            IsDelta = isDelta;
        }

        /// <summary>
        /// Gets the wrapped <see cref="ODataResourceSet"/>.
        /// </summary>
        public ODataResourceSetBase ResourceSet
        {
            get
            {
                if (IsDelta)
                {
                    return Item as ODataDeltaResourceSet;
                }                 

                return Item as ODataResourceSet;
            }
        }

        /// <summary>
        /// Gets the nested resources of this ResourceSet.
        /// </summary>
        public IList<ODataResourceWrapper> Resources { get; private set; }

        /// <summary>
        /// Delta Links and deleted links
        /// </summary>
        public IList<ODataDeltaLinkWrapper> DeltaLinks { get; private set; }
    }
}
