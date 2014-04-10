// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http;
using System.Web.OData.Properties;
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
