//-----------------------------------------------------------------------------
// <copyright file="RequiredAttributeEdmPropertyConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Builder.Conventions.Attributes
{
    /// <summary>
    /// Marks properties that have <see cref="RequiredAttribute"/> as non-optional on their edm type.
    /// </summary>
    internal class RequiredAttributeEdmPropertyConvention : AttributeEdmPropertyConvention<PropertyConfiguration>
    {
        public RequiredAttributeEdmPropertyConvention()
            : base(attribute => attribute.GetType() == typeof(RequiredAttribute), allowMultiple: false)
        {
        }

        /// <summary>
        /// Marks the property non-optional on the edm type.
        /// </summary>
        /// <param name="edmProperty">The edm property.</param>
        /// <param name="structuralTypeConfiguration">The edm type being configured.</param>
        /// <param name="attribute">The <see cref="Attribute"/> found.</param>
        /// <param name="model">The ODataConventionModelBuilder used to build the model.</param>
        public override void Apply(PropertyConfiguration edmProperty,
            StructuralTypeConfiguration structuralTypeConfiguration,
            Attribute attribute,
            ODataConventionModelBuilder model)
        {
            if (edmProperty == null)
            {
                throw Error.ArgumentNull("edmProperty");
            }

            if (!edmProperty.AddedExplicitly)
            {
                StructuralPropertyConfiguration structuralProperty = edmProperty as StructuralPropertyConfiguration;
                if (structuralProperty != null)
                {
                    structuralProperty.OptionalProperty = false;
                }

                NavigationPropertyConfiguration navigationProperty = edmProperty as NavigationPropertyConfiguration;
                if (navigationProperty != null && navigationProperty.Multiplicity != EdmMultiplicity.Many)
                {
                    navigationProperty.Required();
                }
            }
        }
    }
}
