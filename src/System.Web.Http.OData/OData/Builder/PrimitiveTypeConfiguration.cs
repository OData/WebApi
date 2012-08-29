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

        public Type ClrType
        {
            get { return _clrType; }
        }

        public string FullName
        {
            get { return _edmType.FullName(); }
        }

        public string Namespace
        {
            get { return _edmType.Namespace; }
        }

        public string Name
        {
            get { return _edmType.Name; }
        }

        public EdmTypeKind Kind
        {
            get { return EdmTypeKind.Primitive; }
        }

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
