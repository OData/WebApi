using NuGet;

namespace System.Web.WebPages.Administration.PackageManager
{
    public class WebPackageSource : PackageSource
    {
        public WebPackageSource(string source, string name)
            : base(source, name)
        {
        }

        public bool FilterPreferredPackages { get; set; }

        public override bool Equals(object obj)
        {
            WebPackageSource other = obj as WebPackageSource;
            return base.Equals(other) && FilterPreferredPackages == other.FilterPreferredPackages;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ (FilterPreferredPackages ? 1 : 0);
        }
    }
}
