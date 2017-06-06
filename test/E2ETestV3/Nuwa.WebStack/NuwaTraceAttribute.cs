using System;
using System.Web.Http.Tracing;

namespace Nuwa
{
    /// <summary>
    /// defining the trace writer adoption for the test. either always off,
    /// always on or specific type
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class NuwaTraceAttribute : Attribute
    {
        public NuwaTraceAttribute(Tag tag)
        {
            AlwaysOff = true;
        }

        public NuwaTraceAttribute(Type traceType)
        {
            if (traceType == null)
            {
                throw new ArgumentNullException("traceType");
            }

            if (!typeof(ITraceWriter).IsAssignableFrom(traceType))
            {
                throw new NuwaTraceAttributeException(traceType);
            }

            TraceWriter = traceType;
            AlwaysOff = false;
        }

        public enum Tag
        {
            Off
        }

        public bool AlwaysOff
        {
            get;
            private set;
        }

        public Type TraceWriter
        {
            get;
            private set;
        }
    }
}