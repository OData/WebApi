//-----------------------------------------------------------------------------
// <copyright file="DefaultValueAttributeEdmPropertyConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using Microsoft.AspNet.OData.Common;

namespace Microsoft.AspNet.OData.Builder.Conventions.Attributes
{
    /// <summary>
    /// Sets default value for properties that have <see cref="DefaultValueAttribute"/>
    /// </summary>
    internal class DefaultValueAttributeEdmPropertyConvention : AttributeEdmPropertyConvention<PropertyConfiguration>
    {
        public DefaultValueAttributeEdmPropertyConvention()
            : base(attribute => attribute.GetType() == typeof(DefaultValueAttribute), allowMultiple: false)
        {
        }

        /// <summary>
        /// Sets property default value the on the edm type.
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

            DefaultValueAttribute defaultValueAttribute = attribute as DefaultValueAttribute;
            if (!edmProperty.AddedExplicitly && defaultValueAttribute != null && defaultValueAttribute.Value != null)
            {
                if (edmProperty.Kind == PropertyKind.Primitive)
                {
                    PrimitivePropertyConfiguration primitiveProperty = edmProperty as PrimitivePropertyConfiguration;
                    primitiveProperty.DefaultValueString = defaultValueAttribute.Value.ToString();
                }

                if (edmProperty.Kind == PropertyKind.Enum)
                {
                    EnumPropertyConfiguration enumProperty = edmProperty as EnumPropertyConfiguration;
                    enumProperty.DefaultValueString = defaultValueAttribute.Value.ToString();
                }
            }
        }
    }
}
