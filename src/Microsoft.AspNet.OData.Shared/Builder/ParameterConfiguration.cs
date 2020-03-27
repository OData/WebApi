// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;

namespace Microsoft.AspNet.OData.Builder
{
    /// <summary>
    /// Represents a parameter to a Operation
    /// </summary>
    public abstract class ParameterConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterConfiguration"/> class.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="parameterType">The EDM type of the parameter.</param>
        protected ParameterConfiguration(string name, IEdmTypeConfiguration parameterType)
        {
            if (name == null)
            {
                throw Error.ArgumentNull("name");
            }
            if (parameterType == null)
            {
                throw Error.ArgumentNull("parameterType");
            }

            Name = name;
            TypeConfiguration = parameterType;

            DerivedTypeConstraints = new DerivedTypeConstraintConfiguration();

            Type elementType;
            Nullable = TypeHelper.IsCollection(parameterType.ClrType, out elementType)
                ? EdmLibHelpers.IsNullable(elementType)
                : EdmLibHelpers.IsNullable(parameterType.ClrType);
        }

        /// <summary>
        /// The name of the parameter
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// The type of the parameter
        /// </summary>
        public IEdmTypeConfiguration TypeConfiguration { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether this parameter is nullable or not.
        /// </summary>
        public bool Nullable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this parameter is optional or not.
        /// </summary>
        public bool IsOptional { get; protected set; }

        /// <summary>
        /// Gets or sets a default value for optional parameter.
        /// </summary>
        public string DefaultValue { get; protected set; }

        /// <summary>
        /// Configuration that lists derived types that are allowed for the property. 
        /// </summary>
        public DerivedTypeConstraintConfiguration DerivedTypeConstraints { get; private set; }

        /// <summary>
        /// Adds subtypes to the list of derived type constraints.
        /// </summary>
        /// <param name="subtypes">The subtypes for which the constraint needs to be added.</param>
        /// <returns>Updated configuration object.</returns>
        public ParameterConfiguration HasDerivedTypeConstraints(params Type[] subtypes)
        {
            DerivedTypeConstraints.AddConstraints(subtypes);
            return this;
        }

        /// <summary>
        /// Adds TDerivedType to the list of derived type constraints.
        /// </summary>
        /// <returns>Updated configuration object.</returns>
        public ParameterConfiguration HasDerivedTypeConstraint<TDerivedType>()
        {
            DerivedTypeConstraints.AddConstraint<TDerivedType>();
            return this;
        }

        /// <summary>
        /// Sets the optional value as true.
        /// </summary>
        public ParameterConfiguration Optional()
        {
            IsOptional = true;
            return this;
        }

        /// <summary>
        /// Sets the optional value as false.
        /// </summary>
        public ParameterConfiguration Required()
        {
            IsOptional = false;
            return this;
        }

        /// <summary>
        /// Sets the optional value as true, default value as given value.
        /// </summary>
        /// <param name="defaultValue">The default value.</param>
        public ParameterConfiguration HasDefaultValue(string defaultValue)
        {
            IsOptional = true;
            DefaultValue = defaultValue;
            return this;
        }
    }
}
