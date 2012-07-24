// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ServiceModel;
using System.ServiceModel.Channels;

namespace System.Web.Http.SelfHost.ServiceModel.Channels
{
    internal interface IChannelAcceptor<TChannel> : ICommunicationObject
        where TChannel : class, IChannel
    {
        TChannel AcceptChannel(TimeSpan timeout);

        IAsyncResult BeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state);

        TChannel EndAcceptChannel(IAsyncResult result);

        bool WaitForChannel(TimeSpan timeout);

        IAsyncResult BeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state);

        bool EndWaitForChannel(IAsyncResult result);
    }
}
