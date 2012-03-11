namespace System.Web.Razor.Generator
{
    public class VBRazorCodeGenerator : RazorCodeGenerator
    {
        public VBRazorCodeGenerator(string className, string rootNamespaceName, string sourceFileName, RazorEngineHost host)
            : base(className, rootNamespaceName, sourceFileName, host)
        {
        }

        internal override Func<CodeWriter> CodeWriterFactory
        {
            get { return () => new VBCodeWriter(); }
        }
    }
}
