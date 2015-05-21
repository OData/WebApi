// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.
using System.Reflection;
namespace System.Web.OData.Builder.Conventions
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
                entity.IsAbstract = entity.ClrType.GetTypeInfo().IsAbstract;
            }
        }
    }
}
