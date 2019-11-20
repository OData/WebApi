// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Common;
using Microsoft.OData.UriParser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNet.OData.Builder
{
    internal class DerivedTypeConstraintSet : HashSet<Type>
    {
        /// <summary>
        /// 
        /// </summary>
        public DerivedTypeConstraintSet()
        {
        }

        public DerivedTypeConstraintSet(Type baseType)
        {
            ClrBaseType = baseType;
        }

        public Type ClrBaseType { get; set; }

        public void ValidateAndAddConstraints(IEnumerable<Type> types)
        {
            if (ClrBaseType == null)
            {
                Error.InvalidOperation(SRResources.BaseTypeNotSpecified);
            }

            ValidateAndAddConstraints(ClrBaseType, types);
        }

        public void ValidateAndAddConstraints(Type baseType, IEnumerable<Type> types)
        {
            if (!types.Any())
            {
                ValidateAndAddSingleConstraint(baseType, baseType);

                return;
            }

            foreach (Type t in types)
            {
                ValidateAndAddSingleConstraint(baseType, t);
            }
        }

        private void ValidateAndAddSingleConstraint(Type baseType, Type typeToAdd)
        {
            if (baseType == null)
            {
                throw Error.InvalidOperation(SRResources.NoClrTypeSpecified);
            }
            else if (!baseType.IsAssignableFrom(typeToAdd) && typeToAdd != baseType)
            {
                throw Error.InvalidOperation(SRResources.NotADerivedType, typeToAdd.Name, baseType.Name);
            }

            if (this.Contains(typeToAdd))
            {
                throw Error.InvalidOperation(SRResources.ConstraintAlreadyExists, typeToAdd.Name);
            }

            this.Add(typeToAdd);
        }
    }
}
