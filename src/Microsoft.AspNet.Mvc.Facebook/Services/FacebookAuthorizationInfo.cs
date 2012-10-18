// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc.Facebook.Services
{
    public class FacebookAuthorizationInfo
    {
        public bool IsAuthorized { get; set; }
        public string FacebookId { get; set; }
        public string AccessToken { get; set; }
        public string[] Permissions { get; set; }
    }
}
