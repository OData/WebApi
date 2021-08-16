//-----------------------------------------------------------------------------
// <copyright file="DataServiceQueryExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.OData.Client;

namespace Microsoft.Test.E2E.AspNet.OData.Common
{
    public static class DataServiceQueryExtensions
    {
        public static Task<IEnumerable<T>> ExecuteAsync<T>(this DataServiceQuery<T> self)
        {
            return Task<IEnumerable<T>>.Factory.FromAsync(
                self.BeginExecute,
                self.EndExecute,
                null);
        }
    }
}
