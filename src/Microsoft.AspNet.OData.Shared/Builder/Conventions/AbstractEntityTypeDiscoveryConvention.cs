//-----------------------------------------------------------------------------
// <copyright file="AbstractEntityTypeDiscoveryConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNet.OData.Builder.Conventions
{
    /// <summary>
    /// <see cref="EntityTypeConvention"/> to figure out if an entity is abstract or not.
    /// <remarks>This convention configures all entity types backed by an abstract CLR type as abstract entities.</remarks>
    /// </summary>
    internal class AbstractEntityTypeDiscoveryConvention : EntityTypeConvention
    {
        public override void Apply(EntityTypeConfiguration entity, ODataConventionModelBuilder model)
        {
            if (entity.IsAbstract == null)
            {
                entity.IsAbstract = entity.ClrType.IsAbstract;
            }
        }
    }
}
