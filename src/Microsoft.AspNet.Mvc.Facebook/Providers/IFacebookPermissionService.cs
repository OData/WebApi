// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.Facebook.Providers
{
    public interface IFacebookPermissionService
    {
        IEnumerable<string> GetUserPermissions(string userId, string accessToken);
    }
}
