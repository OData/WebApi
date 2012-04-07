// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

namespace System.Net.Http.Internal
{
    // Wrapper around async result to store additional data. This is useful to pass data between BeginXYZ / EndXYZ. 
    internal class AsyncResultWithExtraData<T> : IAsyncResult
    {
        public AsyncResultWithExtraData(IAsyncResult inner, T extraData)
        {
            Inner = inner;
            ExtraData = extraData;
        }

        public IAsyncResult Inner { get; private set; }

        public T ExtraData { get; private set; }

        public object AsyncState
        {
            get { return Inner.AsyncState; }
        }

        public Threading.WaitHandle AsyncWaitHandle
        {
            get { return Inner.AsyncWaitHandle; }
        }

        public bool CompletedSynchronously
        {
            get { return Inner.CompletedSynchronously; }
        }

        public bool IsCompleted
        {
            get { return Inner.IsCompleted; }
        }
    }
}
