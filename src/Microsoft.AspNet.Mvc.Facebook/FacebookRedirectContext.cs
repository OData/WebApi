// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.Facebook.ModelBinders;

namespace Microsoft.AspNet.Mvc.Facebook
{
    /// <summary>
    /// Provides access to the data redirected from <see cref="Microsoft.AspNet.Mvc.Facebook.Authorization.FacebookAuthorizeFilter"/>.
    /// </summary>
    [FacebookRedirectContextBinder]
    public class FacebookRedirectContext
    {
        /// <summary>
        /// Gets or sets the origin URL.
        /// </summary>
        public string OriginUrl { get; set; }

        /// <summary>
        /// Gets or sets the redirect URL.
        /// </summary>
        public string RedirectUrl { get; set; }

        /// <summary>
        /// Gets or sets the required permissions specified on <see cref="FacebookAuthorizeAttribute"/>.
        /// </summary>
        public string[] RequiredPermissions { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="FacebookConfiguration"/>.
        /// </summary>
        public FacebookConfiguration Configuration { get; set; }
    }
}