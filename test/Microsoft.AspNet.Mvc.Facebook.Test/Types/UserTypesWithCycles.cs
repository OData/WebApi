// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc.Facebook.Test.Types
{
    public class UserWithUserFriends
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public FacebookGroupConnection<UserWithUserFriends> Friends { get; set; }
    }

    public class UserContainingFriendsWithCycle
    {
        public string Id { get; set; }
        public FacebookGroupConnection<UserWithUserFriends> Friends { get; set; }
    }
}
