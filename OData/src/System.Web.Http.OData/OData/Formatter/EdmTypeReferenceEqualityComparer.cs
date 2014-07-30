// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData
{
    /// <summary>
    /// An equality comparer for <see cref="IEdmTypeReference"/>.
    /// </summary>
    internal class EdmTypeReferenceEqualityComparer : IEqualityComparer<IEdmTypeReference>
    {
        public bool Equals(IEdmTypeReference x, IEdmTypeReference y)
        {
            Contract.Assert(x != null);
            return x.IsEquivalentTo(y);
        }

        public int GetHashCode(IEdmTypeReference obj)
        {
            Contract.Assert(obj != null);

            string fullName = obj.FullName();
            if (fullName == null)
            {
                // EdmTypeReferences without an IEdmSchemaElement Definition will all be hashed to 0
                // This is mostly so unit tests don't cause this method to null-ref
                return 0;
            }
            else
            {
                return fullName.GetHashCode();
            }
        }
    }
}
