namespace Microsoft.Web.Helpers
{
    public abstract class VirtualPathUtilityBase
    {
        public abstract string Combine(string basePath, string relativePath);

        public abstract string ToAbsolute(string virtualPath);
    }
}
