// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;

namespace System.Web.Http.OData.Builder.Conventions.Attributes
{
    internal class ActionOnDeleteAttributeConvention : AttributeEdmPropertyConvention<NavigationPropertyConfiguration>
    {
        public ActionOnDeleteAttributeConvention()
            : base(attribute => attribute.GetType() == typeof(ActionOnDeleteAttribute), allowMultiple: false)
        {
        }

        public override void Apply(NavigationPropertyConfiguration edmProperty,
            StructuralTypeConfiguration structuralTypeConfiguration, Attribute attribute)
        {
            if (edmProperty == null)
            {
                throw Error.ArgumentNull("edmProperty");
            }

            ActionOnDeleteAttribute actionOnDelete = attribute as ActionOnDeleteAttribute;
            if (actionOnDelete != null && !edmProperty.AddedExplicitly && edmProperty.ReferentialConstraint.Any())
            {
                edmProperty.OnDeleteAction = actionOnDelete.OnDeleteAction;
            }
        }
    }
}
