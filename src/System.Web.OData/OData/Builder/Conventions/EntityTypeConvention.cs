// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.OData.Builder.Conventions
{
    /// <summary>
    /// An <see cref="EntityTypeConvention"/> is used to configure an <see cref="EntityTypeConfiguration"/> in the 
    /// <see cref="ODataConventionModelBuilder"/>.
    /// </summary>
    internal abstract class EntityTypeConvention : IEdmTypeConvention
    {
        protected EntityTypeConvention()
        {
        }

        public void Apply(IEdmTypeConfiguration edmTypeConfiguration, ODataConventionModelBuilder model)
        {
            EntityTypeConfiguration entity = edmTypeConfiguration as EntityTypeConfiguration;
            if (entity != null)
            {
                Apply(entity, model);
            }
        }

        /// <summary>
        /// Applies the convention.
        /// </summary>
        /// <param name="entity">The <see cref="EntityTypeConfiguration"/> to apply the convention on.</param>
        /// <param name="model">The <see cref="ODataModelBuilder"/> instance.</param>
        public abstract void Apply(EntityTypeConfiguration entity, ODataConventionModelBuilder model);
    }
}
