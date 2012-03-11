namespace Microsoft.Internal.Web.Utils
{
    internal interface IVirtualPathUtility
    {
        string Combine(string basePath, string relativePath);

        string ToAbsolute(string virtualPath);
    }
}
