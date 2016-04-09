// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData
{
    internal class FunctionImportComparer : IEqualityComparer<IEdmFunctionImport>
    {
        public bool Equals(IEdmFunctionImport left, IEdmFunctionImport right)
        {
            if (Object.ReferenceEquals(left, right))
            {
                return true;
            }

            if (Object.ReferenceEquals(left, null) || Object.ReferenceEquals(right, null))
            {
                return false;
            }

            return left.Name == right.Name;
        }

        public int GetHashCode(IEdmFunctionImport functionImport)
        {
            if (Object.ReferenceEquals(functionImport, null))
            {
                return 0;
            }

            return functionImport.Function.Name == null ? 0 : functionImport.Function.Name.GetHashCode();
        }
    }
}
