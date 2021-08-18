//-----------------------------------------------------------------------------
// <copyright file="ActionOnDeleteAttributeConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.AspNet.OData.Common;

namespace Microsoft.AspNet.OData.Builder.Conventions.Attributes
{
    internal class ActionOnDeleteAttributeConvention : AttributeEdmPropertyConvention<NavigationPropertyConfiguration>
    {
        public ActionOnDeleteAttributeConvention()
            : base(attribute => attribute.GetType() == typeof(ActionOnDeleteAttribute), allowMultiple: false)
        {
        }

        public override void Apply(NavigationPropertyConfiguration edmProperty,
            StructuralTypeConfiguration structuralTypeConfiguration, Attribute attribute, ODataConventionModelBuilder model)
        {
            if (edmProperty == null)
            {
                throw Error.ArgumentNull("edmProperty");
            }

            ActionOnDeleteAttribute actionOnDelete = attribute as ActionOnDeleteAttribute;
            if (actionOnDelete != null && !edmProperty.AddedExplicitly && edmProperty.DependentProperties.Any())
            {
                edmProperty.OnDeleteAction = actionOnDelete.OnDeleteAction;
            }
        }
    }
}
