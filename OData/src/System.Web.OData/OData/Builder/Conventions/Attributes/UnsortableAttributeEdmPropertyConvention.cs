// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http;
using System.Web.OData.Query;

namespace System.Web.OData.Builder.Conventions.Attributes
{
    internal class UnsortableAttributeEdmPropertyConvention : AttributeEdmPropertyConvention<PropertyConfiguration>
    {
        public UnsortableAttributeEdmPropertyConvention()
            : base(attribute => attribute.GetType() == typeof(UnsortableAttribute), allowMultiple: false)
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
                edmProperty.IsNotSortable();
            }
        }
    }
}
