// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace System.Web.Http.OData
{
    /// <summary>
    /// IDelta of TEntityType extends IDelta with strongly typed methods for manipulating a particular
    /// <typeparamref name="TEntityType"/> type of Delta.
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

    /// <summary>
    /// IDelta allows and tracks changes to an object.
    /// </summary>
    public interface IDelta
    {
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Not appropriate to be a property")]
        IEnumerable<string> GetChangedPropertyNames();

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Not appropriate to be a property")]
        IEnumerable<string> GetUnchangedPropertyNames();

        bool TrySetPropertyValue(string name, object value);

        [SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate", Justification = "Generics not appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#", Justification = "Out param is appropriate here")]
        bool TryGetPropertyValue(string name, out object value);

        void Clear();
    }
}
