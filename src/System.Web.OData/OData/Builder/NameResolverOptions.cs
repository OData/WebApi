// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.OData.Builder
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
