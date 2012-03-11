using System.CodeDom.Compiler;

namespace System.Web.WebPages.Instrumentation
{
    [GeneratedCode("Microsoft.Web.CodeGen.DynamicCallerGenerator", "1.0.0.0")]
    internal partial class HttpContextAdapter
    {
        internal PageInstrumentationServiceAdapter PageInstrumentation
        {
            get { return new PageInstrumentationServiceAdapter((object)Adaptee.PageInstrumentation); }
        }

        // BEGIN Adaptor Infrastructure Code
        private static readonly Type _TargetType = typeof(HttpContext);
        internal dynamic Adaptee { get; private set; }

        internal HttpContextAdapter(object existing)
        {
            Adaptee = existing;
        }

        // END Adaptor Infrastructure Code
    }
}
