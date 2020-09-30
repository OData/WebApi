// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Formatter.Deserialization
{
    /// <summary>
    /// Encapsulates an <see cref="ODataResourceSet"/> and the <see cref="ODataResource"/>'s that are part of it.
    /// </summary>
    public class ODataResourceSetWrapperBase : ODataItemBase
    {
        /// <summary>
        /// /
        /// </summary>
        public ResourceSetType ResourceSetType;

        /// <summary>
        /// Initializes a new instance of <see cref="ODataResourceSetWrapper"/>.
        /// </summary>
        /// <param name="item">The wrapped item.</param>
        public ODataResourceSetWrapperBase(ODataResourceSetBase item)
            : base(item)
        {
            ResourceItems = new List<ODataItemBase>();            
        }

   
         /// <summary>
        /// Gets the wrapped <see cref="ResourceSetBase"/>.
        /// </summary>
        public ODataResourceSetBase ResourceSetBase
        {
            get
            {
                return Item as ODataResourceSetBase;             
            }
        }

        /// <summary>
        /// Gets the nested resources and deltalinks of this ResourceSet.
        /// </summary>
        protected IList<ODataItemBase> ResourceItems { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public IList<ODataItemBase> Resources
        {
            get { return ResourceItems; }
        }
    }
}
