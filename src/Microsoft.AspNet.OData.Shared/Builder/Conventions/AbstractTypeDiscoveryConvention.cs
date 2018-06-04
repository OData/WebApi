﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNet.OData.Builder.Conventions
{
    /// <summary>
    /// <see cref="AbstractTypeDiscoveryConvention"/> to figure out if a structural type is abstract or not.
    /// <remarks>This convention configures all structural types backed by an abstract CLR type as abstract.</remarks>
    /// </summary>
    internal class AbstractTypeDiscoveryConvention : IEdmTypeConvention
    {
        public void Apply(IEdmTypeConfiguration edmTypeConfiguration, ODataConventionModelBuilder model)
        {
            StructuralTypeConfiguration structuralType = edmTypeConfiguration as StructuralTypeConfiguration;
            if (structuralType != null && structuralType.IsAbstract == null)
            {
                structuralType.IsAbstract = TypeHelper.IsAbstract(structuralType.ClrType);
            }
        }
    }
}
