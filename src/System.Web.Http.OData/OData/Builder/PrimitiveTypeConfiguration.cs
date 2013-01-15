// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder
{
    /// <summary>
    /// Represents a PrimitiveType
    /// </summary>
    public class PrimitiveTypeConfiguration : IEdmTypeConfiguration
    {
        private Type _clrType;
        private IEdmPrimitiveType _edmType;
        private ODataModelBuilder _builder;

        /// <summary>
        /// This constructor is public only for unit testing purposes.
        /// To get a PrimitiveTypeConfiguration use ODataModelBuilder.GetTypeConfigurationOrNull(Type)
        /// </summary>
        public PrimitiveTypeConfiguration(ODataModelBuilder builder, IEdmPrimitiveType edmType, Type clrType)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull("builder");
            }
            if (edmType == null)
            {
                throw Error.ArgumentNull("edmType");
            }
            if (clrType == null)
            {
                throw Error.ArgumentNull("clrType");
            }
            _builder = builder;
            _clrType = clrType;
            _edmType = edmType;
        }

        /// <summary>
        /// Gets the backing CLR type of this EDM type.
        /// </summary>
        public Type ClrType
        {
            get { return _clrType; }
        }

        /// <summary>
        /// Gets the full name of this EDM type.
        /// </summary>
        public string FullName
        {
            get { return _edmType.FullName(); }
        }

        /// <summary>
        ///  Gets the namespace of this EDM type.
        /// </summary>
        public string Namespace
        {
            get { return _edmType.Namespace; }
        }

        /// <summary>
        /// Gets the name of this EDM type.
        /// </summary>
        public string Name
        {
            get { return _edmType.Name; }
        }

        /// <summary>
        /// Gets the <see cref="EdmTypeKind"/> of this EDM type.
        /// </summary>
        public EdmTypeKind Kind
        {
            get { return EdmTypeKind.Primitive; }
        }

        /// <summary>
        /// Gets the <see cref="ODataModelBuilder"/> used to create this configuration.
        /// </summary>
        public ODataModelBuilder ModelBuilder
        {
            get { return _builder; }
        }

        /// <summary>
        /// Returns the IEdmPrimitiveType associated with this PrimitiveTypeConfiguration
        /// </summary>
        public IEdmPrimitiveType EdmPrimitiveType
        {
            get { return _edmType; }
        }
    }
}
