// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.OData.Edm;

namespace System.Web.OData.Builder
{
    /// <summary>
    /// Allows configuration to be performed for a complex type in a model. A <see cref="ComplexTypeConfiguration"/>
    /// can be obtained by using the method <see cref="ODataModelBuilder.ComplexType"/>.
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
            get { return EdmTypeKind.Complex; }
        }

        /// <summary>
        /// Gets or sets the base type of this complex type.
        /// </summary>
        public virtual ComplexTypeConfiguration BaseType
        {
            get
            {
                return BaseTypeInternal as ComplexTypeConfiguration;
            }
            set
            {
                DerivesFrom(value);
            }
        }

        /// <summary>
        /// Marks this complex type as abstract.
        /// </summary>
        /// <returns>Returns itself so that multiple calls can be chained.</returns>
        public virtual ComplexTypeConfiguration Abstract()
        {
            AbstractImpl();
            return this;
        }

        /// <summary>
        /// Sets the base type of this complex type to <c>null</c> meaning that this complex type
        /// does not derive from anything.
        /// </summary>
        /// <returns>Returns itself so that multiple calls can be chained.</returns>
        public virtual ComplexTypeConfiguration DerivesFromNothing()
        {
            DerivesFromNothingImpl();
            return this;
        }

        /// <summary>
        /// Sets the base type of this complex type.
        /// </summary>
        /// <param name="baseType">The base complex type.</param>
        /// <returns>Returns itself so that multiple calls can be chained.</returns>
        public virtual ComplexTypeConfiguration DerivesFrom(ComplexTypeConfiguration baseType)
        {
            DerivesFromImpl(baseType);
            return this;
        }
    }
}
