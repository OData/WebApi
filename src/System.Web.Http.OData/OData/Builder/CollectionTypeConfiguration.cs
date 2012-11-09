// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Globalization;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder
{
    /// <summary>
    /// Represents a Collection of some named type.
    /// <example>
    /// Collection(Namespace.Customer) or Collection(Namespace.Address)
    /// </example>
    /// </summary>
    public class CollectionTypeConfiguration : IEdmTypeConfiguration
    {
        private IEdmTypeConfiguration _elementType;
        private Type _clrType;

        /// <summary>
        /// Construct a collection that contains elements of the specified ElementType
        /// and that is represented in .NET using the specified clrType
        /// </summary>
        /// <param name="elementType">The EdmTypeConfiguration of the elements in the collection</param>
        /// <param name="clrType">The type of this collection when manifested in .NET</param>
        public CollectionTypeConfiguration(IEdmTypeConfiguration elementType, Type clrType)
        {
            if (elementType == null)
            {
                throw Error.ArgumentNull("elementType");
            }
            if (clrType == null)
            {
                throw Error.ArgumentNull("clrType");
            }
            _elementType = elementType;
            _clrType = clrType;
        }

        /// <summary>
        /// The EdmTypeConfiguration of elements in this collection
        /// </summary>
        public IEdmTypeConfiguration ElementType
        {
            get { return _elementType; }
        }

        public Type ClrType
        {
            get { return _clrType; }
        }

        public string FullName
        {
            get
            {
                // There is no need to include the Namespace when it comes from the Edm Namespace.
                return Name;
            }
        }

        public string Namespace
        {
            get { return "Edm"; }
        }

        public string Name
        {
            get { return String.Format(CultureInfo.InvariantCulture, "Collection({0})", ElementType.FullName); }
        }

        public EdmTypeKind Kind
        {
            get { return EdmTypeKind.Collection; }
        }

        public ODataModelBuilder ModelBuilder
        {
            get { return _elementType.ModelBuilder; }
        }
    }
}
