// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc.Facebook.Models;

namespace Microsoft.AspNet.Mvc.Facebook.Services
{
    public class DefaultFacebookObjectStorageService : IFacebookObjectStorageService
    {
        private readonly SortedList<string, FacebookObjectList<FacebookObject>> objects;
        private static DefaultFacebookObjectStorageService instance;

        public DefaultFacebookObjectStorageService()
        {
            objects = new SortedList<string, FacebookObjectList<FacebookObject>>(100);
        }

        public static DefaultFacebookObjectStorageService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DefaultFacebookObjectStorageService();
                }

                return instance;
            }
        }

        public FacebookObjectList<FacebookObject> GetObjects(string userFacebookId)
        {
            if (objects.ContainsKey(userFacebookId))
            {
                return objects[userFacebookId];
            }

            return null;
        }

        public void AddObject(FacebookObject obj)
        {
            if (objects.ContainsKey(obj.FacebookUserId))
            {
                var o = objects[obj.FacebookUserId];
                var foundObject = o.FirstOrDefault(oo => oo.FacebookId == obj.FacebookId);
                if (foundObject == null)
                {
                    o.Add(obj);
                }
            }
        }

        public int UpdateObject(FacebookObject obj)
        {
            if (objects.ContainsKey(obj.FacebookUserId))
            {
                var o = objects[obj.FacebookUserId];
                if (o != null)
                {
                    var foundObject = o.FirstOrDefault(oo => oo.FacebookId == obj.FacebookId);
                    if (foundObject != null)
                    {
                        o.Remove(foundObject);
                    }
                    o.Add(obj);

                    return 1;
                }
            }

            return 0;
        }
    }
}
