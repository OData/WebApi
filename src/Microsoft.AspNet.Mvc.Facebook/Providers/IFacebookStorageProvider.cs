// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc.Facebook.Providers
{
    public interface IFacebookStorageProvider
    {
        Task<object> GetAsync(object id);
        Task<bool> AddOrUpdateAsync(object id, object value);
        Task<bool> DeleteAsync(object id);
    }
}
