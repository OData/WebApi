using System.Reflection;

namespace System.Web.WebPages.Instrumentation
{
    internal partial class HttpContextAdapter
    {
        private static readonly bool _isInstrumentationAvailable = typeof(HttpContext).GetProperty("PageInstrumentation", BindingFlags.Instance | BindingFlags.Public) != null;

        internal static bool IsInstrumentationAvailable
        {
            get { return _isInstrumentationAvailable; }
        }
    }
}
