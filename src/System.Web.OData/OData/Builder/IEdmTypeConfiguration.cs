// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.OData.Edm;

namespace System.Web.OData.Builder
{
    /// <summary>
    /// Represents an EdmType
    /// </summary>
    public interface IEdmTypeConfiguration
    {
        /// <summary>
        /// The CLR type associated with the EdmType.
        /// </summary>
        Type ClrType { get; }

        /// <summary>
        /// The fullname (including namespace) of the EdmType.
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// The namespace of the EdmType.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Namespace", Justification = "Namespace matches the EF naming scheme")]
        string Namespace { get; }

        /// <summary>
        /// The name of the EdmType.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The kind of the EdmType.
        /// Examples include EntityType, ComplexType, PrimitiveType, CollectionType, EnumType.
        /// </summary>
        EdmTypeKind Kind { get; }

        /// <summary>
        /// The ODataModelBuilder used to create this IEdmType.
        /// </summary>
        ODataModelBuilder ModelBuilder { get; }
    }
}
