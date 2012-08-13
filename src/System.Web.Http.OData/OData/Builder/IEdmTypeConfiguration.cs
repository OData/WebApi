// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace System.Web.Http.OData.Builder
{
    public interface IEdmTypeConfiguration
    {
        Type ClrType { get; }

        string FullName { get; }

        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Namespace", Justification = "Namespace matches the EF naming scheme")]
        string Namespace { get; }

        string Name { get; }
    }
}
