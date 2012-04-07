// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;

namespace System.Web.Http.Tracing
{
    /// <summary>
    /// Test spy used internally to capture <see cref="TraceRecord"/>s.
    /// </summary>
    internal class TestTraceWriter : ITraceWriter
    {
        private List<TraceRecord> _traceRecords = new List<TraceRecord>();

        public IList<TraceRecord> Traces { get { return _traceRecords;  } }

        public bool IsEnabled(string category, TraceLevel level)
        {
            return true;
        }

        public void Trace(HttpRequestMessage request, string category, TraceLevel level, Action<TraceRecord> traceAction)
        {
            TraceRecord traceRecord = new TraceRecord(request, category, level);
            traceAction(traceRecord);
            lock (_traceRecords)
            {
                _traceRecords.Add(traceRecord);
            }
        }
    }
}
