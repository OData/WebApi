// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http;
using System.Web.OData.Query;

namespace System.Web.OData.Builder.Conventions.Attributes
{
    internal class NonFilterableAttributeEdmPropertyConvention : AttributeEdmPropertyConvention<PropertyConfiguration>
    {
        public NonFilterableAttributeEdmPropertyConvention()
            : base(attribute => attribute.GetType() == typeof(NonFilterableAttribute), allowMultiple: false)
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
                edmProperty.IsNonFilterable();
            }
        }
    }
}
