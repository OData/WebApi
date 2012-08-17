// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.OData.Builder.Conventions
{
    /// <summary>
    /// An <see cref="EntityTypeConvention"/> is used to configure an <see cref="IEntityTypeConfiguration"/> in the 
    /// <see cref="ODataConventionModelBuilder"/>.
    /// </summary>
    public abstract class EntityTypeConvention : IEdmTypeConvention
    {
        protected EntityTypeConvention()
        {
        }

        public void Apply(IEdmTypeConfiguration edmTypeConfiguration, ODataModelBuilder model)
        {
            IEntityTypeConfiguration entity = edmTypeConfiguration as IEntityTypeConfiguration;
            if (entity != null)
            {
                Apply(entity, model);
            }
        }

        /// <summary>
        /// Applies the convention.
        /// </summary>
        /// <param name="entity">The <see cref="IEntityTypeConfiguration"/> to apply the convention on.</param>
        /// <param name="model">The <see cref="ODataModelBuilder"/> instance.</param>
        public abstract void Apply(IEntityTypeConfiguration entity, ODataModelBuilder model);
    }
}
