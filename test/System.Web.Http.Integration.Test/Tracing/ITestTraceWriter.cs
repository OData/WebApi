// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.Tracing
{
    public interface ITestTraceWriter : ITraceWriter
    {
        // Asks the trace writer to initialize for another test iteration
        void Start();

        // Tells the trace writer to stop tracing
        void Finish();

        // Returns true if the tracer writer received any trace requests
        bool DidReceiveTraceRequests { get; }
    }
}
