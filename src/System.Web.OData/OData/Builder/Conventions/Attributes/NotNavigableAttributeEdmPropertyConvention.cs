// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Query;

namespace System.Web.Http.OData.Builder.Conventions.Attributes
{
    internal class NotNavigableAttributeEdmPropertyConvention : AttributeEdmPropertyConvention<NavigationPropertyConfiguration>
    {
        public NotNavigableAttributeEdmPropertyConvention()
            : base(attribute => attribute.GetType() == typeof(NotNavigableAttribute), allowMultiple: false)
        {
        }

        public override void Apply(NavigationPropertyConfiguration edmProperty, StructuralTypeConfiguration structuralTypeConfiguration, Attribute attribute)
        {
            if (edmProperty == null)
            {
                throw Error.ArgumentNull("edmProperty");
            }

            if (!edmProperty.AddedExplicitly)
            {
                edmProperty.IsNotNavigable();
            }
        }
    }
}
