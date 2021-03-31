// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Common;
using System;
using System.Collections.Generic;

namespace Microsoft.AspNet.OData.Builder
{
    /// <summary>
    /// Represents an <see cref="Attribute"/> that can be placed on a property or placed on a class to specify the derived type constraints. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DerivedTypeConstraintAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DerivedTypeConstraintAttribute"/> class.
        /// </summary>
        public DerivedTypeConstraintAttribute()
        {
            DerivedTypeConstraints = new HashSet<Type>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DerivedTypeConstraintAttribute"/> class
        /// with the allowed types. 
        /// </summary>
        /// <param name="types">CLR types </param>
        public DerivedTypeConstraintAttribute(params Type[] types)
        {
            DerivedTypeConstraints = new HashSet<Type>();
            foreach (var type in types)
            {
                if (!DerivedTypeConstraints.Add(type))
                {
                    throw Error.InvalidOperation(SRResources.ConstraintAlreadyExists, type.Name);
                }
            }
        }

        /// <summary>
        /// Set of derived type constraints 
        /// </summary>
        public ISet<Type> DerivedTypeConstraints { get; private set; }
    }
}
