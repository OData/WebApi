using NuGet;

namespace System.Web.WebPages.Administration.PackageManager
{
    internal static class PackageExtensions
    {
        public static string GetDisplayName(this IPackage package)
        {
            string name = String.IsNullOrEmpty(package.Title) ? package.Id : package.Title;
            return String.Concat(name, ' ', package.Version);
        }
    }
}
