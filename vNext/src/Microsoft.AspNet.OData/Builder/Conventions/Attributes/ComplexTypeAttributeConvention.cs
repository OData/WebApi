// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.AspNet.OData.Common;

namespace System.Web.OData.Builder.Conventions.Attributes
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
            }
        }
    }
}
