﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.OData.Builder
{
    /// <summary>
    /// Options for resolving names.
    /// </summary>
    [Flags]
    public enum NameResolverOptions
    {
        /// <summary>
        /// Process reflected property names.
        /// </summary>
        ProcessReflectedPropertyNames = 1,

        /// <summary>
        /// Process property names in DataMemberAttribute
        /// such as [DataMember(Name = "DataMemberCustomerName")].
        /// </summary>
        ProcessDataMemberAttributePropertyNames = 2,

        /// <summary>
        /// Process explicit property names
        /// such as entityTypeConfiguration.Property(e => e.Key).Name="Id".
        /// </summary>
        ProcessExplicitPropertyNames = 4
    }
}
