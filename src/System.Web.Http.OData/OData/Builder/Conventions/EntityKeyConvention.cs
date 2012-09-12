// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;

namespace System.Web.Http.OData.Builder.Conventions
{
    public class EntityKeyConvention : EntityTypeConvention
    {
        public override void Apply(IEntityTypeConfiguration entity, ODataModelBuilder model)
        {
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }

            PropertyInfo key = ConventionsHelpers.GetKeyProperty(entity.ClrType);
            if (key != null && !entity.IgnoredProperties.Contains(key))
            {
                entity.HasKey(key);
            }
        }
    }
}
