using System.CodeDom.Compiler;
using System.IO;
using System.Linq.Expressions;

namespace System.Web.WebPages.Instrumentation
{
    [GeneratedCode("Microsoft.Web.CodeGen.DynamicCallerGenerator", "1.0.0.0")]
    internal partial class PageExecutionContextAdapter
    {
        internal bool IsLiteral
        {
            get { return Adaptee.IsLiteral; }
            set { Adaptee.IsLiteral = value; }
        }

        internal int Length
        {
            get { return Adaptee.Length; }
            set { Adaptee.Length = value; }
        }

        internal int StartPosition
        {
            get { return Adaptee.StartPosition; }
            set { Adaptee.StartPosition = value; }
        }

        internal TextWriter TextWriter
        {
            get { return Adaptee.TextWriter; }
            set { Adaptee.TextWriter = value; }
        }

        internal string VirtualPath
        {
            get { return Adaptee.VirtualPath; }
            set { Adaptee.VirtualPath = value; }
        }

        private static class _CallSite_ctor_1
        {
            public static Func<object> Site;

            static _CallSite_ctor_1()
            {
                Site = Expression.Lambda<Func<object>>(
                    Expression.New(_TargetType.GetConstructor(new Type[] { })))
                    .Compile();
            }
        }

        internal PageExecutionContextAdapter()
        {
            Adaptee = _CallSite_ctor_1.Site();
        }

        // BEGIN Adaptor Infrastructure Code
        private static readonly Type _TargetType = typeof(HttpContext).Assembly.GetType("System.Web.Instrumentation.PageExecutionContext");
        internal dynamic Adaptee { get; private set; }

        internal PageExecutionContextAdapter(object existing)
        {
            Adaptee = existing;
        }

        // END Adaptor Infrastructure Code
    }
}
