﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
