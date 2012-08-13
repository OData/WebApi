// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.OData.Builder.Conventions
{
    public abstract class EntityTypeConvention : IEdmTypeConvention
    {
        public void Apply(IEdmTypeConfiguration edmTypeConfiguration, ODataModelBuilder model)
        {
            IEntityTypeConfiguration entity = edmTypeConfiguration as IEntityTypeConfiguration;
            if (entity != null)
            {
                Apply(entity, model);
            }
        }

        public abstract void Apply(IEntityTypeConfiguration entity, ODataModelBuilder model);
    }
}
