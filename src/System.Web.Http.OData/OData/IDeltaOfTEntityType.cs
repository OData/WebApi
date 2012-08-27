// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace System.Web.Http.OData
{
    /// <summary>
    /// This interface extends <see cref="IDelta"/> with strongly typed methods for manipulating a particular
    /// <typeparamref name="TEntityType"/> Type of Delta.
    /// </summary>
    public interface IDelta<TEntityType> : IDelta where TEntityType : class, new()
    {
        void CopyChangedValues(TEntityType original);

        void CopyUnchangedValues(TEntityType original);

        void Patch(TEntityType original);

        void Put(TEntityType original);

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Not appropriate to be a property")]
        TEntityType GetEntity();
    }
}
