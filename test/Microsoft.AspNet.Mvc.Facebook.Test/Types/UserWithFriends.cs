// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc.Facebook.Test.Types
{
    public class UserWithFriends
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public FacebookConnection<FacebookPicture> Picture { get; set; }
        public FacebookGroupConnection<SimpleUser> Friends { get; set; }
    }
}
