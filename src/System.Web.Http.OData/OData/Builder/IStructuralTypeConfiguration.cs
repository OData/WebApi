// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;

namespace System.Web.Http.OData.Builder
{
    public interface IStructuralTypeConfiguration : IEdmTypeConfiguration
    {
        IEnumerable<PropertyConfiguration> Properties { get; }

        IEnumerable<PropertyInfo> IgnoredProperties { get; }

        PrimitivePropertyConfiguration AddProperty(PropertyInfo propertyInfo);

        ComplexPropertyConfiguration AddComplexProperty(PropertyInfo propertyInfo);

        CollectionPropertyConfiguration AddCollectionProperty(PropertyInfo propertyInfo);

        void RemoveProperty(PropertyInfo propertyInfo);
    }
}
