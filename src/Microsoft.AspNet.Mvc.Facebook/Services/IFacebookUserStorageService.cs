// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.Facebook.Models;

namespace Microsoft.AspNet.Mvc.Facebook.Services
{
    public interface IFacebookUserStorageService
    {
        FacebookUser GetUser(string facebookId);
        void AddUser(FacebookUser user);
        int UpdateUser(FacebookUser user);
        //TODO: (ErikPo) Move these into their own interface
        void SetPermissions(string facebookId, string[] permission);
        string[] GetPermissions(string facebookId);
    }
}
