//-----------------------------------------------------------------------------
// <copyright file="SingletonConfigurationOfTEntityType.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Builder
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

        internal SingletonConfiguration Singleton
        {
            get { return (SingletonConfiguration)Configuration; }
        }

        /// <summary>
        /// Adds subtypes to the list of derived type constraints.
        /// </summary>
        /// <param name="subtypes">The subtypes for which the constraint needs to be added.</param>
        /// <returns>Updated configuration object.</returns>
        public SingletonConfiguration<TEntityType> HasDerivedTypeConstraints(params Type[] subtypes)
        {
            Singleton.HasDerivedTypeConstraints(subtypes);
            return this;
        }

        /// <summary>
        /// Adds TDerivedType to the list of derived type constraints.
        /// </summary>
        /// <returns>Updated configuration object.</returns>
        public SingletonConfiguration<TEntityType> HasDerivedTypeConstraints<TDerivedType>() where TDerivedType : TEntityType
        {
            Singleton.HasDerivedTypeConstraints(typeof(TDerivedType));
            return this;
        }
    }
}
