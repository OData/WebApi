//-----------------------------------------------------------------------------
// <copyright file="AutoExpandAttributeEdmTypeConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData.Common;

namespace Microsoft.AspNet.OData.Builder.Conventions.Attributes
{
    /// <summary>
    /// Configures classes that have the <see cref="AutoExpandAttribute"/> to specify all navigation properties are auto expanded.
    /// </summary>
    internal class AutoExpandAttributeEdmTypeConvention : AttributeEdmTypeConvention<StructuralTypeConfiguration>
    {
        public AutoExpandAttributeEdmTypeConvention()
            : base(attribute => attribute.GetType() == typeof(AutoExpandAttribute), allowMultiple: false)
        {
        }

        /// <summary>
        /// Set all navigation properties auto expand.
        /// </summary>
        /// <param name="edmTypeConfiguration">The edm type to configure.</param>
        /// <param name="model">The edm model that this type belongs to.</param>
        /// <param name="attribute">The <see cref="Attribute"/> found on this type.</param>
        public override void Apply(StructuralTypeConfiguration edmTypeConfiguration, ODataConventionModelBuilder model,
            Attribute attribute)
        {
            if (edmTypeConfiguration == null)
            {
                throw Error.ArgumentNull("edmTypeConfiguration");
            }

            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            EntityTypeConfiguration entityTypeConfiguration = edmTypeConfiguration as EntityTypeConfiguration;
            AutoExpandAttribute autoExpandAttribute = attribute as AutoExpandAttribute;
            foreach (var property in entityTypeConfiguration.NavigationProperties)
            {
                if (!property.AddedExplicitly)
                {
                    property.AutomaticallyExpand(autoExpandAttribute.DisableWhenSelectPresent);
                }
            }
        }
    }
}
