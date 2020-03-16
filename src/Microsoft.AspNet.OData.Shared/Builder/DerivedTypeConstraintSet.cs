// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData.Edm.Csdl;

namespace Microsoft.AspNet.OData.Builder
{
    /// <summary>
    /// Set of derived type constraints. 
    /// </summary>
    public class DerivedTypeConstraintSet : HashSet<Type>
    {
        /// <summary>
        /// Initializes a DerivedTypeConstraintSet instance without setting a base type.
        /// </summary>
        public DerivedTypeConstraintSet()
        {
            Location = EdmVocabularyAnnotationSerializationLocation.Inline;
        }

        /// <summary>
        /// Initializes a DerivedTypeContraintSet instance with a base type specified.
        /// </summary>
        /// <param name="baseType">CLR type of the base class.</param>
        public DerivedTypeConstraintSet(Type baseType)
            : base()
        {
            ClrBaseType = baseType;
        }

        /// <summary>
        /// CLR Type of the base class. 
        /// </summary>
        public Type ClrBaseType { get; set; }

        /// <summary>
        /// Location of the derived type constraint annotation. By default, it will be inline.
        /// </summary>
        public EdmVocabularyAnnotationSerializationLocation Location { get; set; }

        /// <summary>
        /// Add TDerivedType to the set of derived type constraints.
        /// </summary>
        /// <typeparam name="TDerivedType">Derived Type.</typeparam>
        /// <returns>Updated DerivedTypeConstraint set.</returns>
        public DerivedTypeConstraintSet AddConstraint<TDerivedType>()
        {
            ValidateAndAddSingleConstraint(typeof(TDerivedType));
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
                ValidateAndAddSingleConstraint(ClrBaseType);
                return;
            }

            foreach (Type type in derivedTypes)
            {
                ValidateAndAddSingleConstraint(type);
            }
        }

        private void ValidateAndAddSingleConstraint(Type typeToAdd)
        {
            if (ClrBaseType == null)
            {
                throw Error.InvalidOperation(SRResources.NoClrTypeSpecified);
            }
            else if (!ClrBaseType.IsAssignableFrom(typeToAdd) && typeToAdd != ClrBaseType)
            {
                throw Error.InvalidOperation(SRResources.NotADerivedType, typeToAdd.Name, ClrBaseType.Name);
            }

            if (!this.Add(typeToAdd))
            {
                throw Error.InvalidOperation(SRResources.ConstraintAlreadyExists, typeToAdd.Name);
            }
        }
    }
}
