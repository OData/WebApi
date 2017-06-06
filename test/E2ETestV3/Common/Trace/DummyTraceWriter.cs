using System;
using System.Net.Http;
using System.Web.Http.Tracing;

namespace WebStack.QA.Common.Trace
{
    public class DummyTraceWriter : ITraceWriter
    {
        public bool IsEnabled(string category, TraceLevel level)
        {
            return true;
        }

        public void Trace(HttpRequestMessage request, string category, TraceLevel level, Action<TraceRecord> traceAction)
        {
            var record = new TraceRecord(request, category, level);
            traceAction(record);
        }
    }
}
