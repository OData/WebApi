// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if !NETCORE
using System;
using System.Net.Http;
using System.Web.Http.Tracing;

namespace Microsoft.Test.E2E.AspNet.OData.Common
{
    public class PlaceholderTraceWriter : ITraceWriter
    {
        public void Trace(HttpRequestMessage request, string category, TraceLevel level, Action<TraceRecord> traceAction)
        {
            var record = new TraceRecord(request, category, level);
            traceAction(record);
        }
    }
}
#endif