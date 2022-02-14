//-----------------------------------------------------------------------------
// <copyright file="DataServiceContextAsyncExtension.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.OData.Client;

namespace Microsoft.Test.E2E.AspNet.OData.Common
{
    public static class DataServiceContextAsyncExtension
    {

        public static Task<DataServiceResponse> SaveChangesAsync(this DataServiceContext self)
        {
            return Task<DataServiceResponse>.Factory.FromAsync(
                self.BeginSaveChanges,
                self.EndSaveChanges,
                null);
        }

        public static Task<DataServiceResponse> SaveChangesAsync(this DataServiceContext self, SaveChangesOptions options)
        {
            return Task<DataServiceResponse>.Factory.FromAsync<SaveChangesOptions>(
                self.BeginSaveChanges,
                self.EndSaveChanges,
                options,
                null);
        }

        public static Task<QueryOperationResponse> LoadPropertyAsync(this DataServiceContext self, object entity, string propertyName)
        {
            return Task<QueryOperationResponse>.Factory.FromAsync(
                self.BeginLoadProperty,
                self.EndLoadProperty,
                entity,
                propertyName,
                null);
        }

        public static Task<IEnumerable<TElement>> ExecuteAsync<TElement>(this DataServiceContext self,
            Uri uri, string httpMethod, bool singleResult, params OperationParameter[] operationParameters)
        {
            IAsyncResult asyncResult = self.BeginExecute<TElement>(uri, null, null, httpMethod, singleResult, operationParameters);

            return Task<IEnumerable<TElement>>.Factory.FromAsync(
                asyncResult,
                self.EndExecute<TElement>);
        }

        public static Task<OperationResponse> ExecuteAsync(this DataServiceContext self,
            Uri uri, string httpMethod, params OperationParameter[] operationParameters)
        {
            IAsyncResult asyncResult = self.BeginExecute(uri, null, null, httpMethod, operationParameters);

            return Task<OperationResponse>.Factory.FromAsync(
                asyncResult,
                self.EndExecute);
        }

        public static Task<DataServiceResponse> ExecuteBatchAsync(this DataServiceContext self,
            params DataServiceRequest[] queries)
        {
            IAsyncResult asyncResult = self.BeginExecuteBatch(null, null, queries);

            return Task<DataServiceResponse>.Factory.FromAsync(
                asyncResult,
                self.EndExecuteBatch);
        }
    }
}
