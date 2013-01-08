// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder
{
    /// <summary>
    /// Represents an <see cref="IEdmComplexType"/> that can be built using <see cref="ODataModelBuilder"/>.
    /// </summary>
    public class ComplexTypeConfiguration<TComplexType> : StructuralTypeConfiguration<TComplexType> where TComplexType : class
    {
        internal ComplexTypeConfiguration(ComplexTypeConfiguration configuration)
            : base(configuration)
        {
        }

        internal ComplexTypeConfiguration(ODataModelBuilder modelBuilder)
            : base(new ComplexTypeConfiguration(modelBuilder, typeof(TComplexType)))
        {
        }
    }
}
