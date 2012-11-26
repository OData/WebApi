// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Facebook;
using Newtonsoft.Json;

namespace Microsoft.AspNet.Mvc.Facebook.Providers
{
    public class DefaultFacebookClientProvider : IFacebookClientProvider
    {
        private FacebookConfiguration _config;

        public DefaultFacebookClientProvider(FacebookConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            _config = configuration;
        }

        public virtual FacebookClient CreateClient()
        {
            FacebookClient client = new FacebookClient();
            client.AppId = _config.AppId;
            client.AppSecret = _config.AppSecret;
            client.SetJsonSerializers(JsonConvert.SerializeObject, JsonConvert.DeserializeObject);
            return client;
        }
    }
}
