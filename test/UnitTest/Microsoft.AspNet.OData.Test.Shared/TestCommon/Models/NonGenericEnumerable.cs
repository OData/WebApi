//-----------------------------------------------------------------------------
// <copyright file="NonGenericEnumerable.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;

namespace Microsoft.AspNet.OData.Test.Common.Models
{
    public class NonGenericEnumerable : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
