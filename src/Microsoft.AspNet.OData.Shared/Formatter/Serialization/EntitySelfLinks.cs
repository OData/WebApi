//-----------------------------------------------------------------------------
// <copyright file="EntitySelfLinks.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace Microsoft.AspNet.OData.Formatter.Serialization
{
    /// <summary>
    /// EntitySelfLinks contains the Id, Edit and Read links for an entity.
    /// </summary>
    public class EntitySelfLinks
    {
        /// <summary>
        /// A string that uniquely identifies the resource.
        /// </summary>
         public Uri IdLink { get; set; }
 
        /// <summary>
        /// A URL that can be used to edit a copy of the resource.
        /// </summary>
         public Uri EditLink { get; set; }
 
        /// <summary>
        /// A URL that can be used to retrieve a copy of the resource.
        /// </summary>
         public Uri ReadLink { get; set; }
     }
 }
