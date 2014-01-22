// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.OData.Edm;

namespace System.Web.OData.Builder
{
    internal class NullableEnumTypeConfiguration : IEdmTypeConfiguration
    {
        internal NullableEnumTypeConfiguration(EnumTypeConfiguration enumTypeConfiguration)
        {
            this.ClrType = typeof(Nullable<>).MakeGenericType(enumTypeConfiguration.ClrType);
            this.FullName = enumTypeConfiguration.FullName;
            this.Namespace = enumTypeConfiguration.Namespace;
            this.Name = enumTypeConfiguration.Name;
            this.Kind = enumTypeConfiguration.Kind;
            this.ModelBuilder = enumTypeConfiguration.ModelBuilder;
            this.EnumTypeConfiguration = enumTypeConfiguration;
        }

        /// <summary>
        /// The CLR type associated with the nullable enum type.
        /// </summary>
        public Type ClrType { get; private set; }

        /// <summary>
        /// The fullname (including namespace) of the EdmType.
        /// </summary>
        public string FullName { get; private set; }

        /// <summary>
        /// The namespace of the EdmType.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Namespace", Justification = "Namespace matches the EF naming scheme")]
        public string Namespace { get; private set; }

        /// <summary>
        /// The name of the EdmType.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The kind of the EdmType.
        /// Examples include EntityType, ComplexType, PrimitiveType, CollectionType, EnumType.
        /// </summary>
        public EdmTypeKind Kind { get; private set; }

        /// <summary>
        /// The ODataModelBuilder used to create this IEdmType.
        /// </summary>
        public ODataModelBuilder ModelBuilder { get; private set; }

        /// <summary>
        /// The EnumTypeConfiguration used to create this class.
        /// </summary>
        internal EnumTypeConfiguration EnumTypeConfiguration { get; private set; }
    }
}
