// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.OData.Formatter.Serialization
{
    /// <summary>
    /// EntitySelfLinks contains the Id, Edit and Read links for an entity.
    /// </summary>
    public class EntitySelfLinks
    {
        /// <summary>
        /// A string that uniquely identifies the resource.
        /// </summary>
         public string IdLink { get; set; }
 
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
