//-----------------------------------------------------------------------------
// <copyright file="DerivedTypeConstraintAttributeEdmPropertyConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Query;

namespace Microsoft.AspNet.OData.Builder.Conventions.Attributes
{
    internal class DerivedTypeConstraintAttributeConvention : AttributeEdmPropertyConvention<PropertyConfiguration>
    {
        public DerivedTypeConstraintAttributeConvention() 
            : base(attribute => attribute.GetType() == typeof(DerivedTypeConstraintAttribute), allowMultiple: true)
        {
        }

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
                DerivedTypeConstraintAttribute derivedTypeConstraintAttribute = attribute as DerivedTypeConstraintAttribute;
                edmProperty.DerivedTypeConstraints.AddConstraints(derivedTypeConstraintAttribute.DerivedTypeConstraints);
            }
        }
    }
}
