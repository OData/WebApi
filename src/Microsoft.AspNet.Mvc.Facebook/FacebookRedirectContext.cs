// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.Facebook.ModelBinders;

namespace Microsoft.AspNet.Mvc.Facebook
{
    [FacebookRedirectContextBinder]
    public class FacebookRedirectContext
    {
        public string OriginUrl { get; set; }
        public string RedirectUrl { get; set; }
        public string[] RequiredPermissions { get; set; }
        public FacebookConfiguration Configuration { get; set; }
    }
}
