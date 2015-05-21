// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Common;
using Microsoft.OData.Edm;

namespace System.Web.OData.Builder.Conventions.Attributes
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
