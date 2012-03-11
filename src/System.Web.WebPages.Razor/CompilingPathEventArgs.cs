namespace System.Web.WebPages.Razor
{
    public class CompilingPathEventArgs : EventArgs
    {
        public CompilingPathEventArgs(string virtualPath, WebPageRazorHost host)
        {
            VirtualPath = virtualPath;
            Host = host;
        }

        public string VirtualPath { get; private set; }
        public WebPageRazorHost Host { get; set; }
    }
}
