// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using Microsoft.OData.Edm;

namespace System.Web.OData.Builder
{
    /// <summary>
    /// Represents an <see cref="IEdmComplexType"/> that can be built using <see cref="ODataModelBuilder"/>.
    /// </summary>
    public class ComplexTypeConfiguration<TComplexType> : StructuralTypeConfiguration<TComplexType> where TComplexType : class
    {
        private ComplexTypeConfiguration _configuration;
        private ODataModelBuilder _modelBuilder;

        internal ComplexTypeConfiguration(ComplexTypeConfiguration configuration)
            : base(configuration)
        {
        }

        internal ComplexTypeConfiguration(ODataModelBuilder modelBuilder)
            : this(modelBuilder, new ComplexTypeConfiguration(modelBuilder, typeof(TComplexType)))
        {
        }

        internal ComplexTypeConfiguration(ODataModelBuilder modelBuilder, ComplexTypeConfiguration configuration)
            : base(configuration)
        {
            Contract.Assert(modelBuilder != null);
            Contract.Assert(configuration != null);

            _modelBuilder = modelBuilder;
            _configuration = configuration;
        }

        /// <summary>
        /// Marks this complex type as abstract.
        /// </summary>
        /// <returns>Returns itself so that multiple calls can be chained.</returns>
        public ComplexTypeConfiguration<TComplexType> Abstract()
        {
            _configuration.IsAbstract = true;
            return this;
        }

        /// <summary>
        /// Gets the base type of this complex type.
        /// </summary>
        public ComplexTypeConfiguration BaseType
        {
            get
            {
                return _configuration.BaseType;
            }
        }

        /// <summary>
        /// Sets the base type of this complex type to <c>null</c> meaning that this complex type
        /// does not derive from anything.
        /// </summary>
        /// <returns>Returns itself so that multiple calls can be chained.</returns>
        public ComplexTypeConfiguration<TComplexType> DerivesFromNothing()
        {
            _configuration.DerivesFromNothing();
            return this;
        }

        /// <summary>
        /// Sets the base type of this complex type.
        /// </summary>
        /// <typeparam name="TBaseType">The base complex type.</typeparam>
        /// <returns>Returns itself so that multiple calls can be chained.</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "typeof(TBaseType) is used and getting it as a generic argument is cleaner")]
        public ComplexTypeConfiguration<TComplexType> DerivesFrom<TBaseType>() where TBaseType : class
        {
            ComplexTypeConfiguration<TBaseType> baseEntityType = _modelBuilder.ComplexType<TBaseType>();
            _configuration.DerivesFrom(baseEntityType._configuration);
            return this;
        }
    }
}
