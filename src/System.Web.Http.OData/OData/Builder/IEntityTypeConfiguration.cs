// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder
{
    public interface IEntityTypeConfiguration : IStructuralTypeConfiguration
    {
        IEntityTypeConfiguration BaseType { get; }

        IEnumerable<PrimitivePropertyConfiguration> Keys { get; }

        IEnumerable<NavigationPropertyConfiguration> NavigationProperties { get; }

        IEntityTypeConfiguration HasKey(PropertyInfo keyProperty);

        NavigationPropertyConfiguration AddNavigationProperty(PropertyInfo navigationProperty, EdmMultiplicity multiplicity);
    }
}
