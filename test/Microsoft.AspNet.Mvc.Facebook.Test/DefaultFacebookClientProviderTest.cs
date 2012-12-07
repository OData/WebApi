// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Facebook;
using Microsoft.AspNet.Mvc.Facebook.Providers;
using Microsoft.TestCommon;

namespace Microsoft.AspNet.Mvc.Facebook.Test
{
    public class DefaultFacebookClientProviderTest
    {
        [Fact]
        public void Constructor_ThrowsArgumentNullException()
        {
            Assert.ThrowsArgumentNull(() => new DefaultFacebookClientProvider(null), "configuration");
        }

        [Fact]
        public void CreateClient_ReturnsClientWithAppIdAndAppSecret()
        {
            string appId = "654321";
            string appSecret = "abcdefg123";
            FacebookConfiguration config = new FacebookConfiguration
            {
                AppId = appId,
                AppSecret = appSecret
            };
            DefaultFacebookClientProvider clientProvider = new DefaultFacebookClientProvider(config);
            FacebookClient client = clientProvider.CreateClient();

            Assert.Equal(appId, client.AppId);
            Assert.Equal(appSecret, client.AppSecret);
        }
    }
}