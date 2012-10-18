// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using SignalR.Hubs;
using System;

namespace Microsoft.AspNet.Mvc.Facebook.Controllers
{
    [CLSCompliant(false)]
    public class FacebookRealTimeHub : Hub
    {
        public void Send(string data)
        {
            Clients.addMessage(data);
        }
    }
}
