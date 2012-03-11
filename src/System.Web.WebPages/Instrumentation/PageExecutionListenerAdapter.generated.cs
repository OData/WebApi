using System.CodeDom.Compiler;

namespace System.Web.WebPages.Instrumentation
{
    [GeneratedCode("Microsoft.Web.CodeGen.DynamicCallerGenerator", "1.0.0.0")]
    internal partial class PageExecutionListenerAdapter
    {
        internal void BeginContext(PageExecutionContextAdapter context)
        {
            Adaptee.BeginContext(context.Adaptee);
        }

        internal void EndContext(PageExecutionContextAdapter context)
        {
            Adaptee.EndContext(context.Adaptee);
        }

        // BEGIN Adaptor Infrastructure Code
        private static readonly Type _TargetType = typeof(HttpContext).Assembly.GetType("System.Web.Instrumentation.PageExecutionListener");
        internal dynamic Adaptee { get; private set; }

        internal PageExecutionListenerAdapter(object existing)
        {
            Adaptee = existing;
        }

        // END Adaptor Infrastructure Code
    }
}
