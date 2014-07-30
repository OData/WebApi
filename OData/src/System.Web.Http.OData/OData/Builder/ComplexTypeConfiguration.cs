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
        /// Initializes a new instance of the <see cref="ComplexTypeConfiguration"/> class.
        /// </summary>
        /// <remarks>The default constructor is intended for use by unit testing only.</remarks>
        public ComplexTypeConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexTypeConfiguration"/> class.
        /// <param name="modelBuilder">The <see cref="ODataModelBuilder"/> being used.</param>
        /// <param name="clrType">The backing CLR type for this entity type.</param>
        /// </summary>
        public ComplexTypeConfiguration(ODataModelBuilder modelBuilder, Type clrType)
            : base(modelBuilder, clrType)
        {
        }

        /// <inheritdoc />
        public override EdmTypeKind Kind
        {
            get
            {
                return EdmTypeKind.Complex;
            }
        }
    }
}
