namespace System.Web.WebPages
{
    // Implemented by classes that can create object instances from virtual path.
    // Those implementations can completely bypass the BuildManager (e.g. for dynamic language pages)
    public interface IVirtualPathFactory
    {
        bool Exists(string virtualPath);
        object CreateInstance(string virtualPath);
    }
}
