// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Web.Http.OData.Query;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder.Conventions.Attributes
{
    internal class NotExpandableAttributeEdmPropertyConvention : AttributeEdmPropertyConvention<NavigationPropertyConfiguration>
    {
        public NotExpandableAttributeEdmPropertyConvention()
            : base(attribute => attribute.GetType() == typeof(NotExpandableAttribute), allowMultiple: false)
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
                edmProperty.IsNotExpandable();
            }
        }
    }
}
