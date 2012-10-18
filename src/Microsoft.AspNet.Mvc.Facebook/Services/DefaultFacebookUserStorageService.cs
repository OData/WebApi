// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.Mvc.Facebook.Models;

namespace Microsoft.AspNet.Mvc.Facebook.Services
{
    public class DefaultFacebookUserStorageService : IFacebookUserStorageService, IFacebookUserEvents
    {
        private readonly SortedList<string, FacebookUser> users;
        private readonly SortedList<string, string[]> permissions;
        private static DefaultFacebookUserStorageService instance;

        public DefaultFacebookUserStorageService()
        {
            users = new SortedList<string, FacebookUser>(100);
            permissions = new SortedList<string, string[]>(100);
        }

        public static DefaultFacebookUserStorageService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DefaultFacebookUserStorageService();
                }

                return instance;
            }
        }

        public FacebookUser GetUser(string facebookId)
        {
            if (users.ContainsKey(facebookId))
            {
                return users[facebookId];
            }

            return null;
        }

        public void AddUser(FacebookUser user)
        {
            if (!users.ContainsKey(user.FacebookId))
            {
                users.Add(user.FacebookId, user);
            }
        }

        public int UpdateUser(FacebookUser user)
        {
            if (users.ContainsKey(user.FacebookId))
            {
                users.Remove(user.FacebookId);
                users.Add(user.FacebookId, user);

                return 1;
            }

            return 0;
        }

        public void UserChanged(FacebookUser user)
        {
            UpdateUser(user);
        }

        //TODO: (ErikPo) Move these into their own interface

        public void SetPermissions(string facebookId, string[] permissions)
        {
            this.permissions[facebookId] = permissions;
        }

        public string[] GetPermissions(string facebookId)
        {
            if (this.permissions.ContainsKey(facebookId))
            {
                return this.permissions[facebookId];
            }
            else
            {
                return new string[0];
            }
        }
    }
}
