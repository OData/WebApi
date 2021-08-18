//-----------------------------------------------------------------------------
// <copyright file="ComplexTypeAttributeConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.AspNet.OData.Common;

namespace Microsoft.AspNet.OData.Builder.Conventions.Attributes
{
    internal class ComplexTypeAttributeConvention : AttributeEdmTypeConvention<EntityTypeConfiguration>
    {
        public ComplexTypeAttributeConvention()
            : base(attr => attr.GetType() == typeof(ComplexTypeAttribute), false)
        {
        }

        public override void Apply(EntityTypeConfiguration edmTypeConfiguration, ODataConventionModelBuilder model, 
            Attribute attribute)
        {
            if (edmTypeConfiguration == null)
            {
                throw Error.ArgumentNull("edmTypeConfiguration");
            }

            if (!edmTypeConfiguration.AddedExplicitly)
            {
                PrimitivePropertyConfiguration[] keys = edmTypeConfiguration.Keys.ToArray();
                foreach (PrimitivePropertyConfiguration key in keys)
                {
                    edmTypeConfiguration.RemoveKey(key);
                }

                EnumPropertyConfiguration[] enumKeys = edmTypeConfiguration.EnumKeys.ToArray();
                foreach (EnumPropertyConfiguration key in enumKeys)
                {
                    edmTypeConfiguration.RemoveKey(key);
                }
            }
        }
    }
}
