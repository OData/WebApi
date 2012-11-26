// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Facebook;
using Microsoft.AspNet.Mvc.Facebook.ModelBinders;

namespace Microsoft.AspNet.Mvc.Facebook
{
    [FacebookContextBinderAttribute]
    public class FacebookContext
    {
        public dynamic SignedRequest { get; set; }
        public string AccessToken { get; set; }
        public string UserId { get; set; }
        public FacebookClient Client { get; set; }
        public FacebookConfiguration Configuration { get; set; }
    }
}
