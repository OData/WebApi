// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.Facebook.Models;

namespace Microsoft.AspNet.Mvc.Facebook.Services
{
    public interface IFacebookObjectStorageService
    {
        FacebookObjectList<FacebookObject> GetObjects(string userFacebookId);
        void AddObject(FacebookObject obj);
        int UpdateObject(FacebookObject obj);
    }
}
