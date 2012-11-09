// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder
{
    /// <summary>
    /// Allows configuration to be performed for a complex type in a model. A ComplexTypeConfiguration can be obtained by using the method <see cref="ODataModelBuilder.ComplexType"/>.
    /// </summary>
    public class ComplexTypeConfiguration : StructuralTypeConfiguration
    {
        /// <summary>
        /// Initializes an instance of <see cref="ComplexTypeConfiguration"/>.
        /// </summary>
        /// <remarks>The default constructor is intended for use by unit testing only.</remarks>
        public ComplexTypeConfiguration()
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="ComplexTypeConfiguration"/>.
        /// <param name="modelBuilder">The <see cref="ODataModelBuilder"/> being used.</param>
        /// <param name="clrType">The backing CLR type for this entity type.</param>
        /// </summary>
        public ComplexTypeConfiguration(ODataModelBuilder modelBuilder, Type clrType)
            : base(modelBuilder, clrType)
        {
        }

        public override EdmTypeKind Kind
        {
            get
            {
                return EdmTypeKind.Complex;
            }
        }
    }
}
