// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Threading;

namespace System.Web.Mvc.Async
{
    internal static class AsyncUtil
    {
        public static AsyncCallback WrapCallbackForSynchronizedExecution(AsyncCallback callback, SynchronizationContext syncContext)
        {
            if (callback == null || syncContext == null)
            {
                return callback;
            }

            AsyncCallback newCallback = delegate(IAsyncResult asyncResult)
            {
                if (asyncResult.CompletedSynchronously)
                {
                    callback(asyncResult);
                }
                else
                {
                    // Only take the application lock if this request completed asynchronously,
                    // else we might end up in a deadlock situation.
                    syncContext.Sync(() => callback(asyncResult));
                }
            };

            return newCallback;
        }
    }
}
