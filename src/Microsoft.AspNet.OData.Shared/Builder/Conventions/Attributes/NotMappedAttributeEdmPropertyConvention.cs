//-----------------------------------------------------------------------------
// <copyright file="NotMappedAttributeEdmPropertyConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Builder.Conventions.Attributes
{
    /// <summary>
    /// Ignores properties with the NotMappedAttribute from <see cref="IEdmStructuredType"/>.
    /// </summary>
    internal class NotMappedAttributeConvention : AttributeEdmPropertyConvention<PropertyConfiguration>
    {
        // .net 4.5 NotMappedAttribute has the same name.
        private const string EntityFrameworkNotMappedAttributeTypeName = "System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute";

        private static Func<Attribute, bool> _filter = attribute =>
        {
            return attribute.GetType().FullName.Equals(EntityFrameworkNotMappedAttributeTypeName, StringComparison.Ordinal);
        };

        public NotMappedAttributeConvention()
            : base(_filter, allowMultiple: false)
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

            if (structuralTypeConfiguration == null)
            {
                throw Error.ArgumentNull("structuralTypeConfiguration");
            }

            if (!edmProperty.AddedExplicitly)
            {
                structuralTypeConfiguration.RemoveProperty(edmProperty.PropertyInfo);
            }
        }
    }
}
