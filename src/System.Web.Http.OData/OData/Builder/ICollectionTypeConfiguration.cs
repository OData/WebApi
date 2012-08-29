// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.OData.Builder
{
    /// <summary>
    /// Configuration for a collection.
    /// </summary>
    public interface ICollectionTypeConfiguration : IEdmTypeConfiguration
    {
        /// <summary>
        /// The configuration for the ElementType of the Collection.
        /// </summary>
        IEdmTypeConfiguration ElementType { get; }
    }
}
