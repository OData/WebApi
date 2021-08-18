//-----------------------------------------------------------------------------
// <copyright file="MediaTypeAttributeConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData.Common;

namespace Microsoft.AspNet.OData.Builder.Conventions.Attributes
{
    internal class MediaTypeAttributeConvention : AttributeEdmTypeConvention<EntityTypeConfiguration>
    {
        public MediaTypeAttributeConvention()
            : base(attr => attr.GetType() == typeof(MediaTypeAttribute), false)
        {
        }

        public override void Apply(EntityTypeConfiguration edmTypeConfiguration, ODataConventionModelBuilder model,
            Attribute attribute)
        {
            if (edmTypeConfiguration == null)
            {
                throw Error.ArgumentNull("edmTypeConfiguration");
            }

            edmTypeConfiguration.MediaType();
        }
    }
}
