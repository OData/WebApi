// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.Facebook.Providers;
using Microsoft.TestCommon;

namespace Microsoft.AspNet.Mvc.Facebook.Test
{
    public class DefaultFacebookPermissionServiceTest
    {
        [Fact]
        public void Constructor_ThrowsArgumentNullException()
        {
            Assert.ThrowsArgumentNull(() =>
            {
                new DefaultFacebookPermissionService(null);
            },
            "configuration");
        }

        [Fact]
        public void GetUserPermissions_ThrowsArgumentNullException()
        {
            FacebookConfiguration config = new FacebookConfiguration();
            config.ClientProvider = new DefaultFacebookClientProvider(config);
            DefaultFacebookPermissionService permissionService = new DefaultFacebookPermissionService(config);

            Assert.ThrowsArgumentNull(() =>
            {
                permissionService.GetUserPermissions(null, "accessToken");
            },
            "userId");
        }
    }
}
