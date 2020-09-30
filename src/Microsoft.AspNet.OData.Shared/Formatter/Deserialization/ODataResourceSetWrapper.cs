// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
        public ODataResourceSetWrapper(ODataResourceSetBase item)
            : base(item)
        {
            
        }

   
        /// <summary>
        /// Gets the wrapped <see cref="ODataResourceSet"/>.
        /// </summary>
        public ODataResourceSet ResourceSet
        {
            get
            {
                return Item as ODataResourceSet;
            }
        }

        /// <summary>
        /// Gets the nested resources of this ResourceSet.
        /// </summary>
        public new IList<ODataResourceWrapper> Resources 
        {
            get
            {
                IList<ODataResourceWrapper> resources=  ResourceItems as IList<ODataResourceWrapper>;

                if(resources == null)
                {
                    throw new ODataException("Right resources are not added to the Resources property");
                }

                return resources;
            }
        }    
    }
}
