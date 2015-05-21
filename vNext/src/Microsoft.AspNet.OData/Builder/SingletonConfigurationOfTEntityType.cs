// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Common;

using Microsoft.OData.Edm;

namespace System.Web.OData.Builder
{
    /// <summary>
    /// Represents an <see cref="IEdmSingleton"/> that can be built using <see cref="ODataModelBuilder"/>.
    /// </summary>
    public class SingletonConfiguration<TEntityType> : NavigationSourceConfiguration<TEntityType> where TEntityType : class
    {
        internal SingletonConfiguration(ODataModelBuilder modelBuilder, string name)
            : base(modelBuilder, new SingletonConfiguration(modelBuilder, typeof(TEntityType), name))
        {
        }

        internal SingletonConfiguration(ODataModelBuilder modelBuilder, SingletonConfiguration configuration)
            : base(modelBuilder, configuration)
        {
        }
    }
}
