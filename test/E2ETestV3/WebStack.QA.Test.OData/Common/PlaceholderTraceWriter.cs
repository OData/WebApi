using System;
using System.Net.Http;
using System.Web.Http.Tracing;

namespace WebStack.QA.Test.OData.Common
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
