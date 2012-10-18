// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.Mvc.Facebook.Models;

namespace Microsoft.AspNet.Mvc.Facebook.Services
{
    public static class FacebookUserEvents
    {
        private static readonly List<IFacebookUserEvents> userEvents = new List<IFacebookUserEvents>();

        public static void RegisterUserEventListener(IFacebookUserEvents userEvent)
        {
            userEvents.Add(userEvent);
        }

        public static void FireUserChangedEvent(FacebookUser user)
        {
            foreach (var userEvent in userEvents)
            {
                userEvent.UserChanged(user);
            }
        }
    }
}
