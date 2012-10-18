// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.Facebook.Models;

namespace Microsoft.AspNet.Mvc.Facebook.Services
{
    public interface IFacebookUserEvents
    {
        //TODO: (ErikPo) Include which fields?
        void UserChanged(FacebookUser user);
    }
}
