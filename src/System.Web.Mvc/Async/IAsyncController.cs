// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Routing;

namespace System.Web.Mvc.Async
{
    public interface IAsyncController : IController
    {
        IAsyncResult BeginExecute(RequestContext requestContext, AsyncCallback callback, object state);
        void EndExecute(IAsyncResult asyncResult);
    }
}
