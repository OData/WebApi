//-----------------------------------------------------------------------------
// <copyright file="DerivedTypeConstraintConfiguration.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData.Edm.Csdl;

namespace Microsoft.AspNet.OData.Builder
{
    /// <summary>
    /// Configuration for derived type constraints. 
    /// </summary>
    public class DerivedTypeConstraintConfiguration
    {
        /// <summary>
        /// Initializes a DerivedTypeConstraintConfiguration instance without setting a base type.
        /// </summary>
        public DerivedTypeConstraintConfiguration()
            : this(EdmVocabularyAnnotationSerializationLocation.Inline)
        {
        }

        /// <summary>
        /// Initializes a DerivedTypeContraintSet instance with a base type specified.
        /// </summary>
        /// <param name="location">The location for the annotation.</param>
        public DerivedTypeConstraintConfiguration(EdmVocabularyAnnotationSerializationLocation location)
        {
            Location = location;
            ConstraintSet = new HashSet<Type>();
        }

        /// <summary>
        /// Location of the derived type constraint annotation. By default, it will be inline.
        /// </summary>
        public EdmVocabularyAnnotationSerializationLocation Location { get; set; }

        /// <summary>
        /// Add TDerivedType to the set of derived type constraints.
        /// </summary>
        /// <typeparam name="TDerivedType">Derived Type.</typeparam>
        /// <returns>Updated DerivedTypeConstraint set.</returns>
        public DerivedTypeConstraintConfiguration AddConstraint<TDerivedType>()
        {
            if (!this.ConstraintSet.Add(typeof(TDerivedType)))
            {
                throw Error.InvalidOperation(SRResources.ConstraintAlreadyExists, typeof(TDerivedType).Name);
            }

            return this;
        }

        /// <summary>
        /// Add the derived type constraints to the set. 
        /// </summary>
        /// <param name="derivedTypes">Derived types to be added to the set.</param>
        public void AddConstraints(IEnumerable<Type> derivedTypes)
        {
            if (!derivedTypes.Any())
            {
                throw Error.InvalidOperation(SRResources.NoClrTypeSpecified);
            }

            foreach (Type type in derivedTypes)
            {
                if (!this.ConstraintSet.Add(type))
                {
                    throw Error.InvalidOperation(SRResources.ConstraintAlreadyExists, type.Name);
                }
            }
        }

        internal ISet<Type> ConstraintSet { get; }

        internal void ValidateConstraints(Type clrBaseType)
        {
            if (clrBaseType == null)
            {
                throw Error.InvalidOperation(SRResources.NoClrTypeSpecified);
            }

            foreach (Type type in ConstraintSet)
            {
                if (!clrBaseType.IsAssignableFrom(type))
                {
                    throw Error.InvalidOperation(SRResources.NotADerivedType, type.Name, clrBaseType.Name);
                }
            }
        }
    }
}
